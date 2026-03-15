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
        [JsonPropertyName("site_datatype_id")]
        public int SITE_DATATYPE_ID { get; set; }

        /// <summary>
        /// Site identifier
        /// </summary>
        [JsonPropertyName("site_id")]
        public int SITE_ID { get; set; }

        /// <summary>
        /// Datatype identifier
        /// </summary>
        [JsonPropertyName("datatype_id")]
        public int DATATYPE_ID { get; set; }

        /// <summary>
        /// Metadata for the site and datatype associated with this SiteDatatype
        /// </summary>
        [JsonPropertyName("metadata")]
        public SiteDataTypeMetadataDto? Metadata { get; set; }
    }

    public class SiteDataTypeMetadataDto
    {
        [JsonPropertyName("site_metadata")]
        public SiteDto? SiteMetadata { get; set; }

        [JsonPropertyName("datatype_metadata")]
        public DataTypeDto? DatatypeMetadata { get; set; }
    }
}
