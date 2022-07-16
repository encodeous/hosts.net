# `hosts.net`

![Nuget](https://img.shields.io/nuget/v/hosts.net)

`hosts.net` is a simple library that enables developers to easily develop applications that can programmatically manipulate the hosts file across various platforms.

## `installation`

```shell
dotnet add package hosts.net
```

## `usage`

**Editing the hosts file on unix requires sudo permission, and likewise administrator is required on Windows.**

*On Unix-like Systems:* `hosts.net` will default to the `/etc/hosts` file.

*On Windows Systems:* the library will default to the `C:/Windows/System32/drivers/etc/hosts` file. The library automatically handles Windows' Access Control List. If any issues arise from this please file an issue.

**Demo Code:**

```csharp
using System.Linq;
using System.Net;
using hosts.net;

// open the file
var file = Hosts.OpenHostsFile(/* optional file path */);

// use linq to query the entries to prevent duplicate entries

if (file.Entries.Any(entry => entry.Comment is not null && entry.Comment == "Added with hosts.net")) return;

file.AddBlankEntry()
    .SetComment("Added with hosts.net");

file.AddBlankEntry()
    .SetHost(/* address */ IPAddress.Loopback, /* canonical hostname */ "non-existent.domain.n", /* optional aliases */ "non-existent.alias.n");

// other code here...

// save to the disk
file.Write();


// Result:

// # Added with hosts.net
// 127.0.0.1 non-existent.domain.n non-existent.alias.n
```

**Removing an entry with LINQ:**
```csharp
file.Entries.RemoveAll(entry => entry.CanonicalHostname == "encodeous.ca");
```

**Adding an empty line:**
```csharp
file.AddBlankEntry();
```

**Reading & Printing all of the lines:**
```csharp
Console.WriteLine(string.Join('\n', 
    file.Entries.Select(x => x.RawString))
);
```