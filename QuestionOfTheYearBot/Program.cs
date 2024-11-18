using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var apiUrl = "https://gotquestions.online/api/question/395284/";
var client = new HttpClient();
var response = await client.GetAsync(apiUrl);

if (response.IsSuccessStatusCode)
{
    var json = await response.Content.ReadAsStringAsync();
    var question = JsonSerializer.Deserialize<Question>(json);
    question.QuestionId = apiUrl.Split('/')[^2];

    Console.WriteLine(question);
}
else
{
    Console.WriteLine("Failed to receive data");
}

internal class Question
{
    [JsonPropertyName("packTitle")] public string TournamentName { get; init; }
    public int TournamentId { get; set; }
    public string QuestionId { get; set; }
    [JsonPropertyName("number")] public int Number { get; init; }
    [JsonPropertyName("razdatkaText")] public string? HandoutText { get; init; }
    [JsonPropertyName("razdatkaPic")] public string? HandoutPic { get; init; }
    [JsonPropertyName("text")] public string Text { get; init; }
    [JsonPropertyName("answer")] public string Answer { get; init; }
    [JsonPropertyName("zachet")] public string? AdditionalAnswer { get; init; }
    [JsonPropertyName("nezachet")] public string? WrongAnswer { get; init; }
    [JsonPropertyName("comment")] public string Comment { get; init; }
    [JsonPropertyName("source")] public string Source { get; init; }
    [JsonPropertyName("authors")] public List<Author> Authors { get; init; }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append($"Название турнира: {TournamentName}\n");
        sb.Append($"Ссылка на вопрос: https://gotquestions.online/question/{QuestionId}\n");
        sb.Append($"Вопрос {Number}\n");
        if (HandoutPic != null || HandoutText != null)
        {
            sb.Append("[Раздаточный материал:\n");
            if (HandoutPic != null) sb.Append($"{HandoutPic}\n");

            if (HandoutText != null) sb.Append($"{HandoutText}\n");
            sb.Append("]\n");
        }

        sb.Append($"{Text}\n");
        sb.Append($"Ответ: {Answer}\n");
        if (AdditionalAnswer != null) sb.Append($"Зачет: {AdditionalAnswer}\n");
        if (WrongAnswer != null) sb.Append($"Незачет: {WrongAnswer}\n");
        sb.Append($"Комментарий: {Comment}\n");
        sb.Append($"Источник: {Source}\n");
        if (Authors.Capacity > 1)
        {
            sb.Append($"Авторы: {string.Join(", ", Authors.Select(a => a.Name))}\n");
            return sb.ToString();
        }

        sb.Append($"Автор: {Authors.First()}\n");
        return sb.ToString();
    }

    internal class Author
    {
        [JsonPropertyName("name")] public string Name { get; init; }

        public override string ToString()
        {
            return Name;
        }
    }
}