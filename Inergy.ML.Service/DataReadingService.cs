using Inergy.ML.Data;
using Inergy.ML.Model;
using Inergy.Tools.Architecture.Data.Mongo;
using MongoDB.Driver;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inergy.ML.Service.Cosmos
{
    public class DataReadingService : IDataReadingService
    {
        private readonly IDataReadingRepository dataReadingRepository;
        private readonly ILogger log;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString">Cadena de conexión</param>
        /// <param name="database">Base de datos</param>
        public DataReadingService(string connectionString, string database) : this(new DataReadingRepository(new MongoContext(connectionString, database), "LoadCurves"))
        {
            this.log = new LoggerConfiguration().WriteTo.File("logs\\LoadCurvesLog.txt", rollingInterval: RollingInterval.Day).CreateLogger();
        }

        /// <summary>
        /// Cosntructor
        /// </summary>
        /// <param name="dataReadingRepository">Repositorio Mongo</param>
        public DataReadingService(IDataReadingRepository dataReadingRepository)
        {
            this.dataReadingRepository = dataReadingRepository;
        }

        /// <summary>
        /// Obtener series temporales por intervalo de fecha
        /// </summary>
        /// <param name="cups">Identificador de cups</param>
        /// <param name="beginTimeStamp">Fecha de inicio</param>
        /// <param name="endTimeStamp">Fecha de fin</param>
        /// <returns>Enumerable de series temporales</returns>
        public IEnumerable<DataReading> GetDataReadings(string cups, DateTime beginTimeStamp, DateTime endTimeStamp)
        {
            try
            {
                return this.dataReadingRepository.GetDataReadings(cups, beginTimeStamp, endTimeStamp).Result.Select(r =>
                {
                    r.TimeStamp = r.TimeStamp.ToLocalTime();
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
        public void InsertDataReadings(IEnumerable<DataReading> dataReadings)
        {
            try
            {
                //* Trabajamos por defecto con UTC *//
                var groupedDataReading = dataReadings.GroupBy(d => d.Cups, (cups, data) => new
                {
                    Cups = cups,
                    BeginTimeStamp = data.Min(m => m.TimeStamp).ToUniversalTime(),
                    EndTimeStamp = data.Max(m => m.TimeStamp).ToUniversalTime(),
                    Data = data.Select(f =>
                    {
                        //* Conversión obligatoria a UTC *//
                        f.TimeStamp = f.TimeStamp.ToUniversalTime();
                        return f;
                    })
                });

                //* Procesar datos agrupado spor cups *//
                groupedDataReading.AsParallel().ForAll(async g =>
                {
                    var currentDataReadings = await this.dataReadingRepository.GetDataReadings(g.Cups, g.BeginTimeStamp, g.EndTimeStamp);

                    if (!currentDataReadings.Any())
                    {
                        var result = await this.dataReadingRepository.InsertDataReadings(g.Data);

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
        public void UpdateDataReadings(IEnumerable<DataReading> dataReadings)
        {
            try
            {
                var groupedDataReading = dataReadings.GroupBy(d => d.Cups, (cups, data) => new
                {
                    Cups = cups,
                    Data = data.Select(f =>
                    {
                        //* Conversión obligatoria a UTC *//
                        f.TimeStamp = f.TimeStamp.ToUniversalTime();
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
                throw (exception);
            }
        }

        /// <summary>
        /// Método para la eliminación de series teporales por cups e intervalo
        /// </summary>
        /// <param name="cups">Identificador cups</param>
        /// <param name="beginTimeStamp">Fecha/hora de inicio</param>
        /// <param name="endTimeStamp">Fech /hora de fin</param>
        public void DeleteDataReadings(string cups, DateTime beginTimeStamp, DateTime endTimeStamp)
        {
            try
            {
                var count = this.dataReadingRepository.DeleteDataReadings(cups, beginTimeStamp.ToUniversalTime(), endTimeStamp.ToUniversalTime()).Result.DeletedCount;

                log.Information("{Cups} - Total data deleted between {beginTimeStamp} and {endTimeStamp}: {Count}", cups, beginTimeStamp, endTimeStamp, count);
            }
            catch (Exception exception)
            {
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
