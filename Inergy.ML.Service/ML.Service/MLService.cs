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

        public void RunML(string cups, DateTime dateBegin, DateTime dateEnd)
        {
            string rootDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../"));
            string modelPath = Path.Combine(rootDir, "MLModel.zip");

            var data = GetDataViews(cups, dateBegin, dateEnd);

            var transformer = TrainModel(data.Item1);

            EvaluateModel(data.Item2, transformer);

            SaveModel(transformer, modelPath);

        }

        private (IDataView, IDataView) GetDataViews(string cups, DateTime dateBegin, DateTime dateEnd)
        {
            var result = this.dataReadingRepository.GetDataReadings(cups, dateBegin, dateEnd).Result;

            //* Agrupar consumo por días *//
            var modelInputs = result.GroupBy(r => r.TimeStamp.DayOfYear).Select(r => new ModelInput
            {
                ConsumDate = r.Select(p => p.TimeStamp).FirstOrDefault(),
                Year = r.Select(p => p.TimeStamp.Year).FirstOrDefault(),
                TotalConsum = (float) r.Sum(p => p.Value)
            });

            var dataView = mlContext.Data.LoadFromEnumerable<ModelInput>(modelInputs);
            
            IDataView firstYearData = mlContext.Data.FilterRowsByColumn(dataView, "Year", upperBound: dateBegin.Year);
            IDataView secondYearData = mlContext.Data.FilterRowsByColumn(dataView, "Year", lowerBound: dateBegin.Year);

            return (firstYearData, secondYearData);
        }

        private ITransformer TrainModel(IDataView trainData)
        {
            var forecastingPipeline = mlContext.Forecasting.ForecastBySsa(
                outputColumnName: "ForecastedConsum",
                inputColumnName: "TotalConsum",
                windowSize: 7,
                seriesLength: 30,
                trainSize: 365,
                horizon: 7,
                confidenceLevel: 0.95f,
                confidenceLowerBoundColumn: "LowerBoundConsum",
                confidenceUpperBoundColumn: "UpperBoundConsum");

            SsaForecastingTransformer forecaster = forecastingPipeline.Fit(trainData);

            return forecaster;
        }

        private void EvaluateModel(IDataView testData, ITransformer model)
        {
            // Make predictions
            IDataView predictions = model.Transform(testData);

            // Actual values
            IEnumerable<float> actual = this.mlContext.Data.CreateEnumerable<ModelInput>(testData, true).Select(observed => observed.TotalConsum);

            // Predicted values
            IEnumerable<float> forecast = this.mlContext.Data.CreateEnumerable<ModelOutput>(predictions, true).Select(prediction => prediction.ForecastedConsum[0]);

            // Calculate error (actual - forecast)
            var metrics = actual.Zip(forecast, (actualValue, forecastValue) => actualValue - forecastValue);

            // Get metric averages
            var MAE = metrics.Average(error => Math.Abs(error)); // Mean Absolute Error
            var RMSE = Math.Sqrt(metrics.Average(error => Math.Pow(error, 2))); // Root Mean Squared Error

            // Output metrics

            //* Esto no debe de estar en un service *//

            Console.WriteLine("Evaluation Metrics");
            Console.WriteLine("---------------------");
            Console.WriteLine($"Mean Absolute Error: {MAE:F3}");
            Console.WriteLine($"Root Mean Squared Error: {RMSE:F3}\n");
        }

        private void SaveModel(ITransformer model, string modelPath)
        {
            var forecastEngine = model.CreateTimeSeriesEngine<ModelInput, ModelOutput>(mlContext);
            forecastEngine.CheckPoint(mlContext, modelPath);
        }

        private void ForecastModel(IDataView testData, int horizon, TimeSeriesPredictionEngine<ModelInput, ModelOutput> forecaster)
        {

            ModelOutput forecast = forecaster.Predict();

            IEnumerable<string> forecastOutput = this.mlContext.Data.CreateEnumerable<ModelInput>(testData, reuseRowObject: false)
                    .Take(horizon)
                    .Select((ModelInput consum, int index) =>
                    {
                        string rentalDate = consum.ConsumDate.ToShortDateString();
                        float actualConsum = consum.TotalConsum;
                        float lowerEstimate = Math.Max(0, forecast.LowerBoundConsum[index]);
                        float estimate = forecast.ForecastedConsum[index];
                        float upperEstimate = forecast.UpperBoundConsum[index];
                        return $"Date: {rentalDate}\n" +
                        $"Actual Consum: {actualConsum}\n" +
                        $"Lower Estimate: {lowerEstimate}\n" +
                        $"Forecast: {estimate}\n" +
                        $"Upper Estimate: {upperEstimate}\n";
                    });

            // Output predictions

            //* Esto no debe de estar en un service *//

            Console.WriteLine("Rental Forecast");
            Console.WriteLine("---------------------");
            foreach (var prediction in forecastOutput)
            {
                Console.WriteLine(prediction);
            }
        }
    }
}
