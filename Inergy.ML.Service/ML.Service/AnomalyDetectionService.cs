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
    public class AnomalyDetectionService : IAnomalyDetectionService
    {
        private readonly IDataReadingRepository dataReadingRepository;
        private readonly ILogger log;
        private readonly MLContext mlContext;

        public AnomalyDetectionService(IDataReadingRepository dataReadingRepository, ILogger log, MLContext mlContext)
        {
            this.dataReadingRepository = dataReadingRepository;
            this.log = log;
            this.mlContext = mlContext;
        }

        public async Task<(IEnumerable<ModelInput>, IEnumerable<Prediction>)> GetPredictedValues(string cups)
        {
            //string rootDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../Inergy.ML.Models/ForecastBySsa"));
            string rootDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../"));
            string modelPath = Path.Combine(rootDir, $"{cups}_daily_detectSpikeBySsa.zip");

            //* Cargar el modelo en el caso de que ya exista *//
            //bool createModel = !File.Exists(modelPath);

            var modelInputs = await GetDataView(cups);

            //if (createModel)
            //{
                //* Guardar métricas en el log de la aplicación *//
                var model = TrainAndSaveModel(modelInputs, modelPath);
            //}

            //* Realizar predicción en base al modelo *//
            return (modelInputs, PredictModel(modelInputs, modelPath, model));
        }

        private async Task<IEnumerable<ModelInput>> GetDataView(string cups)
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
                TotalConsum = (float)r.Sum(p => p.Value)
            })
            .OrderBy(r => r.ConsumDate);

            return modelInputs;
        }

        private ITransformer TrainAndSaveModel(IEnumerable<ModelInput> modelInputs, string modelPath)
        {
            var dataView = mlContext.Data.LoadFromEnumerable<ModelInput>(modelInputs);

            var trainigPipeLine = mlContext.Transforms.DetectSpikeBySsa(
               outputColumnName: "ConsumPrediction",
               inputColumnName: "TotalConsum",
               confidence: 98,
               pvalueHistoryLength: 31,
               trainingWindowSize: modelInputs.Count(),
               seasonalityWindowSize: 31);

            var trainedModel = trainigPipeLine.Fit(dataView);

            return trainedModel;

            // STEP 6: Save/persist the trained model to a .ZIP file
            //mlContext.Model.Save(trainedModel, dataView.Schema, modelPath);
        }

        private IEnumerable<Prediction> PredictModel(IEnumerable<ModelInput> modelInputs, string modelPath, ITransformer trainedModel = null)
        {
            // Load the forecast engine that has been previously saved.
            //ITransformer trainedModel;

            //using (var file = File.OpenRead(modelPath))
            //{
            //    trainedModel = mlContext.Model.Load(file, out DataViewSchema schema);
            //}

            var dataView = mlContext.Data.LoadFromEnumerable<ModelInput>(modelInputs);
            var transformedData = trainedModel.Transform(dataView);

            var predictions =  mlContext.Data.CreateEnumerable<Prediction>(transformedData, false);
            
            return predictions;
        }
    }
}
