using System.Text.Json;
using System.Text.Json.Serialization;

namespace CommonLibrary.Serialization;

public static class JsonSerializationOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}