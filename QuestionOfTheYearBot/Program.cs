using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using QuestionOfTheYearBot;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

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
    var questionApiUrl = $"{ServiceLines.questionApiBase}{questionId}/";
    var questionResponse = await client.GetAsync(questionApiUrl);

    if (!questionResponse.IsSuccessStatusCode) return ServiceLines.questionReceiveError;

    var questionJson = await questionResponse.Content.ReadAsStringAsync();
    var question = JsonSerializer.Deserialize<Question>(questionJson);
    question.QuestionId = questionApiUrl.Split('/')[^2];

    var packageResponse = await client.GetAsync($"{ServiceLines.packageApiBase}{question.PackageId}/");
    var packageJson = await packageResponse.Content.ReadAsStringAsync();
    var package = JsonSerializer.Deserialize<Tournament>(packageJson);
    if (package is { Id: not null }) question.TournamentId = package.Id.Value;

    return question.ToString();
}

async Task OnMessage(Message msg, UpdateType type)
{
    if (msg.Text is null)
    {
        await bot.SendMessage(msg.Chat, ServiceLines.noTextError);
        return;
    }

    var (isMatch, questionId) = TryGetQuestionIdAsync(msg.Text);
    if (!isMatch)
    {
        await bot.SendMessage(msg.Chat, ServiceLines.noLinkError);
        return;
    }

    var question = await GetQuestionAsync(questionId.Value);
    if (string.IsNullOrEmpty(question))
    {
        await bot.SendMessage(msg.Chat, ServiceLines.questionReceiveError);
        return;
    }

    await bot.SendMessage(msg.Chat, $"```\n{question}\n```", ParseMode.MarkdownV2);
}

static (bool isMatch, int? questionId) TryGetQuestionIdAsync(string messageText)
{
    var match = Regex.Match(messageText, ServiceLines.pattern);
    if (!match.Success) return (false, null);

    var numberString = match.Groups[1].Value;
    var number = int.Parse(numberString);
    return (true, number);
}