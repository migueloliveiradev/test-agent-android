namespace TodoApi.Dtos;

public record TaskRequest(string Title, string Content, DateTime Date, string Status);
