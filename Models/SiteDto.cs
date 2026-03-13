using System.Text.Json.Serialization;

namespace HdbApi.Models
{
    /// <summary>
    /// Data Transfer Object for Site data from HDB_SITE table
    /// </summary>
    public class SiteDto
    {
        /// <summary>
        /// Unique site identifier
        /// </summary>
        [JsonPropertyName("SITE_ID")]
        public string? SITE_ID { get; set; }

        /// <summary>
        /// Full site name
        /// </summary>
        [JsonPropertyName("SITE_NAME")]
        public string? SITE_NAME { get; set; }

        /// <summary>
        /// Common site name
        /// </summary>
        [JsonPropertyName("SITE_COMMON_NAME")]
        public string? SITE_COMMON_NAME { get; set; }

        /// <summary>
        /// State code (e.g., "UT", "CO")
        /// </summary>
        [JsonPropertyName("STATE_CODE")]
        public string? STATE_CODE { get; set; }

        /// <summary>
        /// Object type name (e.g., "STREAM", "RESERVOIR")
        /// </summary>
        [JsonPropertyName("OBJECTTYPE_NAME")]
        public string? OBJECTTYPE_NAME { get; set; }

        /// <summary>
        /// Database-specific site code
        /// </summary>
        [JsonPropertyName("DB_SITE_CODE")]
        public string? DB_SITE_CODE { get; set; }
    }
}