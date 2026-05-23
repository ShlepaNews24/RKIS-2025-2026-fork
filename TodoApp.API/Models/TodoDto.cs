using TodoApp.Models;

namespace TodoApp.API.Models;

public class TodoItemDto
{
    public int Id { get; set; }
    public string Text { get; set; } = "";
    public TodoStatus Status { get; set; }
    public DateTime LastUpdate { get; set; }
}

public class CreateTodoDto
{
    public string Text { get; set; } = "";
    public TodoStatus Status { get; set; } = TodoStatus.NotStarted;
}

public class UpdateTodoDto
{
    public string? Text { get; set; }
    public TodoStatus? Status { get; set; }
}