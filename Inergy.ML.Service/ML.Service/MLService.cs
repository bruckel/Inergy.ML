using Inergy.ML.Data;
using Inergy.Tools.Architecture.Model;
using MongoDB.Driver;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using Inergy.Tools.Utils.Extension;
using Microsoft.ML;
using Inergy.ML.Model.ML;
using Microsoft.ML.Transforms.TimeSeries;
using System.IO;
using System.Threading.Tasks;

namespace Inergy.ML.Service
{
    public class MLService : IMLService
    {
        private readonly IDataReadingRepository dataReadingRepository;
        private readonly ILogger log;
        private readonly MLContext mlContext;

        /// <summary>
        /// Cosntructor
        /// </summary>
        /// <param name="dataReadingRepository">Repositorio Mongo</param>
        /// <param name="log">Servicio de Logging</param>
        public MLService(IDataReadingRepository dataReadingRepository, ILogger log, MLContext mlContext)
        {
            this.dataReadingRepository = dataReadingRepository;
            this.log = log;
            this.mlContext = mlContext;
        }

        public async Task<IEnumerable<ForecastOutput>> GetPredictedValues(string cups, int horizon)
        {
            //string rootDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../Inergy.ML.Models/ForecastBySsa"));
            string rootDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../"));
            string modelPath = Path.Combine(rootDir, $"{cups}_daily_timeSeriesSSA.zip");

            //* Cargar el modelo en el caso de que ya exista *//
            bool createModel = !File.Exists(modelPath);

            var modelInputs = await GetDataView(cups);

            if (createModel)
            {
                //* Guardar métricas en el log de la aplicación *//
                var metrics = TrainAndSaveModel(modelInputs, modelPath);
            }

            //* Realizar predicción en base al modelo *//
            return PredictModel(modelInputs, horizon, modelPath);
        }
        
        public async Task<IEnumerable<ModelInput>> GetDataView(string cups)
        {
            //string rootDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../Inergy.ML.Models"));

            var result = await this.dataReadingRepository.GetDataReadings(cups);

            //* Agrupar consumo por días *//
            var modelInputs = result.GroupBy(r => r.TimeStamp.Date).Select(r => new ModelInput
            {
                ConsumDate = r.Select(p => p.TimeStamp.Date).FirstOrDefault(),
                Periode = Convert.ToInt32(r.Select(p => String.Concat(p.TimeStamp.Year.ToString(), 
                                                    p.TimeStamp.Month.ToString("D2"), 
                                                    p.TimeStamp.Day.ToString("D2"))).FirstOrDefault()),
                TotalConsum = (float) r.Sum(p => p.Value)
            })
            .OrderBy(r => r.ConsumDate);

            return modelInputs;
        }

        public EvaluateMetrics TrainAndSaveModel(IEnumerable<ModelInput> modelInputs, string modelPath)
        {
            var horizon = 31;
            var dataView = mlContext.Data.LoadFromEnumerable<ModelInput>(modelInputs);

            var lastDate = modelInputs.Max(p => p.ConsumDate).AddDays(-(horizon-1)).Date;
            var bound = modelInputs.Where(m => m.ConsumDate.Date == lastDate).Select(m => m.Periode).FirstOrDefault();

            var trainData = mlContext.Data.FilterRowsByColumn(dataView, "Periode", upperBound: bound);
            var testData = mlContext.Data.FilterRowsByColumn(dataView, "Periode", lowerBound: bound);

            var uno = this.mlContext.Data.CreateEnumerable<ModelInput>(trainData, false);
            var dos = this.mlContext.Data.CreateEnumerable<ModelInput>(testData, false);

            //* Número de días del conjunto de entrenamiento *//
            var numSeriesDataPoints = mlContext.Data.CreateEnumerable<ModelInput>(trainData, reuseRowObject: true).Count();

            var forecastingPipeline = mlContext.Forecasting.ForecastBySsa(
                outputColumnName: "ForecastedConsum",
                inputColumnName: "TotalConsum",
                windowSize: 7,
                seriesLength: 31,
                trainSize: numSeriesDataPoints, 
                horizon: horizon,
                confidenceLevel: 0.98f,
                confidenceLowerBoundColumn: "LowerBoundConsum",
                confidenceUpperBoundColumn: "UpperBoundConsum");

            var forecastTransformer = forecastingPipeline.Fit(trainData);

            var forecastEngine = forecastTransformer.CreateTimeSeriesEngine<ModelInput, ModelOutput>(mlContext);
            forecastEngine.CheckPoint(mlContext, modelPath);
            
            // Make predictions
            IDataView predictions = forecastTransformer.Transform(testData);

            // Actual values
            IEnumerable<float> actual = this.mlContext.Data.CreateEnumerable<ModelInput>(testData, true).Select(observed => observed.TotalConsum);

            // Predicted values
            IEnumerable<float> forecast = this.mlContext.Data.CreateEnumerable<ModelOutput>(predictions, true).Select(prediction => prediction.ForecastedConsum[0]);

            // Calculate error (actual - forecast)
            var metrics = actual.Zip(forecast, (actualValue, forecastValue) => actualValue - forecastValue);

            // Get metric averages
            return new EvaluateMetrics
            {
                MAE = metrics.Average(error => Math.Abs(error)),
                RMSE = Math.Sqrt(metrics.Average(error => Math.Pow(error, 2)))
            };
        }

        public IEnumerable<ForecastOutput> PredictModel(IEnumerable<ModelInput> modelInputs, int horizon, string modelPath)
        {
            var nextMonthValues = modelInputs.TakeLast(horizon);

            // Load the forecast engine that has been previously saved.
            ITransformer forecaster;

            using (var file = File.OpenRead(modelPath))
            {
                forecaster = mlContext.Model.Load(file, out DataViewSchema schema);
            }

            // We must create a new prediction engine from the persisted model.
            TimeSeriesPredictionEngine<ModelInput, ModelOutput> forecastEngine = forecaster.CreateTimeSeriesEngine<ModelInput, ModelOutput>(mlContext);

            var modelOutput = forecastEngine.Predict();

            var forecastOutputs = nextMonthValues.Select((n, i) => new ForecastOutput
            {
                Date = n.ConsumDate,
                Day = n.ConsumDate.Day,
                ActualValue = n.TotalConsum,
                LowerEstimate = modelOutput.LowerBoundConsum[i],
                Estimate = modelOutput.ForecastedConsum[i],
                UpperEstimate = modelOutput.UpperBoundConsum[i],
            });

            return forecastOutputs;
        }
    }
}
