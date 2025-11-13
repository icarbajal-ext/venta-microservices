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

// Sample products data
var products = new List<Product>
{
    new Product(1, "Laptop Dell", "Laptop Dell Inspiron 15", 799.99m, "Electronics"),
    new Product(2, "iPhone 15", "Smartphone Apple iPhone 15", 999.99m, "Electronics"),
    new Product(3, "Desk Chair", "Ergonomic office chair", 249.99m, "Furniture")
};

// Public endpoint - no authentication required
app.MapGet("/products", () =>
{
    return Results.Ok(products);
})
.WithName("GetProducts")
.WithOpenApi();

// Protected endpoint - requires authentication
app.MapGet("/products/{id}", (int id) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);
    return product != null ? Results.Ok(product) : Results.NotFound();
})
.RequireAuthorization()
.WithName("GetProduct")
.WithOpenApi();

// Protected endpoint - requires authentication
app.MapPost("/products", (Product product) =>
{
    var newProduct = product with { Id = products.Max(p => p.Id) + 1 };
    products.Add(newProduct);
    return Results.Created($"/products/{newProduct.Id}", newProduct);
})
.RequireAuthorization()
.WithName("CreateProduct")
.WithOpenApi();

app.Run();

record Product(int Id, string Name, string Description, decimal Price, string Category);
