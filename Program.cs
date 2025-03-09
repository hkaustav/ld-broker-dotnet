using System.Collections;
using System.Collections.Concurrent;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

var responseCollection = new ConcurrentDictionary<HttpResponse, bool>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var ldConnector = new LDConnector();
ldConnector.InitLDClient(responseCollection);

app.MapGet("/health", () =>
{
    return Results.Ok("Healthy");
});

app.MapGet("/flag/{name}", (string name) =>
{
    return Results.Ok(ldConnector.BoolVariation(name, false));
});

app.MapGet("/flags", () =>
{
    return Results.Ok(ldConnector.GetAllFlags());
});

app.MapGet("/sse", async (HttpContext context, CancellationToken clientDisconnectToken) =>
{
    context.Response.Headers.Append(HeaderNames.ContentType, "text/event-stream");
    context.Response.Headers.Append(HeaderNames.CacheControl, "no-cache");
    context.Response.Headers.Append(HeaderNames.Connection, "keep-alive");

    responseCollection.TryAdd(context.Response, true);

    await context.Response.Body.FlushAsync();

    try
    {
        await Task.Delay(-1, clientDisconnectToken);
    }
    catch(IOException ex)
    {
        Console.WriteLine("client disconnected {0}", ex.Message);
        responseCollection.TryRemove(context.Response, out _);
    }

});

app.Run();