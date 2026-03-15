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
        [JsonPropertyName("site_id")]
        public string? SITE_ID { get; set; }

        /// <summary>
        /// Full site name
        /// </summary>
        [JsonPropertyName("site_name")]
        public string? SITE_NAME { get; set; }

        /// <summary>
        /// Common site name
        /// </summary>
        [JsonPropertyName("site_common_name")]
        public string? SITE_COMMON_NAME { get; set; }

        /// <summary>
        /// Site description
        /// </summary>
        [JsonPropertyName("description")]
        public string? DESCRIPTION { get; set; }

        /// <summary>
        /// Site geographic elevation (if applicable)
        /// </summary>
        [JsonPropertyName("elevation")]
        public string? ELEVATION { get; set; }

        /// <summary>
        /// Site geographic latitude (if applicable)
        /// </summary>
        [JsonPropertyName("lat")]
        public string? LAT { get; set; }

        /// <summary>
        /// Site geographic longitude (if applicable)
        /// </summary>
        [JsonPropertyName("longi")]
        public string? LONGI { get; set; }

        /// <summary>
        /// Site HDB membership
        /// </summary>
        [JsonPropertyName("db_site_code")]
        public string? DB_SITE_CODE { get; set; }

        /// <summary>
        /// Site Object Type ID
        /// </summary>
        [JsonPropertyName("objecttype_id")]
        public int? OBJECTTYPE_ID { get; set; }

        /// <summary>
        /// Site Object Type name
        /// </summary>
        [JsonPropertyName("objecttype_name")]
        public string? OBJECTTYPE_NAME { get; set; }

        /// <summary>
        /// Basin (if applicable)
        /// </summary>
        [JsonPropertyName("basin_id")]
        public int? BASIN_ID { get; set; }

        /// <summary>
        /// HUC (if applicable)
        /// </summary>
        [JsonPropertyName("hydrologic_unit")]
        public string? HYDROLOGIC_UNIT { get; set; }

        /// <summary>
        /// RM (if applicable)
        /// </summary>
        [JsonPropertyName("river_mile")]
        public float? RIVER_MILE { get; set; }

        /// <summary>
        /// Segment (if applicable)
        /// </summary>
        [JsonPropertyName("segment_no")]
        public int? SEGMENT_NO { get; set; }

        /// <summary>
        /// State ID
        /// </summary>
        [JsonPropertyName("state_id")]
        public int? STATE_ID { get; set; }

        /// <summary>
        /// State Code
        /// </summary>
        [JsonPropertyName("state_code")]
        public string? STATE_CODE { get; set; }

        /// <summary>
        /// USGS ID (if applicable)
        /// </summary>
        [JsonPropertyName("usgs_id")]
        public string? USGS_ID { get; set; }

        /// <summary>
        /// NWS ID (if applicable)
        /// </summary>
        [JsonPropertyName("nws_code")]
        public string? NWS_CODE { get; set; }

        /// <summary>
        /// SHEF ID (if applicable)
        /// </summary>
        [JsonPropertyName("shef_code")]
        public string? SHEF_CODE { get; set; }

        /// <summary>
        /// SCS ID (if applicable)
        /// </summary>
        [JsonPropertyName("scs_id")]
        public string? SCS_ID { get; set; }

        /// <summary>
        /// Parent site Object Type (if applicable)
        /// </summary>
        [JsonPropertyName("parent_objecttype_id")]
        public int? PARENT_OBJECTTYPE_ID { get; set; }

        /// <summary>
        /// Parent site Site ID (if applicable)
        /// </summary>
        [JsonPropertyName("parent_site_id")]
        public int? PARENT_SITE_ID { get; set; }
    }
}
