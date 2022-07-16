using System;
using System.Net;
using hosts.net;

var hosts = Hosts.OpenHostsFile();

foreach (var entry in hosts.Entries)
{
    Console.WriteLine($"{entry.Type} {entry.RawString}");
}

hosts.AddBlankEntry()
    .SetHost(IPAddress.Loopback, "localhost", "google.com", "apple.com");

foreach (var entry in hosts.Entries)
{
    Console.WriteLine($"{entry.Type} {entry.RawString}");
}

hosts.Write();