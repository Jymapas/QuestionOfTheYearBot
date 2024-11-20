using System.Text;
using System.Text.Json.Serialization;

internal class Question
{
    [JsonPropertyName("packTitle")] public string TournamentName { get; init; }
    [JsonPropertyName("packId")] public int PackageId { get; init; }
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
        if (!string.IsNullOrEmpty(HandoutText) || !string.IsNullOrEmpty(HandoutPic))
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