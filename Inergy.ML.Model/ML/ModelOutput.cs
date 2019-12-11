using System;
using System.Collections.Generic;
using System.Text;

namespace Inergy.ML.Model.ML
{
    public class ModelOutput
    {
        public float[] ForecastedConsum { get; set; }

        public float[] LowerBoundConsum { get; set; }

        public float[] UpperBoundConsum { get; set; }
    }
}
