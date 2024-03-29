using RichillCapital.SinoPac.Sor;

var client = new SorClient();


client.Connect("P123622990", "Among720!");
await Wait();

var firstAccount = client.GetAccounts().FirstOrDefault();

if (firstAccount is null)
{
    Console.WriteLine("No accounts found.");
    return;
}



client.Disconnect();
await Wait();


async Task Wait() => await Task.Delay(1500);