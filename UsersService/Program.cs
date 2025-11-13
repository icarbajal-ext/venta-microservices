using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
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

// Sample users data (in a real app, this would come from a database)
var users = new List<User>
{
    new User(1, "admin", "admin@example.com", "password123", "Admin", DateTime.Now.AddMonths(-6)),
    new User(2, "user1", "user1@example.com", "password123", "User", DateTime.Now.AddMonths(-3)),
    new User(3, "user2", "user2@example.com", "password123", "User", DateTime.Now.AddMonths(-1))
};

// Public endpoint - Login
app.MapPost("/auth/login", (LoginRequest request) =>
{
    var user = users.FirstOrDefault(u => u.Username == request.Username && u.Password == request.Password);
    
    if (user == null)
    {
        return Results.Unauthorized();
    }

    var token = GenerateJwtToken(user, builder.Configuration);
    
    return Results.Ok(new LoginResponse(token, user.Id, user.Username, user.Email, user.Role));
})
.WithName("Login")
.WithOpenApi();

// Public endpoint - Register
app.MapPost("/auth/register", (RegisterRequest request) =>
{
    if (users.Any(u => u.Username == request.Username || u.Email == request.Email))
    {
        return Results.BadRequest("User already exists");
    }

    var newUser = new User(
        users.Max(u => u.Id) + 1,
        request.Username,
        request.Email,
        request.Password, // In a real app, hash the password
        "User",
        DateTime.Now
    );
    
    users.Add(newUser);
    
    var token = GenerateJwtToken(newUser, builder.Configuration);
    
    return Results.Created($"/users/{newUser.Id}", new LoginResponse(token, newUser.Id, newUser.Username, newUser.Email, newUser.Role));
})
.WithName("Register")
.WithOpenApi();

// Protected endpoint - Get current user profile
app.MapGet("/users/profile", (HttpContext context) =>
{
    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userId == null) return Results.Unauthorized();
    
    var user = users.FirstOrDefault(u => u.Id.ToString() == userId);
    if (user == null) return Results.NotFound();
    
    return Results.Ok(new UserProfile(user.Id, user.Username, user.Email, user.Role, user.CreatedAt));
})
.RequireAuthorization()
.WithName("GetProfile")
.WithOpenApi();

// Protected endpoint - Get all users (Admin only)
app.MapGet("/users", (HttpContext context) =>
{
    var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
    if (userRole != "Admin")
    {
        return Results.Forbid();
    }
    
    var userProfiles = users.Select(u => new UserProfile(u.Id, u.Username, u.Email, u.Role, u.CreatedAt));
    return Results.Ok(userProfiles);
})
.RequireAuthorization()
.WithName("GetAllUsers")
.WithOpenApi();

// Protected endpoint - Get user by ID
app.MapGet("/users/{id}", (int id, HttpContext context) =>
{
    var currentUserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
    
    // Users can only access their own profile, unless they're admin
    if (currentUserId != id.ToString() && userRole != "Admin")
    {
        return Results.Forbid();
    }
    
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user == null) return Results.NotFound();
    
    return Results.Ok(new UserProfile(user.Id, user.Username, user.Email, user.Role, user.CreatedAt));
})
.RequireAuthorization()
.WithName("GetUser")
.WithOpenApi();

app.Run();

static string GenerateJwtToken(User user, IConfiguration configuration)
{
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role)
    };

    var token = new JwtSecurityToken(
        issuer: configuration["Jwt:Issuer"],
        audience: configuration["Jwt:Audience"],
        claims: claims,
        expires: DateTime.Now.AddMinutes(double.Parse(configuration["Jwt:ExpireMinutes"])),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

record User(int Id, string Username, string Email, string Password, string Role, DateTime CreatedAt);
record LoginRequest(string Username, string Password);
record RegisterRequest(string Username, string Email, string Password);
record LoginResponse(string Token, int UserId, string Username, string Email, string Role);
record UserProfile(int Id, string Username, string Email, string Role, DateTime CreatedAt);
