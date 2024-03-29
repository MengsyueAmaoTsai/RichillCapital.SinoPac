using DotNetEnv;
using RichillCapital.SinoPac.Sor;

Env.Load("./Examples/RichillCapital.SinoPac.Sor.ConsoleExample/.env");

var userId = Env.GetString("USER_ID");
var password = Env.GetString("PASSWORD");


Console.WriteLine($"User ID: {userId}");
Console.WriteLine($"Password: {password}");

var client = new SorClient();


client.Connect(userId, password);
await Wait();

var accountsResult = client.GetAccounts();

if (accountsResult.IsFailure)
{
    Console.WriteLine(accountsResult.Error);
    return;
}

var firstAccount = accountsResult.ValueOrDefault.First();

client.QueryAccountBalance(firstAccount);
await Wait();


client.QueryAccountPositions(firstAccount);
await Wait();

client.Disconnect();
await Wait();


async Task Wait() => await Task.Delay(1500);