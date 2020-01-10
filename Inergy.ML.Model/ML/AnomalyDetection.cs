using System;

namespace Inergy.ML.Model.ML
{
    public class AnomalyDetection
    {
        public string Date { get; set; }

        public float Consum { get; set; }

        public double Alert { get; set; }

        public double Score { get; set; }

        public double PValue { get; set; }
    }
}
