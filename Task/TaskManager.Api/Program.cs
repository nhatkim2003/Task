    using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TaskManager.Api.Data;
using TaskManager.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// Thêm appsettings.Development.json vào cấu hình
builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);


// Add services to the container.
builder.Services.AddControllers();

// Configure MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? 
    "server=localhost;port=3306;database=taskmanager;user=root;password=;";
builder.Services.AddDbContext<TaskDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "your-256-bit-secret-used-for-jwt-token-generation"))

        };
    });

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "Task Manager API", 
        Version = "v1",
        Description = "API for Task Manager application"
    });

    // Add JWT Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Manager API v1");
    c.RoutePrefix = string.Empty; // Serve Swagger UI at the root
});

// Use CORS before routing
app.UseCors("AllowAll");

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ensure database is created and seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
    db.Database.EnsureCreated();

    // Add sample data if the database is empty
    if (!db.Tasks.Any())
    {
        var sampleTasks = new List<TodoTask>
        {
            new TodoTask
            {
                Title = "Complete project documentation",
                Description = "Write comprehensive documentation for the Task Manager project",
                DueDate = DateTime.Today.AddDays(3),
                IsCompleted = false
            },
            new TodoTask
            {
                Title = "Review code changes",
                Description = "Review and approve pending pull requests",
                DueDate = DateTime.Today,
                IsCompleted = false
            },
            new TodoTask
            {
                Title = "Setup development environment",
                Description = "Install and configure all necessary development tools",
                DueDate = DateTime.Today.AddDays(-1),
                IsCompleted = true
            },
            new TodoTask
            {
                Title = "Team meeting",
                Description = "Weekly team sync-up meeting",
                DueDate = DateTime.Today.AddDays(1),
                IsCompleted = false
            },
            new TodoTask
            {
                Title = "Update dependencies",
                Description = "Update all project dependencies to their latest versions",
                DueDate = DateTime.Today.AddDays(5),
                IsCompleted = false
            },
            new TodoTask
            {
                Title = "Fix reported bugs",
                Description = "Address and fix all reported issues in the bug tracker",
                DueDate = DateTime.Today.AddDays(-2),
                IsCompleted = false
            },
            new TodoTask
            {
                Title = "Write unit tests",
                Description = "Increase test coverage for core functionality",
                DueDate = DateTime.Today.AddDays(4),
                IsCompleted = false
            },
            new TodoTask
            {
                Title = "Deploy to staging",
                Description = "Deploy latest changes to staging environment",
                DueDate = DateTime.Today.AddDays(2),
                IsCompleted = true
            },
            new TodoTask
            {
                Title = "Performance optimization",
                Description = "Optimize application performance and loading times",
                DueDate = DateTime.Today.AddDays(7),
                IsCompleted = false
            },
            new TodoTask
            {
                Title = "Security audit",
                Description = "Conduct security review of the application",
                DueDate = DateTime.Today.AddDays(6),
                IsCompleted = false
            }
        };

        db.Tasks.AddRange(sampleTasks);
        db.SaveChanges();
    }
}

// Set the port explicitly
app.Urls.Add("http://localhost:5200");

// Launch browser
var url = "http://localhost:5200";
try
{
    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
    {
        FileName = url,
        UseShellExecute = true
    });
}
catch
{
    // If browser launch fails, just continue
}

app.Run();
