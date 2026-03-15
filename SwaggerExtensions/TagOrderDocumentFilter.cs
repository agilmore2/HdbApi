using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HdbApi.SwaggerExtensions
{
    /// <summary>
    /// Reorders Swagger tags to match the legacy API UI order.
    /// </summary>
    public class TagOrderDocumentFilter : IDocumentFilter
    {
        private static readonly string[] DesiredTagOrder = new[]
        {
            "HDB Tables",
            "HDB Connections",
            "HDB TimeSeries Data",
            "Testing Sandbox"
        };

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            if (swaggerDoc.Tags == null || !swaggerDoc.Tags.Any())
            {
                // If no global tags, create them from the operations
                var operationTags = swaggerDoc.Paths
                    .SelectMany(p => p.Value.Operations)
                    .SelectMany(o => o.Value.Tags)
                    .Select(t => t.Name)
                    .Distinct()
                    .OrderBy(name => 
                    {
                        var index = System.Array.IndexOf(DesiredTagOrder, name);
                        return index < 0 ? DesiredTagOrder.Length : index;
                    })
                    .ThenBy(name => name)
                    .Select(name => new OpenApiTag { Name = name })
                    .ToList();
                
                swaggerDoc.Tags = operationTags;
            }
            else
            {
                // Reorder existing global tags
                swaggerDoc.Tags = swaggerDoc.Tags
                    .OrderBy(t =>
                    {
                        var index = System.Array.IndexOf(DesiredTagOrder, t.Name);
                        return index < 0 ? DesiredTagOrder.Length : index;
                    })
                    .ThenBy(t => t.Name)
                    .ToList();
            }
        }
    }
}
