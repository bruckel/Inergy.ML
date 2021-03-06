﻿using Inergy.ML.Data;
using Inergy.Tools.Architecture.Model;
using MongoDB.Driver;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using Inergy.Tools.Utils.Extension;

namespace Inergy.ML.Service
{
    public class DataReadingService : IDataReadingService
    {
        private readonly IDataReadingRepository dataReadingRepository;
        private readonly ILogger log;

        /// <summary>
        /// Cosntructor
        /// </summary>
        /// <param name="dataReadingRepository">Repositorio Mongo</param>
        /// <param name="log">Servicio de Logging</param>
        public DataReadingService(IDataReadingRepository dataReadingRepository, ILogger log)
        {
            this.dataReadingRepository = dataReadingRepository;
            this.log = log;
        }

        /// <summary>
        /// Obtener series temporales por intervalo de fecha
        /// </summary>
        /// <param name="cups">Identificador de cups</param>
        /// <param name="dateBegin">Fecha de inicio</param>
        /// <param name="dateEnd">Fecha de fin</param>
        /// <param name="timeZone">Zona horaria</param>
        /// <returns>Enumerable de series temporales</returns>
        public IEnumerable<DataReading> GetDataReadings(string cups, DateTime dateBegin, DateTime dateEnd, string timeZone)
        {
            try
            {
                return this.dataReadingRepository.GetDataReadings(cups, dateBegin, dateEnd).Result;
            }
            catch (Exception exception)
            {
                log.Error(exception, "Error while inserting data");

                throw (exception);
            }
        }

        /// <summary>
        /// Método para insertar series temporales
        /// </summary>
        /// <param name="dataReadings">Series temporales</param>
        /// <param name="timeZone">Zona horaria</param>
        public void CreateDataReadings(IEnumerable<DataReading> dataReadings, string timeZone)
        {
            try
            {
                //* Trabajamos por defecto con UTC *//
                var groupedDataReading = dataReadings.GroupBy(d => d.Cups, (cups, data) => new
                {
                    Cups = cups,
                    BeginTimeStamp = data.Min(m => m.TimeStamp),
                    EndTimeStamp = data.Max(m => m.TimeStamp),
                    Data = data.Select(f =>
                    {
                        //* Conversión obligatoria a UTC *//
                        f.TimeOffset = NodaDateTime.GetUtcOffset(f.TimeStamp, timeZone);

                        return f;
                    })
                });

                //* Procesar datos agrupado spor cups *//
                groupedDataReading.AsParallel().ForAll(async g =>
                {
                    bool status = true;
                    var currentDataReadings = await this.dataReadingRepository.GetDataReadings(g.Cups, g.BeginTimeStamp, g.EndTimeStamp);

                    if (currentDataReadings.Any())
                    {
                        var deleteResult = await this.dataReadingRepository.DeleteDataReadings(g.Cups, g.BeginTimeStamp, g.EndTimeStamp);

                        //* Si no se elimina correctamente, abortamos la creación de los datos *//
                        status = deleteResult.IsAcknowledged;

                        LogDeleteDataReadings(deleteResult, g.Cups, g.BeginTimeStamp, g.EndTimeStamp);
                    }

                    if (status)
                    {
                        var result = await this.dataReadingRepository.CreateDataReadings(g.Data);

                        var logObject = new
                        {
                            g.Cups,
                            Count = g.Data.Count()
                        };

                        if (result)
                        {
                            log.Information("{Cups} - Total data inserted: {Count}", logObject.Cups, logObject.Count);
                        }
                        else
                        {
                            log.Warning("{Cups} - Total data not inserted: {Count}", logObject.Cups, logObject.Count);
                        }
                    }
                });
            }
            catch (Exception exception)
            {
                log.Error(exception, "Error while inserting data");

                throw (exception);
            }
        }

        /// <summary>
        /// Método para actualizar las series temporales
        /// </summary>
        /// <param name="dataReadings">Colección de series temporales</param>
        /// <param name="timeZone">Zona horaria</param>
        public void UpdateDataReadings(IEnumerable<DataReading> dataReadings, string timeZone)
        {
            try
            {
                var groupedDataReading = dataReadings.GroupBy(d => d.Cups, (cups, data) => new
                {
                    Cups = cups,
                    BeginTimeStamp = data.Min(m => m.TimeStamp),
                    EndTimeStamp = data.Max(m => m.TimeStamp),
                    Data = data.Select(f =>
                    {
                        //* Conversión obligatoria a UTC *//
                        f.TimeOffset = NodaDateTime.GetUtcOffset(f.TimeStamp, timeZone);
                        return f;
                    })
                });

                groupedDataReading.AsParallel().ForAll(async g =>
                {
                    bool status = true;
                    var deleteResult = await this.dataReadingRepository.DeleteDataReadings(g.Cups, g.BeginTimeStamp, g.EndTimeStamp);

                    //* Si no se elimina correctamente, abortamos la creación de los datos *//
                    status = deleteResult.IsAcknowledged;

                    LogDeleteDataReadings(deleteResult, g.Cups, g.BeginTimeStamp, g.EndTimeStamp);

                    if (status)
                    {
                        var result = await this.dataReadingRepository.CreateDataReadings(g.Data);

                        var logObject = new
                        {
                            g.Cups,
                            Count = g.Data.Count()
                        };

                        if (result)
                        {
                            log.Information("{Cups} - Total data inserted: {Count}", logObject.Cups, logObject.Count);
                        }
                        else
                        {
                            log.Warning("{Cups} - Total data not inserted: {Count}", logObject.Cups, logObject.Count);
                        }
                    }
                });
            }
            catch (Exception exception)
            {
                log.Error(exception, "Error while updating data");

                throw (exception);
            }
        }

        /// <summary>
        /// Método para la eliminación de series teporales por cups e intervalo
        /// </summary>
        /// <param name="cups">Identificador cups</param>
        /// <param name="dateBegin">Fecha de inicio</param>
        /// <param name="dateEnd">Fecha de fin</param>
        /// <param name="timeZone">Zona horaria</param>
        public void DeleteDataReadings(string cups, DateTime dateBegin, DateTime dateEnd, string timeZone)
        {
            try
            {
                //* Convertir fechas a UTC para la consulta en Cosmos DB *//
                var deleteResult = this.dataReadingRepository.DeleteDataReadings(cups, dateBegin, dateEnd).Result;

                LogDeleteDataReadings(deleteResult, cups, dateBegin, dateEnd);
            }
            catch (Exception exception)
            {
                log.Error(exception, "Error while deleting data");

                throw (exception);
            }
        }

        /// <summary>
        /// Método para el log de los resultados de la actualización de datos temporales
        /// </summary>
        /// <param name="result">Objeto resultado Mongo</param>
        /// <param name="cups">identificador cups</param>
        /// <param name="count">Número de registros a actualizar</param>
        private void LogDeleteDataReadings(DeleteResult result, string cups, DateTime dateBegin, DateTime dateEnd)
        {
            //* Registro de datos no actualizdos correctamente *//
            if (!result.IsAcknowledged)
            {
                log.Warning("{Cups} - Total data not deleted: {DeletedCount}", cups, result.DeletedCount);
            }
            else
            {
                //* Registro de datos temporales actualizados correctamente *//
                log.Information("{Cups} - Total data deleted between {beginTimeStamp} and {endTimeStamp}: {DeletedCount}", cups, dateBegin, dateEnd, result.DeletedCount);
            }
        }
    }
}
