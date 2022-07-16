using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;

namespace hosts.net.Parsing;

public class HostsFile
{
    internal HostsFile(string path)
    {
        Path = path;
    }

    /// <summary>
    /// Reads the hosts file from disk
    /// </summary>
    public void Read()
    {
        using var sr = new StreamReader(File.OpenRead(Path));
        Entries = new List<HostsFileEntry>();
        while (true)
        {
            var ln = sr.ReadLine();
            if (ln is null) break;
            Entries.Add(new HostsFileEntry()
            {
                RawString = ln
            });
        }
    }
    
    /// <summary>
    /// Writes the hosts file to disk
    /// </summary>
    public void Write()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var fi = new FileInfo(Path);
            // windows security moment
            FileSecurity fileS = fi.GetAccessControl();
            fi.Attributes &= (~FileAttributes.ReadOnly);
            SecurityIdentifier cu = WindowsIdentity.GetCurrent().User;
            var rule = new FileSystemAccessRule(cu, FileSystemRights.Write, AccessControlType.Allow);
            fileS.SetAccessRule(rule);
            fi.SetAccessControl(fileS);
            using var sw = new StreamWriter(Path);
            foreach (var entry in Entries)
            {
                sw.WriteLine(entry.RawString);
            }
            sw.Close();
            // restore old rules
            fi.Attributes |= FileAttributes.ReadOnly;
            fileS.RemoveAccessRule(rule);
            fi.SetAccessControl(fileS);
        }
        else
        {
            using var sw = new StreamWriter(Path);
            foreach (var entry in Entries)
            {
                sw.WriteLine(entry.RawString);
            }
        }
        
    }
    public string Path { get; private set; }
    public List<HostsFileEntry> Entries;

    public HostsFileEntry AddBlankEntry()
    {
        var ent = HostsFileEntry.NewEntry();
        Entries.Add(ent);
        return ent;
    }
}