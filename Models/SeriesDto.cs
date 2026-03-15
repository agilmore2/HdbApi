using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HdbApi.Models
{
    public class SeriesQueryDto
    {
        [JsonPropertyName("hdb")]
        public string? Hdb { get; set; }

        [JsonPropertyName("sdi")]
        public string? Sdi { get; set; }

        [JsonPropertyName("interval")]
        public string? Interval { get; set; }

        [JsonPropertyName("t1")]
        public DateTime T1 { get; set; }

        [JsonPropertyName("t2")]
        public DateTime T2 { get; set; }

        [JsonPropertyName("retrieved")]
        public DateTime Retrieved { get; set; }

        [JsonPropertyName("table")]
        public string? Table { get; set; }

        [JsonPropertyName("mrid")]
        public int Mrid { get; set; }

        [JsonPropertyName("rbase")]
        public bool Rbase { get; set; }
    }

    public class SeriesMetadataDto
    {
        [JsonPropertyName("site_metadata")]
        public SiteDto? SiteMetadata { get; set; }

        [JsonPropertyName("datatype_metadata")]
        public DataTypeDto? DatatypeMetadata { get; set; }
    }

    public class SeriesPointDto
    {
        [JsonPropertyName("datetime")]
        public DateTime Datetime { get; set; }

        [JsonPropertyName("value")]
        public string? Value { get; set; }

        [JsonPropertyName("flag")]
        public string? Flag { get; set; }
    }

    public class SeriesResponseDto
    {
        [JsonPropertyName("query")]
        public SeriesQueryDto? Query { get; set; }

        [JsonPropertyName("metadata")]
        public SeriesMetadataDto? Metadata { get; set; }

        [JsonPropertyName("data")]
        public List<SeriesPointDto>? Data { get; set; }
    }
}
