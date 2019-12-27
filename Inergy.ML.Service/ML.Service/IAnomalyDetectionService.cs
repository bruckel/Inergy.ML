using Inergy.ML.Model.ML;
using Inergy.Tools.Architecture.Model;
using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inergy.ML.Service
{
    public interface IAnomalyDetectionService
    {
        Task<IEnumerable<Prediction>> GetPredictedValues(string cups);
    }
}
