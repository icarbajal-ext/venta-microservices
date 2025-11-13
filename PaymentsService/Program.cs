using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

// Sample payments data
var payments = new List<Payment>
{
    new Payment(1, 101, 799.99m, "Credit Card", DateTime.Now.AddDays(-1), "Completed"),
    new Payment(2, 102, 999.99m, "PayPal", DateTime.Now.AddDays(-2), "Completed"),
    new Payment(3, 103, 249.99m, "Debit Card", DateTime.Now.AddHours(-3), "Pending")
};

// Protected endpoint - requires authentication
app.MapGet("/payments", () =>
{
    return Results.Ok(payments);
})
.RequireAuthorization()
.WithName("GetPayments")
.WithOpenApi();

// Protected endpoint - requires authentication
app.MapGet("/payments/{id}", (int id) =>
{
    var payment = payments.FirstOrDefault(p => p.Id == id);
    return payment != null ? Results.Ok(payment) : Results.NotFound();
})
.RequireAuthorization()
.WithName("GetPayment")
.WithOpenApi();

// Protected endpoint - requires authentication
app.MapPost("/payments", (Payment payment) =>
{
    var newPayment = payment with { 
        Id = payments.Max(p => p.Id) + 1,
        PaymentDate = DateTime.Now,
        Status = "Pending"
    };
    payments.Add(newPayment);
    return Results.Created($"/payments/{newPayment.Id}", newPayment);
})
.RequireAuthorization()
.WithName("CreatePayment")
.WithOpenApi();

// Protected endpoint - requires authentication
app.MapPut("/payments/{id}/status", (int id, string status) =>
{
    var payment = payments.FirstOrDefault(p => p.Id == id);
    if (payment == null) return Results.NotFound();
    
    var index = payments.IndexOf(payment);
    payments[index] = payment with { Status = status };
    return Results.Ok(payments[index]);
})
.RequireAuthorization()
.WithName("UpdatePaymentStatus")
.WithOpenApi();

app.Run();

record Payment(int Id, int OrderId, decimal Amount, string PaymentMethod, DateTime PaymentDate, string Status);
