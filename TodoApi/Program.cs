using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;
using Scalar.AspNetCore;

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

// Routes
var todos = app.MapGroup("/todos");

// GET all
todos.MapGet("/", async (TodoDb db) =>
    await db.Todos.ToListAsync());

// GET by id
todos.MapGet("/{id:int}", async (int id, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);
    return todo is not null ? Results.Ok(todo) : Results.NotFound();
});

// CREATE
todos.MapPost("/", async (Todo todo, TodoDb db) =>
{
    if (string.IsNullOrWhiteSpace(todo.Title))
        return Results.BadRequest("Title is required");

    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    return Results.Created($"/todos/{todo.Id}", todo);
});

// UPDATE
todos.MapPut("/{id:int}", async (int id, Todo updated, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo is null) return Results.NotFound();

    todo.Title = updated.Title;
    todo.IsCompleted = updated.IsCompleted;

    await db.SaveChangesAsync();
    return Results.Ok(todo);
});

// DELETE
todos.MapDelete("/{id:int}", async (int id, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo is null) return Results.NotFound();

    db.Todos.Remove(todo);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.Run();

