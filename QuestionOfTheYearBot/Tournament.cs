using System.Text.Json.Serialization;

internal class Tournament
{
    [JsonPropertyName("syncronId")] public int? Id { get; init; }
}