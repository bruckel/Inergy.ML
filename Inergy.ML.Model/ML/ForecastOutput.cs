using System;
using System.Collections.Generic;
using System.Text;

namespace Inergy.ML.Model.ML
{
    public class ForecastOutput
    {
        public DateTime Date { get; set; }

        public int Day { get; set; }

        public float ActualValue { get; set; }

        public float LowerEstimate { get; set; }

        public float Estimate { get; set; }

        public float UpperEstimate { get; set; }
    }
}
