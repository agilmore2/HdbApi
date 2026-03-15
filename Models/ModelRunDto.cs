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
        [JsonPropertyName("model_run_id")]
        public int MODEL_RUN_ID { get; set; }

        /// <summary>
        /// Model run name
        /// </summary>
        [JsonPropertyName("model_run_name")]
        public string? MODEL_RUN_NAME { get; set; }

        /// <summary>
        /// Date/time when model run was loaded
        /// </summary>
        [JsonPropertyName("date_time_loaded")]
        public DateTime DATE_TIME_LOADED { get; set; }

        /// <summary>
        /// Date when model was last run
        /// </summary>
        [JsonPropertyName("run_date")]
        public DateTime RUN_DATE { get; set; }

        /// <summary>
        /// User who last modified the model run
        /// </summary>
        [JsonPropertyName("user_name")]
        public string? USER_NAME { get; set; }

        /// <summary>
        /// Model run comment/description
        /// </summary>
        [JsonPropertyName("model_run_cmmnt")]
        public string? MODEL_RUN_CMMT { get; set; }

        // HDB_MODEL fields
        /// <summary>
        /// Model identifier
        /// </summary>
        [JsonPropertyName("model_id")]
        public int MODEL_ID { get; set; }

        /// <summary>
        /// Model name
        /// </summary>
        [JsonPropertyName("model_name")]
        public string? MODEL_NAME { get; set; }

        /// <summary>
        /// Model comment/description
        /// </summary>
        [JsonPropertyName("model_cmmnt")]
        public string? MODEL_CMMT { get; set; }
    }
}