using Inergy.ML.Model;
using Inergy.Tools.Architecture.Data.Mongo;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Inergy.ML.Data
{
    public partial class DataReadingRepository : MongoRepository<DataReading>, IDataReadingRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Contexto de la BB.DD. Mongo</param>
        public DataReadingRepository(IMongoContext context, string collectionName) : base(context, collectionName)
        {

        }

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

        public IEnumerable<UpdateResult> UpdateDataReadings(IEnumerable<DataReading> dataReadings)
        {
            try
            {
                List<UpdateResult> updateResults = new List<UpdateResult>();

                //* Actualizar valores, y si no existe, insertar *//
                dataReadings.AsParallel().ForAll(d =>
                {
                    updateResults.Add(this.GetAll().UpdateOne<DataReading>(r => r.Cups == d.Cups && r.TimeStamp == d.TimeStamp, 
                                                                            Builders<DataReading>.Update.Set(p => p.Value, d.Value), 
                                                                            new UpdateOptions { IsUpsert = true }));
                });

                return updateResults;
            }
            catch (Exception exception)
            {
                throw (exception);
            }
        }

        public DeleteResult DeleteDataReadings(string cups, DateTime beginTimeStamp, DateTime endTimeStamp)
        {
            try
            {
                //* Filtro para obtener datos para el suministro indicado y entre las horas especificadas *//
                return this.GetAll().DeleteMany<DataReading>(e => e.Cups == cups
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