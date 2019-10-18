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

        //public async Task<IEnumerable<DataReading>> UpdateDataReadings(IEnumerable<DataReading> dataReadings)
        //{
        //    try
        //    {
        //        //dataReadings.AsParallel().ForAll(async d =>
        //        //{
        //        //    var t = "hola";
        //        //});


        //        //* Filtro para obtener datos para el suministro indicado y entre las horas especificadas *//
        //        //            var dataReadings = this.GetAll().UpdateMany<DataReading>(e => e.Cups == cups
        //        //                                                                        && e.TimeStamp >= beginTimeStamp
        //        //                                                                        && e.TimeStamp <= endTimeStamp, );


        //        //            residenceCollection.UpdateMany(x =>
        //        //x.City == "Stockholm",
        //        //Builders<Residence>.Update.Set(p => p.Municipality, "Stoholms län"),
        //        //new UpdateOptions { IsUpsert = false }
        //        //);

        //        //* Retornar valores coincidentes *//
        //        //return await dataReadings.ToList();
        //    }
        //    catch (Exception exception)
        //    {
        //        throw (exception);
        //    }
        //}
    }
}