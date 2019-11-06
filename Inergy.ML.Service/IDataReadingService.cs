using Inergy.Tools.Architecture.Model;
using System;
using System.Collections.Generic;

namespace Inergy.ML.Service.Cosmos
{
    public interface IDataReadingService
    {
        IEnumerable<DataReading> GetDataReadings(string cups, DateTime beginTimeStamp, DateTime endTimeStamp);

        void CreateDataReadings(IEnumerable<DataReading> dataReadings);

        void UpdateDataReadings(IEnumerable<DataReading> dataReadings);

        void DeleteDataReadings(string cups, DateTime beginTimeStamp, DateTime endTimeStamp);
    }
}
