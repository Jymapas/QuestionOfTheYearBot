using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

const string packageApiBase = "https://gotquestions.online/api/pack/";
const string questionApiBase = "https://gotquestions.online/api/question/";
const string noLinkError = "В сообщении нет ссылки на вопрос.";
const string noTextError = "Бот поддеживает только работу с текстом.";

async Task<string> GetQuestionAsync(int questionId)
{
    var client = new HttpClient();
    var questionApiUrl = $"{questionApiBase}{questionId}/";
    var questionResponse = await client.GetAsync(questionApiUrl);

    if (!questionResponse.IsSuccessStatusCode) return "Failed to receive data";

    var questionJson = await questionResponse.Content.ReadAsStringAsync();
    var question = JsonSerializer.Deserialize<Question>(questionJson);
    question.QuestionId = questionApiUrl.Split('/')[^2];

    var packageResponse = await client.GetAsync($@"{packageApiBase}{question.packageId}/");
    var packageJson = await packageResponse.Content.ReadAsStringAsync();
    var package = JsonSerializer.Deserialize<Tournament>(packageJson);
    if (package is { Id: not null }) question.TournamentId = package.Id.Value;

    return question.ToString();
}

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", false, true)
    .Build();

var botToken = configuration["Authentication:Token"];

if (string.IsNullOrEmpty(botToken)) throw new InvalidOperationException("Token is not configured.");

using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient(botToken, cancellationToken: cts.Token);
bot.OnMessage += OnMessage;

Console.ReadLine();

async Task OnMessage(Message msg, UpdateType type)
{
    if (msg.Text is null) await bot.SendMessage(msg.Chat, noTextError);

    var (isMatch, questionId) = await TryGetQuestionIdAsync(msg.Text);
    if (!isMatch) await bot.SendMessage(msg.Chat, noLinkError);

    await bot.SendMessage(msg.Chat, await GetQuestionAsync(questionId.Value));
}

static async Task<(bool isMatch, int? questionId)> TryGetQuestionIdAsync(string messageText)
{
    var pattern = @"https:\/\/gotquestions\.online\/question\/(\d+)";

    var match = Regex.Match(messageText, pattern);
    if (!match.Success) return (false, null);

    var numberString = match.Groups[1].Value;
    var number = int.Parse(numberString);
    return (true, number);
}

internal class Question
{
    [JsonPropertyName("packTitle")] public string TournamentName { get; init; }
    [JsonPropertyName("packId")] public int packageId { get; init; }
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
        if (TournamentId != 0) sb.Append($"Ссылка на турнир: https://rating.chgk.info/tournament/{TournamentId}\n");
        sb.Append($"Ссылка на вопрос: https://gotquestions.online/question/{QuestionId}\n");
        sb.Append($"Вопрос {Number}\n");
        if (HandoutPic != null || HandoutText != null)
        {
            sb.Append("[Раздаточный материал:");
            if (HandoutPic != null) sb.Append($"{HandoutPic}\n");

            if (HandoutText != null) sb.Append($"{HandoutText}\n");
            sb.Append("]\n");
        }

        sb.Append($"{Text}\n");
        sb.Append($"Ответ: {Answer}\n");
        if (AdditionalAnswer != null) sb.Append($"Зачет: {AdditionalAnswer}\n");
        if (!string.IsNullOrEmpty(WrongAnswer)) sb.Append($"Незачет: {WrongAnswer}\n");
        sb.Append($"Комментарий: {Comment}\n");
        sb.Append($"Источник: {Source}\n");
        if (Authors.Count > 1)
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

internal class Tournament
{
    [JsonPropertyName("syncronId")] public int? Id { get; init; }
}