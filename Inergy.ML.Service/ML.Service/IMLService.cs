using Inergy.ML.Model.ML;
using Inergy.Tools.Architecture.Model;
using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inergy.ML.Service
{
    public interface IMLService
    {

        Task<IEnumerable<ForecastOutput>> GetPredictedValues(string cups, int horizon);

        Task<IEnumerable<ModelInput>> GetDataView(string cups);

        EvaluateMetrics TrainAndSaveModel(IEnumerable<ModelInput> modelInputs, string modelPath);

        IEnumerable<ForecastOutput> PredictModel(IEnumerable<ModelInput> modelInputs, int horizon, string modelPath);
    }
}
