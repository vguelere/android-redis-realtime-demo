using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var redisConnection = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
builder.Services.AddHostedService<MessageConsoleReader>();

var app = builder.Build();

app.MapPost("/contacts", async (ContactRequest request, IConnectionMultiplexer redis) =>
{
    if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Number))
        return Results.ValidationProblem(new Dictionary<string, string[]> {
            ["name/number"] = ["Informe o nome e o número do contato."]
        });

    var entryId = await redis.GetDatabase().StreamAddAsync("demo:contacts", new NameValueEntry[]
    {
        new("name", request.Name.Trim()),
        new("number", request.Number.Trim())
    });

    return Results.Accepted($"/contacts/{entryId}", new { id = entryId, status = "queued" });
});

app.MapGet("/health", async (IConnectionMultiplexer redis) =>
{
    await redis.GetDatabase().PingAsync();
    return Results.Ok(new { status = "ok", redis = "connected" });
});

app.Run();

record ContactRequest(string Name, string Number);
