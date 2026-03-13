using System.Text.Json.Serialization;

namespace HdbApi.Models
{
    /// <summary>
    /// Data Transfer Object for Model Run data from REF_MODEL_RUN and HDB_MODEL tables
    /// </summary>
    public class ModelRunDto
    {
        // REF_MODEL_RUN fields
        /// <summary>
        /// Unique model run identifier
        /// </summary>
        [JsonPropertyName("MODEL_RUN_ID")]
        public int MODEL_RUN_ID { get; set; }

        /// <summary>
        /// Model run name
        /// </summary>
        [JsonPropertyName("MODEL_RUN_NAME")]
        public string? MODEL_RUN_NAME { get; set; }

        /// <summary>
        /// Date/time when model run was loaded
        /// </summary>
        [JsonPropertyName("DATE_TIME_LOADED")]
        public DateTime DATE_TIME_LOADED { get; set; }

        /// <summary>
        /// Date when model was last run
        /// </summary>
        [JsonPropertyName("RUN_DATE")]
        public DateTime RUN_DATE { get; set; }

        /// <summary>
        /// User who last modified the model run
        /// </summary>
        [JsonPropertyName("USER_NAME")]
        public string? USER_NAME { get; set; }

        /// <summary>
        /// Model run comment/description
        /// </summary>
        [JsonPropertyName("MODEL_RUN_CMMT")]
        public string? MODEL_RUN_CMMT { get; set; }

        // HDB_MODEL fields
        /// <summary>
        /// Model identifier
        /// </summary>
        [JsonPropertyName("MODEL_ID")]
        public int MODEL_ID { get; set; }

        /// <summary>
        /// Model name
        /// </summary>
        [JsonPropertyName("MODEL_NAME")]
        public string? MODEL_NAME { get; set; }

        /// <summary>
        /// Model comment/description
        /// </summary>
        [JsonPropertyName("MODEL_CMMT")]
        public string? MODEL_CMMT { get; set; }
    }
}