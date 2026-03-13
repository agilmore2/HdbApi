using System.Text.Json.Serialization;

namespace HdbApi.Models
{
    /// <summary>
    /// Data Transfer Object for Datatype data from HDB_DATATYPE and HDB_UNIT tables
    /// </summary>
    public class DataTypeDto
    {
        // HDB_DATATYPE fields
        /// <summary>
        /// Unique datatype identifier
        /// </summary>
        [JsonPropertyName("DATATYPE_ID")]
        public int DATATYPE_ID { get; set; }

        /// <summary>
        /// Full datatype name
        /// </summary>
        [JsonPropertyName("DATATYPE_NAME")]
        public string? DATATYPE_NAME { get; set; }

        /// <summary>
        /// Common datatype name
        /// </summary>
        [JsonPropertyName("DATATYPE_COMMON_NAME")]
        public string? DATATYPE_COMMON_NAME { get; set; }

        /// <summary>
        /// Physical quantity name
        /// </summary>
        [JsonPropertyName("PHYSICAL_QUANTITY_NAME")]
        public string? PHYSICAL_QUANTITY_NAME { get; set; }

        /// <summary>
        /// Unit identifier
        /// </summary>
        [JsonPropertyName("UNIT_ID")]
        public int UNIT_ID { get; set; }

        /// <summary>
        /// Allowable time intervals
        /// </summary>
        [JsonPropertyName("ALLOWABLE_INTERVALS")]
        public string? ALLOWABLE_INTERVALS { get; set; }

        /// <summary>
        /// Agency identifier
        /// </summary>
        [JsonPropertyName("AGEN_ID")]
        public int? AGEN_ID { get; set; }

        /// <summary>
        /// Comment/description
        /// </summary>
        [JsonPropertyName("CMMNT")]
        public string? CMMNT { get; set; }

        // HDB_UNIT fields (duplicated UNIT_ID for join)
        /// <summary>
        /// Unit identifier (duplicate from join)
        /// </summary>
        [JsonPropertyName("UNIT_ID")]
        public int UNIT_ID_1 { get; set; }

        /// <summary>
        /// Unit name
        /// </summary>
        [JsonPropertyName("UNIT_NAME")]
        public string? UNIT_NAME { get; set; }

        /// <summary>
        /// Common unit name/symbol
        /// </summary>
        [JsonPropertyName("UNIT_COMMON_NAME")]
        public string? UNIT_COMMON_NAME { get; set; }

        /// <summary>
        /// Dimension identifier
        /// </summary>
        [JsonPropertyName("DIMENSION_ID")]
        public int DIMENSION_ID { get; set; }

        /// <summary>
        /// Base unit identifier
        /// </summary>
        [JsonPropertyName("BASE_UNIT_ID")]
        public int BASE_UNIT_ID { get; set; }

        /// <summary>
        /// Month year flag
        /// </summary>
        [JsonPropertyName("MONTH_YEAR")]
        public string? MONTH_YEAR { get; set; }

        /// <summary>
        /// Over month year flag
        /// </summary>
        [JsonPropertyName("OVER_MONTH_YEAR")]
        public string? OVER_MONTH_YEAR { get; set; }

        /// <summary>
        /// Is factor flag
        /// </summary>
        [JsonPropertyName("IS_FACTOR")]
        public int IS_FACTOR { get; set; }

        /// <summary>
        /// Multiplication factor
        /// </summary>
        [JsonPropertyName("MULT_FAC")]
        public decimal MULT_FAC { get; set; }
    }
}