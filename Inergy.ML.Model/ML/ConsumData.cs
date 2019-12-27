using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inergy.ML.Model.ML
{
    public class ConsumData
    {
        //[LoadColumn(0)]
        public string Cups { get; set; }
        //[LoadColumn(1)]
        public DateTime ConsumDate { get; set; }
        //[LoadColumn(2)]
        public float TotalConsum { get; set; }
    }
}
