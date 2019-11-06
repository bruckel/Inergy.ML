using Inergy.Tools.Architecture.Data.Mongo;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Inergy.Tools.Architecture.Model;

namespace Inergy.ML.Data
{
    public partial class DataReadingRepository : MongoRepository<DataReading>, IDataReadingRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Contexto de la BB.DD. Mongo</param>
        public DataReadingRepository(IMongoContext context) : base(context, "DataReadings")
        {

        }

        /// <summary>
        /// Obtener lecturas por identificador y rango de fechas
        /// </summary>
        /// <param name="cups">Identificador</param>
        /// <param name="beginTimeStamp">Fecha de inicio</param>
        /// <param name="endTimeStamp">Fecha de fin</param>
        /// <returns>Resultado de la consulta</returns>
        public async Task<IEnumerable<DataReading>> GetDataReadings(string cups, DateTime beginTimeStamp, DateTime endTimeStamp)
        {
            try
            {
                //* Filtro para obtener datos para el suministro indicado y entre las horas especificadas *//
                var dataReadings = this.GetAll().Find<DataReading>(e => e.Cups == cups
                                                                        && e.TimeStamp >= beginTimeStamp
                                                                        && e.TimeStamp <= endTimeStamp);


                //* Retornar valores coincidentes *//
                return await dataReadings.ToListAsync();
            }
            catch (Exception exception)
            {
                throw (exception);
            }
        }

        /// <summary>
        /// Insertar lecturas
        /// </summary>
        /// <param name="dataReadings">Lecturas</param>
        /// <returns>Resultado de la insercción</returns>
        public async Task<bool> CreateDataReadings(IEnumerable<DataReading> dataReadings)
        {
            try
            {
                return await Task.Run(() => this.Add(dataReadings));
            }
            catch (Exception exception)
            {
                throw (exception);
            }
        }

        /// <summary>
        /// Actualizar lecturas
        /// </summary>
        /// <param name="dataReadings">lecturas</param>
        /// <returns>Resultado de la actualización</returns>
        public async Task<IEnumerable<UpdateResult>> UpdateDataReadings(IEnumerable<DataReading> dataReadings)
        {
            try
            {
                List<UpdateResult> updateResults = new List<UpdateResult>();

                //* Actualizar valores, y si no existe, insertar *//
                dataReadings.AsParallel().ForAll(async d =>
                {
                    //* Upsert indica que si el registro de lectura no existe se añade *//
                    var updateResult = await this.GetAll().UpdateOneAsync<DataReading>(r => r.Cups == d.Cups && r.TimeStamp == d.TimeStamp, 
                                                                                        Builders<DataReading>.Update.Set(p => p.Value, d.Value), 
                                                                                        new UpdateOptions { IsUpsert = true });

                    updateResults.Add(updateResult);
                });

                return await Task.Run(() => updateResults);
            }
            catch (Exception exception)
            {
                throw (exception);
            }
        }

        /// <summary>
        /// Eliminar lecturas en función de indentificador y rango de fechas
        /// </summary>
        /// <param name="cups">Identificador</param>
        /// <param name="beginTimeStamp">Fecha de inicio</param>
        /// <param name="endTimeStamp">Fecha de fin</param>
        /// <returns>Resultado de la eliminación</returns>
        public async Task<DeleteResult> DeleteDataReadings(string cups, DateTime beginTimeStamp, DateTime endTimeStamp)
        {
            try
            {
                //* Filtro para obtener datos para el suministro indicado y entre las horas especificadas *//
                return await this.GetAll().DeleteManyAsync<DataReading>(e => e.Cups == cups
                                                                        && e.TimeStamp >= beginTimeStamp
                                                                        && e.TimeStamp <= endTimeStamp);
            }
            catch (Exception exception)
            {
                throw (exception);
            }
        }
    }
}