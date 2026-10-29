using StackExchange.Redis;

sealed class MessageConsoleReader(IConnectionMultiplexer redis) : BackgroundService
{
    private const string Stream = "demo:messages";
    private const string Group = "demo-api";
    private const string Consumer = "console";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var db = redis.GetDatabase();
        var delay = TimeSpan.FromSeconds(1);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                try
                {
                    await db.StreamCreateConsumerGroupAsync(Stream, Group, "0", createStream: true);
                }
                catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP", StringComparison.OrdinalIgnoreCase))
                {
                    // O grupo já existe; pode continuar consumindo.
                }

                var entries = await db.StreamReadGroupAsync(Stream, Group, Consumer, ">", count: 10);
                foreach (var entry in entries)
                {
                    var sender = entry.Values.FirstOrDefault(v => v.Name == "sender").Value;
                    var body = entry.Values.FirstOrDefault(v => v.Name == "body").Value;
                    Console.WriteLine($"[mensagem de demonstração] {sender}: {body}");
                    await db.StreamAcknowledgeAsync(Stream, Group, entry.Id);
                }

                delay = TimeSpan.FromSeconds(1);
                if (entries.Length == 0)
                    await Task.Delay(TimeSpan.FromMilliseconds(500), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Redis indisponível ({ex.GetType().Name}); tentando novamente em {delay.TotalSeconds:0}s.");
                await Task.Delay(delay, stoppingToken);
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 30));
            }
        }
    }
}
