using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// JWT Authentication
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Sample logs data
var logs = new List<LogEntry>
{
    new LogEntry(1, "UsersService", "INFO", "User login successful", "admin", DateTime.Now.AddMinutes(-30)),
    new LogEntry(2, "ProductsService", "INFO", "Product retrieved", "user1", DateTime.Now.AddMinutes(-25)),
    new LogEntry(3, "PaymentsService", "ERROR", "Payment processing failed", "user2", DateTime.Now.AddMinutes(-20)),
    new LogEntry(4, "UsersService", "INFO", "User registered", "user3", DateTime.Now.AddMinutes(-15)),
    new LogEntry(5, "ProductsService", "INFO", "Product created", "admin", DateTime.Now.AddMinutes(-10))
};

// Protected endpoint - Get all logs (Admin only)
app.MapGet("/logs", (HttpContext context) =>
{
    var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
    if (userRole != "Admin")
    {
        return Results.Forbid();
    }
    
    return Results.Ok(logs.OrderByDescending(l => l.Timestamp));
})
.RequireAuthorization()
.WithName("GetLogs")
.WithOpenApi();

// Protected endpoint - Get logs by service
app.MapGet("/logs/service/{service}", (string service, HttpContext context) =>
{
    var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
    if (userRole != "Admin")
    {
        return Results.Forbid();
    }
    
    var serviceLogs = logs.Where(l => l.Service.Equals(service, StringComparison.OrdinalIgnoreCase))
                         .OrderByDescending(l => l.Timestamp);
    
    return Results.Ok(serviceLogs);
})
.RequireAuthorization()
.WithName("GetLogsByService")
.WithOpenApi();

// Protected endpoint - Get logs by level
app.MapGet("/logs/level/{level}", (string level, HttpContext context) =>
{
    var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
    if (userRole != "Admin")
    {
        return Results.Forbid();
    }
    
    var levelLogs = logs.Where(l => l.Level.Equals(level, StringComparison.OrdinalIgnoreCase))
                       .OrderByDescending(l => l.Timestamp);
    
    return Results.Ok(levelLogs);
})
.RequireAuthorization()
.WithName("GetLogsByLevel")
.WithOpenApi();

// Protected endpoint - Add new log entry
app.MapPost("/logs", (LogCreateRequest request, HttpContext context) =>
{
    var username = context.User.FindFirst(ClaimTypes.Name)?.Value ?? "Anonymous";
    
    var newLog = new LogEntry(
        logs.Max(l => l.Id) + 1,
        request.Service,
        request.Level,
        request.Message,
        username,
        DateTime.Now
    );
    
    logs.Add(newLog);
    
    return Results.Created($"/logs/{newLog.Id}", newLog);
})
.RequireAuthorization()
.WithName("CreateLog")
.WithOpenApi();

// Protected endpoint - Get logs by date range
app.MapGet("/logs/date-range", (DateTime from, DateTime to, HttpContext context) =>
{
    var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
    if (userRole != "Admin")
    {
        return Results.Forbid();
    }
    
    var dateRangeLogs = logs.Where(l => l.Timestamp >= from && l.Timestamp <= to)
                           .OrderByDescending(l => l.Timestamp);
    
    return Results.Ok(dateRangeLogs);
})
.RequireAuthorization()
.WithName("GetLogsByDateRange")
.WithOpenApi();

app.Run();

record LogEntry(int Id, string Service, string Level, string Message, string Username, DateTime Timestamp);
record LogCreateRequest(string Service, string Level, string Message);
