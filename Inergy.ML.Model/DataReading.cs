using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Inergy.ML.Model
{
    /// <summary>
    /// Clase estándar de datos temporales
    /// </summary>
    public class DataReading
    {
        /// <summary>
        /// Identificador Mongo
        /// </summary>
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        /// <summary>
        /// Identificador de suministro
        /// </summary>
        public string Cups { get; set; }

        /// <summary>
        /// Tipo de fuente energética
        /// </summary>
        public int IdEnergySource { get; set; }

        /// <summary>
        /// Tipo de dato
        /// </summary>
        public double Type { get; set; }

        /// <summary>
        /// Valor
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Fecha y hora (UTC)
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Unidad de medida
        /// </summary>
        public string Unit { get; set; }
    }
}
