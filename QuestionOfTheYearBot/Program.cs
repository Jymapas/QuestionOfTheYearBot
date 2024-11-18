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
}
else
{
    Console.WriteLine("Failed to receive data");
}

internal class Question
{
    [JsonPropertyName("text")]
    public string Text { get; init; }
}