using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;
using Scalar.AspNetCore;
using TodoApi.Routes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<TodoDb>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TodoDb>();
    db.Database.EnsureCreated();

    if (!db.Todos.Any())
    {
        db.Todos.AddRange(
            new Todo { Title = "Learn ASP.NET Core", IsCompleted = false },
            new Todo { Title = "Build Todo API", IsCompleted = true },
            new Todo { Title = "Test with Scalar", IsCompleted = false }
        );

        db.SaveChanges();
    }
}

app.MapTodoRoutes();

app.Run();
