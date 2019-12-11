using Inergy.Tools.Architecture.Model;
using System;
using System.Collections.Generic;

namespace Inergy.ML.Service
{
    public interface IDataReadingService
    {
        IEnumerable<DataReading> GetDataReadings(string cups, DateTime dateBegin, DateTime dateEnd, string timeZone);

        void CreateDataReadings(IEnumerable<DataReading> dataReadings, string timeZone);

        void UpdateDataReadings(IEnumerable<DataReading> dataReadings, string timeZone);

        void DeleteDataReadings(string cups, DateTime dateBegin, DateTime dateEnd, string timeZone);
    }
}
