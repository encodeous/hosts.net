using System;
using System.Runtime.InteropServices;
using hosts.net.Parsing;

namespace hosts.net;

public class Hosts
{
    public static HostsFile OpenHostsFile(string? path = default)
    {
        if (path is null)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) 
                || RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD)
                || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                path = "/etc/hosts";
            }
            else
            {
                path = Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\drivers\etc\hosts";
            }
        }

        var hosts = new HostsFile(path);
        hosts.Read();
        return hosts;
    }
}