using System.Net.NetworkInformation;

var ping = new Ping();

for ( var hop = 1; hop <=30; hop++)
{
    var reply = ping.Send("8.8.8.8", 1000, new byte[32], new PingOptions(hop, false)); 
    Console.WriteLine($"Hop: {hop} Status: {reply.Status} Address: {reply.Address}");
    if (reply.Status == IPStatus.Success) break;
}