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
    internal class CosmosService : IHostedService
    {
        private readonly IMLService mlService;
        private readonly ApiContext apiContext;

        public CosmosService(IMLService mlService, ApiContext apiContext)
        {
            this.mlService = mlService;
            this.apiContext = apiContext;
        }

        /// <summary>
        /// Inicio de la aplicación de consola para generar y entrenar los modelos ya existentes
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Task</returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            RunML("ES0021000019390674DS", 30);

            return Task.CompletedTask;
        }
        
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
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
            Console.WriteLine($"=============== Predicción de Consumo Cups: {cups} ===============");
            Console.WriteLine("================================================================================\n");
            Console.WriteLine($"Obtener datos para el Cups: {cups}\n");

            var modelInputs = await mlService.GetDataView(cups);

            Console.WriteLine($"Entrenar el modelo y guardar como {cups}_daily_timeSeriesSSA.zip\n");

            var metrics = mlService.TrainAndSaveModel(modelInputs, modelPath);

            Console.WriteLine("Métricas de la evaluación:");
            Console.WriteLine($"* Mean Absolute Error: {metrics.MAE:F3}");
            Console.WriteLine($"* Root Mean Squared Error: {metrics.RMSE:F3}\n");

            Console.WriteLine($"Testear el modelo comparando los datos estimados y los datos reales de consumo para los últimos {horizon} días\n");

            var forecastOutputs = mlService.PredictModel(modelInputs, horizon, modelPath);

            foreach (var forecastOutput in forecastOutputs)
            {
                Console.WriteLine($"Fecha: {forecastOutput.Date.ToShortDateString()}, Real: {forecastOutput.ActualValue}, Predicción: {forecastOutput.Estimate}, " + 
                                    $"Baja: {forecastOutput.LowerEstimate}, Alta: {forecastOutput.UpperEstimate}");
            }

            Console.WriteLine("==================================================================");
            Console.WriteLine("==================================================================");
        }
    }
}
