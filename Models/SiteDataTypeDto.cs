using System.Text.Json.Serialization;

namespace HdbApi.Models
{
    /// <summary>
    /// Data Transfer Object for Site-Datatype relationships from HDB_SITE_DATATYPE table
    /// </summary>
    public class SiteDataTypeDto
    {
        /// <summary>
        /// Unique site-datatype identifier
        /// </summary>
        [JsonPropertyName("SITE_DATATYPE_ID")]
        public int SITE_DATATYPE_ID { get; set; }

        /// <summary>
        /// Site identifier
        /// </summary>
        [JsonPropertyName("SITE_ID")]
        public int SITE_ID { get; set; }

        /// <summary>
        /// Datatype identifier
        /// </summary>
        [JsonPropertyName("DATATYPE_ID")]
        public int DATATYPE_ID { get; set; }
    }
}