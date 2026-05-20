using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using Xunit;

namespace ReviewAnalyser
{
    public class ReviewAnalysisSchemaTests
    {
        [Fact]
        public void ReviewAnalysis_Schema_ContainsOnlySupportedTypes()
        {
            // Generate the schema that the SDK will send to the API
            JsonNode schema = JsonSchemaExporter.GetJsonSchemaAsNode(
                new JsonSerializerOptions(),
                typeof(ReviewAnalysis));

            string schemaJson = schema.ToJsonString();

            // "format" annotations appear when DateTime or DateTimeOffset leak into the schema
            Assert.DoesNotContain("\"format\"", schemaJson);

            // "$ref" appears when complex types that may not be supported are present
            Assert.DoesNotContain("\"$ref\"", schemaJson);

            // "anyUri" appears when Uri is used
            Assert.DoesNotContain("\"anyUri\"", schemaJson);
        }

        [Fact]
        public void ReviewAnalysis_Schema_ContainsExpectedProperties()
        {
            JsonNode schema = JsonSchemaExporter.GetJsonSchemaAsNode(
                new JsonSerializerOptions(),
                typeof(ReviewAnalysis));

            string schemaJson = schema.ToJsonString();

            // Verify all required properties are present in the generated schema
            Assert.Contains("\"productName\"", schemaJson);
            Assert.Contains("\"summary\"", schemaJson);
            Assert.Contains("\"sentiment\"", schemaJson);
            Assert.Contains("\"score\"", schemaJson);
            Assert.Contains("\"tags\"", schemaJson);
            Assert.Contains("\"reviewDate\"", schemaJson);
        }
    }
}
