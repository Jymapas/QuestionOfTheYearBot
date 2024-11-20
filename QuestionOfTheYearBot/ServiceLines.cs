namespace QuestionOfTheYearBot;

public abstract record ServiceLines
{
    public const string packageApiBase = "https://gotquestions.online/api/pack/";
    public const string questionApiBase = "https://gotquestions.online/api/question/";
    public const string noLinkError = "В сообщении нет ссылки на вопрос.";
    public const string noTextError = "Бот поддерживает только работу с текстом.";
    public const string questionReceiveError = "Не удалось вопрос по этой ссылке.";
}