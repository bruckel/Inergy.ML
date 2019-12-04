using Inergy.Tools.Architecture.Data.Mongo;
using Inergy.Tools.Architecture.Model;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inergy.ML.Data
{
    public interface IDataReadingRepository : IMongoRepository<DataReading>
    {
        Task<IEnumerable<DataReading>> GetDataReadings(string cups, DateTime beginTimeStamp, DateTime endTimeStamp);

        Task<bool> CreateDataReadings(IEnumerable<DataReading> dataReadings);

        Task<DeleteResult> DeleteDataReadings(string cups, DateTime beginTimeStamp, DateTime endTimeStamp);
    }
}