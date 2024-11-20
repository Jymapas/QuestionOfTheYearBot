using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

const string packageApiBase = "https://gotquestions.online/api/pack/";
const string questionApiBase = "https://gotquestions.online/api/question/";
const string noLinkError = "В сообщении нет ссылки на вопрос.";
const string noTextError = "Бот поддерживает только работу с текстом.";
const string questionReceiveError = "Не удалось вопрос по этой ссылке.";
HttpClient client = new();

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", false, true)
    .Build();

var botToken = configuration["Authentication:Token"];

if (string.IsNullOrEmpty(botToken)) throw new InvalidOperationException("Token is not configured.");

using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient(botToken, cancellationToken: cts.Token);
bot.OnMessage += OnMessage;

Console.ReadLine();
return;

async Task<string> GetQuestionAsync(int questionId)
{
    var questionApiUrl = $"{questionApiBase}{questionId}/";
    var questionResponse = await client.GetAsync(questionApiUrl);

    if (!questionResponse.IsSuccessStatusCode) return "Failed to receive data";

    var questionJson = await questionResponse.Content.ReadAsStringAsync();
    var question = JsonSerializer.Deserialize<Question>(questionJson);
    question.QuestionId = questionApiUrl.Split('/')[^2];

    var packageResponse = await client.GetAsync($"{packageApiBase}{question.PackageId}/");
    var packageJson = await packageResponse.Content.ReadAsStringAsync();
    var package = JsonSerializer.Deserialize<Tournament>(packageJson);
    if (package is { Id: not null }) question.TournamentId = package.Id.Value;

    return question.ToString();
}

async Task OnMessage(Message msg, UpdateType type)
{
    if (msg.Text is null)
    {
        await bot.SendMessage(msg.Chat, noTextError);
        return;
    }

    var (isMatch, questionId) = TryGetQuestionIdAsync(msg.Text);
    if (!isMatch)
    {
        await bot.SendMessage(msg.Chat, noLinkError);
        return;
    }

    var question = await GetQuestionAsync(questionId.Value);
    if (string.IsNullOrEmpty(question))
    {
        await bot.SendMessage(msg.Chat, questionReceiveError);
        return;
    }

    await bot.SendMessage(msg.Chat, $"```\n{question}\n```", ParseMode.MarkdownV2);
}

static (bool isMatch, int? questionId) TryGetQuestionIdAsync(string messageText)
{
    var pattern = @"https:\/\/gotquestions\.online\/question\/(\d+)";

    var match = Regex.Match(messageText, pattern);
    if (!match.Success) return (false, null);

    var numberString = match.Groups[1].Value;
    var number = int.Parse(numberString);
    return (true, number);
}