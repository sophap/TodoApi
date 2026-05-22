using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Routes;

public static class TodoRoutes
{
    public static void MapTodoRoutes(this WebApplication app)
    {
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
    }
}
