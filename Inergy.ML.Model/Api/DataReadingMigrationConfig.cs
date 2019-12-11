using System;
using System.Collections.Generic;
using System.Text;

namespace Inergy.ML.Model.Api
{
    public class DataReadingMigrationConfig
    {
        public byte IdDatabase { get; set; }
        public int IdProject { get; set; }
        public string Cups { get; set; }
        public DateTime BeginTimestamp { get; set; }
        public DateTime EndTimestamp { get; set; }
        public byte IdDataReadingType { get; set; }
    }
}
