using Inergy.ML.Data;
using Inergy.Tools.Architecture.Model;
using MongoDB.Driver;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using Inergy.Tools.Utils.Extension;

namespace Inergy.ML.Service.Cosmos
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
                //* Convertir fechas a UTC para la consulta en Cosmos DB *//
                var utcDateBegin = NodaDateTime.GetUtcDateTime(DateTime.SpecifyKind(dateBegin, DateTimeKind.Unspecified), timeZone);
                var utcDateEnd = NodaDateTime.GetUtcDateTime(DateTime.SpecifyKind(dateEnd, DateTimeKind.Unspecified), timeZone);

                return this.dataReadingRepository.GetDataReadings(cups, utcDateBegin, utcDateEnd).Result.Select(r =>
                {
                    //* Convertir hora Utc en la hora local especificada en su momento por su offset *//
                    r.TimeStamp = NodaDateTime.GetOffsetDateTime(r.TimeStamp, r.TimeOffset);
                    return r;
                });
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
                    BeginTimeStamp = NodaDateTime.GetUtcDateTime(DateTime.SpecifyKind(data.Min(m => m.TimeStamp), DateTimeKind.Unspecified), timeZone),
                    EndTimeStamp = NodaDateTime.GetUtcDateTime(DateTime.SpecifyKind(data.Max(m => m.TimeStamp), DateTimeKind.Unspecified), timeZone),
                    Data = data.Select(f =>
                    {
                        //* Conversión obligatoria a UTC *//
                        f.TimeStamp = NodaDateTime.GetUtcDateTime(DateTime.SpecifyKind(f.TimeStamp, DateTimeKind.Unspecified), timeZone);
                        f.TimeOffset = NodaDateTime.GetUtcOffset(DateTime.SpecifyKind(f.TimeStamp, DateTimeKind.Unspecified), timeZone);
                        return f;
                    })
                });

                //* Procesar datos agrupado spor cups *//
                groupedDataReading.AsParallel().ForAll(async g =>
                {
                    var currentDataReadings = await this.dataReadingRepository.GetDataReadings(g.Cups, g.BeginTimeStamp, g.EndTimeStamp);

                    if (!currentDataReadings.Any())
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
                    else
                    {
                        var result = await this.dataReadingRepository.UpdateDataReadings(g.Data);

                        //* Logging de los resultados de la actualización *//
                        LogUpdateDataReadings(result, g.Cups, g.Data.Count());
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
                    Data = data.Select(f =>
                    {
                        //* Conversión obligatoria a UTC *//
                        f.TimeStamp = NodaDateTime.GetUtcDateTime(DateTime.SpecifyKind(data.Max(m => m.TimeStamp), DateTimeKind.Unspecified), timeZone);
                        f.TimeOffset = NodaDateTime.GetUtcOffset(DateTime.SpecifyKind(f.TimeStamp, DateTimeKind.Unspecified), timeZone);
                        return f;
                    })
                });

                groupedDataReading.AsParallel().ForAll(async g =>
                {
                    var result = await this.dataReadingRepository.UpdateDataReadings(g.Data);

                    //* Logging de los resultados de la actualización *//
                    LogUpdateDataReadings(result, g.Cups, g.Data.Count());
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
                var utcDateBegin = NodaDateTime.GetUtcDateTime(DateTime.SpecifyKind(dateBegin, DateTimeKind.Unspecified), timeZone);
                var utcDateEnd = NodaDateTime.GetUtcDateTime(DateTime.SpecifyKind(dateEnd, DateTimeKind.Unspecified), timeZone);

                var count = this.dataReadingRepository.DeleteDataReadings(cups, utcDateBegin, utcDateEnd).Result.DeletedCount;

                log.Information("{Cups} - Total data deleted between {beginTimeStamp} and {endTimeStamp}: {Count}", cups, dateBegin, dateEnd, count);
            }
            catch (Exception exception)
            {
                log.Error(exception, "Error while deleting data");

                throw (exception);
            }
        }

        /// <summary>
        /// Método para el logggin de los resultados de la actualización de datos temporales
        /// </summary>
        /// <param name="result">Objeto resultado Mongo</param>
        /// <param name="cups">identificador cups</param>
        /// <param name="count">Número de registros a actualizar</param>
        private void LogUpdateDataReadings(IEnumerable<UpdateResult> result, string cups, int count)
        {
            var logObject = new
            {
                Cups = cups,
                Count = count,
                MatchedCount = result.Where(r => r.IsAcknowledged).Sum(r => r.MatchedCount),
                ModifiedCount = result.Where(r => r.IsAcknowledged).Sum(r => r.ModifiedCount),
                ErrorCount = result.Where(r => !r.IsAcknowledged).Count()
            };

            //* Registro de datos temporales actualizados correctamente *//
            log.Information("{Cups} - Total data: {Count}, total matched: {MatchedCount} and total modified {ModifiedCount}",
                            logObject.Cups, logObject.Count, logObject.MatchedCount, logObject.ModifiedCount);

            //* Registro de datos no actualizdos correctamente *//
            if (result.Where(r => !r.IsAcknowledged).Any())
            {
                log.Warning("{Cups} - Total data not updated: {ErrorCount}", logObject.Cups, logObject.ErrorCount);
            }
        }
    }
}
