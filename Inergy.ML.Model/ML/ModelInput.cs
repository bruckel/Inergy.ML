using System;
using System.Collections.Generic;
using System.Text;

namespace Inergy.ML.Model.ML
{
    public class ModelInput
    {
        public DateTime ConsumDate { get; set; }

        public float Periode { get; set; }

        public float TotalConsum { get; set; }
    }
}
