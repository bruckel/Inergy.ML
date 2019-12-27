using Inergy.ML.Data.Entity;
using Inergy.ML.Service;
using Inergy.Tools.Architecture.Model;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Inergy.ML.Application
{
    internal class MLMainService : IHostedService
    {
        private readonly IMLService mlService;
        private readonly IAnomalyDetectionService anomalyDetectionService;
        private readonly ApiContext apiContext;

        public MLMainService(IMLService mlService, IAnomalyDetectionService anomalyDetectionService, ApiContext apiContext)
        {
            this.mlService = mlService;
            this.anomalyDetectionService = anomalyDetectionService;
            this.apiContext = apiContext;
        }

        /// <summary>
        /// Inicio de la aplicaci�n de consola para generar y entrenar los modelos ya existentes
        /// </summary>
        /// <param name="cancellationToken">Token de cancelaci�n</param>
        /// <returns>Task</returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            //RunML("ES0021000019390674DS", 30);
            RunAnomalyDetection("ES0021000019390674DS");

            return Task.CompletedTask;
        }
        
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async void RunAnomalyDetection(string cups)
        {
            Console.WriteLine("\n================================================================================");
            Console.WriteLine($"=============== Predicci�n de Consumo Cups: {cups} ===============");
            Console.WriteLine("================================================================================\n");
            Console.WriteLine($"Obtener anomal�as para el Cups: {cups}\n");

            var results = await this.anomalyDetectionService.GetPredictedValues(cups);

            // Output the input data and predictions
            Console.WriteLine("======Displaying anomalies in the Power meter data=========");
            Console.WriteLine("Date\t\tConsum\tAlert\tScore\tP-Value");

            if (results.Item2.Any())
            {
                var items = results.Item1.Zip(results.Item2, (first, second) => new
                {
                    Value = first,
                    Prediction = second
                });

                foreach (var item in items)
                {
                    if (item.Prediction.ConsumPrediction[0] == 1)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkYellow;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }

                    Console.WriteLine("{0}\t{1:0.0000}\t{2:0.00}\t{3:0.00}\t{4:0.00}", item.Value.ConsumDate.ToShortDateString(), 
                                        item.Value.TotalConsum, item.Prediction.ConsumPrediction[0], 
                                        item.Prediction.ConsumPrediction[1], item.Prediction.ConsumPrediction[2]);
                    
                    Console.ResetColor();
                }
            }

            Console.WriteLine("==================================================================");
            Console.WriteLine("==================================================================");
        }

        public async void RunML(string cups, int horizon)
        {
            string rootDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../Inergy.ML.Models"));
            string modelPath = Path.Combine(rootDir, $"{cups}_daily_timeSeriesSSA.zip");

            if (File.Exists(modelPath))
            {
                File.Delete(modelPath);
            }

            Console.WriteLine("\n================================================================================");
            Console.WriteLine($"=============== Predicci�n de Consumo Cups: {cups} ===============");
            Console.WriteLine("================================================================================\n");
            Console.WriteLine($"Obtener datos para el Cups: {cups}\n");

            var modelInputs = await mlService.GetDataView(cups);

            Console.WriteLine($"Entrenar el modelo y guardar como {cups}_daily_timeSeriesSSA.zip\n");

            var metrics = mlService.TrainAndSaveModel(modelInputs, modelPath);

            Console.WriteLine("M�tricas de la evaluaci�n:");
            Console.WriteLine($"* Mean Absolute Error: {metrics.MAE:F3}");
            Console.WriteLine($"* Root Mean Squared Error: {metrics.RMSE:F3}\n");

            Console.WriteLine($"Testear el modelo comparando los datos estimados y los datos reales de consumo para los �ltimos {horizon} d�as\n");

            var forecastOutputs = mlService.PredictModel(modelInputs, horizon, modelPath);

            foreach (var forecastOutput in forecastOutputs)
            {
                Console.WriteLine($"Fecha: {forecastOutput.Date.ToShortDateString()}, Real: {forecastOutput.ActualValue}, Predicci�n: {forecastOutput.Estimate}, " + 
                                    $"Baja: {forecastOutput.LowerEstimate}, Alta: {forecastOutput.UpperEstimate}");
            }

            Console.WriteLine("==================================================================");
            Console.WriteLine("==================================================================");
        }
    }
}
