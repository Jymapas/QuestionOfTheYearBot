using System.Text.Json;
using System.Text.Json.Serialization;

var apiUrl = "https://gotquestions.online/api/question/395284/";
var client = new HttpClient();
var response = await client.GetAsync(apiUrl);

if (response.IsSuccessStatusCode)
{
    var json = await response.Content.ReadAsStringAsync();
    var question = JsonSerializer.Deserialize<Question>(json);

    Console.WriteLine(question.Text);
    Console.WriteLine(question.Authors.First().Name);
}
else
{
    Console.WriteLine("Failed to receive data");
}

internal class Question
{
    [JsonPropertyName("packTitle")] public string TournamentName { get; init; }
    public int TournamentId { get; init; }
    public string QuestionLink { get; init; }
    [JsonPropertyName("number")] public int QuestionNumber { get; init; }
    [JsonPropertyName("razdatkaText")] public string? HandoutText { get; init; }
    [JsonPropertyName("razdatkaPic")] public string? HandoutPic { get; init; }
    [JsonPropertyName("text")] public string Text { get; init; }
    [JsonPropertyName("answer")] public string Answer { get; init; }
    [JsonPropertyName("zachet")] public string? AdditionalAnswer { get; init; }
    [JsonPropertyName("nezachet")] public string? WrongAnswer { get; init; }
    [JsonPropertyName("comment")] public string Comment { get; init; }
    [JsonPropertyName("source")] public string Source { get; init; }
    [JsonPropertyName("authors")] public List<Author> Authors { get; init; }

    internal class Author
    {
        [JsonPropertyName("name")] public string Name { get; init; }
    }
}