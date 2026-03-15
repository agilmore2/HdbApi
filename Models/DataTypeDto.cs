using System.Text.Json.Serialization;

namespace HdbApi.Models
{
    /// <summary>
    /// Data Transfer Object for Datatype data from HDB_DATATYPE and HDB_UNIT tables
    /// </summary>
    public class DataTypeDto
    {
        /// <summary>
        /// Unique datatype identifier
        /// </summary>
        [JsonPropertyName("datatype_id")]
        public string? DATATYPE_ID { get; set; }

        /// <summary>
        /// Full datatype name
        /// </summary>
        [JsonPropertyName("datatype_name")]
        public string? DATATYPE_NAME { get; set; }

        /// <summary>
        /// Common datatype name
        /// </summary>
        [JsonPropertyName("datatype_common_name")]
        public string? DATATYPE_COMMON_NAME { get; set; }

        /// <summary>
        /// Physical quantity name
        /// </summary>
        [JsonPropertyName("physical_quantity_name")]
        public string? PHYSICAL_QUANTITY_NAME { get; set; }

        /// <summary>
        /// Unit identifier
        /// </summary>
        [JsonPropertyName("unit_id")]
        public int? UNIT_ID { get; set; }

        /// <summary>
        /// Unit name
        /// </summary>
        [JsonPropertyName("unit_name")]
        public string? UNIT_NAME { get; set; }

        /// <summary>
        /// Common unit name/symbol
        /// </summary>
        [JsonPropertyName("unit_common_name")]
        public string? UNIT_COMMON_NAME { get; set; }

        /// <summary>
        /// Allowable time intervals
        /// </summary>
        [JsonPropertyName("allowable_intervals")]
        public string? ALLOWABLE_INTERVALS { get; set; }

        /// <summary>
        /// Agency identifier
        /// </summary>
        [JsonPropertyName("agen_id")]
        public int? AGEN_ID { get; set; }

        /// <summary>
        /// Comment/description
        /// </summary>
        [JsonPropertyName("cmmnt")]
        public string? CMMNT { get; set; }
    }
}
