using TaskManager.Web.Models;

public interface ITaskService
{
    Task<IEnumerable<TodoTask>> GetTasksAsync();
    Task<TodoTask> GetTaskByIdAsync(int id);
    Task<TodoTask> CreateTaskAsync(TodoTask task);
    Task<TodoTask> UpdateTaskAsync(TodoTask task);
    Task DeleteTaskAsync(int id);
    Task CompleteTaskAsync(int id);
}
