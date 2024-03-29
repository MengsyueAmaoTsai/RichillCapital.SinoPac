using DotNetEnv;
using RichillCapital.SinoPac.Sor;

var envPath = "./Examples/RichillCapital.SinoPac.Sor.ConsoleExample/.env";
Env.Load(envPath);

var client = new SorClient();

var credentials = GetCredentials();
client.Connect(credentials.UserId, credentials.Password);
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



(string UserId, string Password) GetCredentials()
{
    var userId = Environment.GetEnvironmentVariable("USER_ID");
    var password = Environment.GetEnvironmentVariable("PASSWORD");

    if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password))
    {
        throw new Exception("USER_ID and PASSWORD environment variables must be set");
    }

    return (userId, password);
}