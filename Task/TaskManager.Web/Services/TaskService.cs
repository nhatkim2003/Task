using System.Net.Http.Json;
using TaskManager.Web.Models;

namespace TaskManager.Web.Services;

public class TaskService : ITaskService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private const string API_CLIENT_NAME = "TaskAPI";

    public TaskService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient CreateClient()
    {
        return _httpClientFactory.CreateClient(API_CLIENT_NAME);
    }

    public async Task<IEnumerable<TodoTask>> GetTasksAsync()
    {
        var client = CreateClient();
        return await client.GetFromJsonAsync<IEnumerable<TodoTask>>("api/tasks") ?? Array.Empty<TodoTask>();
    }

    public async Task<TodoTask> GetTaskByIdAsync(int id)
    {
        var client = CreateClient();
        return await client.GetFromJsonAsync<TodoTask>($"api/tasks/{id}")
            ?? throw new Exception("Không tìm thấy công việc");
    }

    public async Task<TodoTask> CreateTaskAsync(TodoTask task)
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("api/tasks", task);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TodoTask>()
            ?? throw new Exception("Không thể tạo công việc");
    }

    public async Task<TodoTask> UpdateTaskAsync(TodoTask task)
    {
        var client = CreateClient();
        var response = await client.PutAsJsonAsync($"api/tasks/{task.Id}", task);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TodoTask>()
            ?? throw new Exception("Không thể cập nhật công việc");
    }

    public async Task DeleteTaskAsync(int id)
    {
        var client = CreateClient();
        var response = await client.DeleteAsync($"api/tasks/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task CompleteTaskAsync(int id)
    {
        var client = CreateClient();
        var response = await client.PatchAsync($"api/tasks/{id}/complete", null);
        response.EnsureSuccessStatusCode();
    }
}
