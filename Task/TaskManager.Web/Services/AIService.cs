using System.Text;
using System.Text.Json;
using System.Net.Http.Json;

namespace TaskManager.Web.Services;

public class AIService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _openAiApiKey;

    public AIService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _openAiApiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("OpenAI API Key is not configured");
    }

    public async Task<TaskAnalysis> AnalyzeTaskAsync(string title, string description)
    {
        var prompt = $@"Phân tích công việc sau và trả về JSON:
Tiêu đề: {title}
Mô tả: {description}

Yêu cầu:
1. Đánh giá mức độ ưu tiên (1-3, 1: Thấp, 2: Trung bình, 3: Cao)
2. Ước tính thời gian thực hiện (giờ)
3. Đề xuất thời hạn hợp lý (số ngày)
4. Gợi ý các bước thực hiện
5. Nhãn phân loại

Trả về theo định dạng JSON:
{{
    ""priority"": số (1-3),
    ""estimatedHours"": số,
    ""suggestedDueDays"": số,
    ""steps"": [""bước 1"", ""bước 2"", ...],
    ""tags"": [""nhãn 1"", ""nhãn 2"", ...]
}}";

        var analysis = await CallOpenAIAsync(prompt);
        return JsonSerializer.Deserialize<TaskAnalysis>(analysis) ?? new TaskAnalysis();
    }

    public async Task<string> GenerateDescriptionAsync(string title)
    {
        var prompt = $@"Tạo mô tả chi tiết cho công việc có tiêu đề: {title}
Mô tả cần:
1. Ngắn gọn nhưng đầy đủ thông tin
2. Nêu rõ mục tiêu
3. Có thể bao gồm các yêu cầu chính
4. Viết bằng tiếng Việt
5. Không quá 200 ký tự";

        return await CallOpenAIAsync(prompt);
    }

    public async Task<List<string>> SuggestSubtasksAsync(string title, string description)
    {
        var prompt = $@"Phân tích và đề xuất các công việc nhỏ cần thực hiện cho task sau:
Tiêu đề: {title}
Mô tả: {description}

Yêu cầu:
1. Liệt kê 3-5 công việc nhỏ
2. Mỗi công việc phải cụ thể và có thể thực hiện được
3. Sắp xếp theo thứ tự thực hiện
4. Viết bằng tiếng Việt
5. Mỗi công việc không quá 50 ký tự

Trả về danh sách JSON: [""công việc 1"", ""công việc 2"", ...]";

        var result = await CallOpenAIAsync(prompt);
        return JsonSerializer.Deserialize<List<string>>(result) ?? new List<string>();
    }

    private async Task<string> CallOpenAIAsync(string prompt)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAiApiKey}");

        var request = new
        {
            model = "gpt-3.5-turbo",
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            temperature = 0.7,
            max_tokens = 500
        };

        var response = await client.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenAIResponse>();
        return result?.choices?[0]?.message?.content ?? string.Empty;
    }

    public class TaskAnalysis
    {
        public int Priority { get; set; }
        public double EstimatedHours { get; set; }
        public int SuggestedDueDays { get; set; }
        public List<string> Steps { get; set; } = new();
        public List<string> Tags { get; set; } = new();
    }

    private class OpenAIResponse
    {
        public List<Choice> choices { get; set; } = new();

        public class Choice
        {
            public Message message { get; set; } = new();
        }

        public class Message
        {
            public string content { get; set; } = string.Empty;
        }
    }
} 