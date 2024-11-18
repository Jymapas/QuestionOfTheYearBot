using System.Text.Json;

var apiUrl = "https://gotquestions.online/api/question/395284/";
var client = new HttpClient();
var response = await client.GetAsync(apiUrl);

if (response.IsSuccessStatusCode)
{
    var json = await response.Content.ReadAsStringAsync();
    var question = JsonSerializer.Deserialize<Question>(json);

    Console.WriteLine(question.text);
}
else
{
    Console.WriteLine("Failed to receive data");
}

class Question
{
    public string text { get; set; }
}