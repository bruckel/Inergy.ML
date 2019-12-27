using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inergy.ML.Model.ML
{
    public class Prediction
    {
        [VectorType(3)]
        public double[] ConsumPrediction { get; set; }
    }
}
