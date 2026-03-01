namespace TodoApi.Models;

public class TodoTask
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public DateTime Date { get; set; }
    public required string Status { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
}
