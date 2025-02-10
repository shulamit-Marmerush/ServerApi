using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TodoApi;




var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "My API",
        Version = "v1"
    });
});

// הוסף שירותי CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin() // מאפשר לכל הדומיינים
            .AllowAnyMethod() // מאפשר לכל שיטות ה-HTTP (GET, POST, PUT, DELETE)
            .AllowAnyHeader()); // מאפשר לכל הכותרות
});
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddDbContext<ToDoDbContext>(opt =>
    opt.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"), 
                 new MySqlServerVersion(new Version(8, 0,41 ))));


var app = builder.Build();

// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
        options.RoutePrefix = string.Empty; 
    });
// }



app.UseCors("AllowAll");


app.MapGet("/items", async (ToDoDbContext db) => await db.Items.ToListAsync());
app.MapGet("/items/{id}", async (int id, ToDoDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null) 
        return Results.NotFound();
    
    return Results.Ok(item);
});

app.MapPost("/items", async (ToDoDbContext db, Item item) =>
{
    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/items/{item.Id}", item);
});

app.MapPut("/items/{id}", async (int id, ToDoDbContext db, Item updatedItem) => 
{
    var item = await db.Items.FindAsync(id);
    if (item is null) 
        return Results.NotFound();
    
    item.Name = updatedItem.Name; 
    // item.IsComplete = updatedItem.IsComplete;

    await db.SaveChangesAsync();
    return Results.NoContent();
});




app.MapDelete("/items/{id}", async (int id, ToDoDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null) 
        return Results.NotFound();
    
    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
});
app.MapGet("/",()=>"server api is running");



app.Run();

