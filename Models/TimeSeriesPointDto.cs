using System.Text.Json.Serialization;

namespace HdbApi.Models
{
    /// <summary>
    /// Data Transfer Object for Time Series data points
    /// </summary>
    public class TimeSeriesPointDto
    {
        /// <summary>
        /// Date/time of the data point
        /// </summary>
        [JsonPropertyName("DATETIME")]
        public DateTime DATETIME { get; set; }

        /// <summary>
        /// Value of the data point
        /// </summary>
        [JsonPropertyName("VALUE")]
        public string? VALUE { get; set; }
    }
}