using Inergy.ML.Model;
using Inergy.Tools.Architecture.Data.Mongo;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inergy.ML.Data
{
    public interface IDataReadingRepository : IMongoRepository<DataReading>
    {
        Task<IEnumerable<DataReading>> GetDataReadings(string cups, DateTime beginTimeStamp, DateTime endTimeStamp);
    }
}