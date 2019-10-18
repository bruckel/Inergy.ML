using Inergy.ML.Model;
using System;
using System.Collections.Generic;

namespace Inergy.ML.Service.Cosmos
{
    public interface IDataReadingService
    {
        void InsertDataReadings(IEnumerable<DataReading> dataReadings);

        void UpdateDataReadings(IEnumerable<DataReading> dataReadings);

        void DeleteDataReadings(string cups, DateTime beginTimeStamp, DateTime endTimeStamp);
    }
}
