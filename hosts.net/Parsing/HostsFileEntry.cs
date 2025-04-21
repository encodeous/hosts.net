using System;
using System.Linq;
using System.Net;

namespace hosts.net.Parsing;

public class HostsFileEntry
{
    internal static readonly char[] _trimChars = {'\r', '\n', ' ', '\t'};
    internal HostsFileEntry(){}
    /// <summary>
    /// Read/Write the raw host entry when save is called.
    /// </summary>
    public string RawString { get; set; }

    /// <summary>
    /// Gets the type of hosts entry of this file.
    /// </summary>
    public EntryType Type
    {
        get
        {
            if (CommentEntry.IsValidString(RawString)) return EntryType.Comment;
            if (HostEntry.IsValidString(RawString)) return EntryType.Host;
            if (BlankEntry.IsValidString(RawString)) return EntryType.Blank;
            return EntryType.Unparsable;
        }
    }

    public bool IsValidString()
    {
        return Type != EntryType.Unparsable;
    }

    /// <summary>
    /// Checks if the string is valid once parsed
    /// </summary>
    public bool IsValid => IsValidString();

    /// <summary>
    /// Gets or sets the comment without the preceding <c>#</c> sign
    /// </summary>
    public string? Comment {
        get
        {
            if (!CommentEntry.IsValidString(RawString)) return default;
            return RawString[1..].Trim(_trimChars);
        }
        set
        {
            RawString = "# " + (value.Trim(_trimChars));
        }
    }
    
    /// <summary>
    /// Gets the IP Address of the host entry
    /// </summary>
    public IPAddress? Address {
        get
        {
            if (!HostEntry.IsValidString(RawString)) return default;
            var tokens = HostEntry.GetTokens(RawString);
            return IPAddress.Parse(tokens[0]);
        }
    }
    
    /// <summary>
    /// Gets the canonical (primary) hostname for the entry
    /// </summary>
    public string? CanonicalHostname {
        get
        {
            if (!HostEntry.IsValidString(RawString)) return default;
            var tokens = HostEntry.GetTokens(RawString);
            return tokens[1];
        }
    }
    
    /// <summary>
    /// Gets the aliases for the entry
    /// </summary>
    public string[]? HostnameAliases {
        get
        {
            if (!HostEntry.IsValidString(RawString)) return default;
            var tokens = HostEntry.GetTokens(RawString);
            if (tokens.Length == 2) return Array.Empty<string>();
            return tokens[2..];
        }
    }
    
    /// <summary>
    /// Sets the value of the current entry to a host entry
    /// </summary>
    /// <param name="address">the ip address of the host</param>
    /// <param name="canonicalHostname">the canonical (primary) hostname for the entry</param>
    /// <param name="aliases">the aliases</param>
    public HostsFileEntry SetHost(IPAddress address, string canonicalHostname, params string[] aliases)
    {
        if (!HostEntry.IsValidHostname(canonicalHostname) || aliases.Any(e => !HostEntry.IsValidHostname(e)))
        {
            throw new FormatException("The specified hostname(s) are not valid");
        }

        var primaryHostEntry = $"{address} {canonicalHostname}";
        if (aliases.Any())
        {
            primaryHostEntry += " " + string.Join(' ', aliases);
        }

        RawString = primaryHostEntry;
        return this;
    }

    /// <summary>
    /// Sets the value of the current entry to a comment
    /// </summary>
    /// <param name="comment"></param>
    /// <returns></returns>
    public HostsFileEntry SetComment(string comment)
    {
        Comment = comment;
        return this;
    }
    
    /// <summary>
    /// Sets the current entry to be a blank entry
    /// </summary>
    /// <returns></returns>
    public HostsFileEntry SetBlank()
    {
        RawString = "";
        return this;
    }

    /// <summary>
    /// Creates a new & blank entry
    /// </summary>
    /// <returns></returns>
    public static HostsFileEntry NewEntry()
    {
        return new HostsFileEntry();
    }
}

internal static class CommentEntry
{
    public static bool IsValidString(string value)
    {
        return value.Any() && value.StartsWith("#");
    }
}
internal static class BlankEntry
{
    public static bool IsValidString(string value)
    {
        return !value.Any();
    }
}
internal static class HostEntry
{
    public static bool IsValidString(string value)
    {
        var tokens = GetTokens(value);
        if (tokens.Length < 2) return false;
        if (!IPAddress.TryParse(tokens[0], out _)) return false;
        if (tokens[1..].Any(tok => !IsValidHostname(tok))) return false;
        return true;
    }

    public static string[] GetTokens(string value)
    {
        return value.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// According to man7
    /// Host names may contain only alphanumeric characters, minus signs ("-"), and periods (".").
    /// They must begin with an alphabetic character and end with an alphanumeric character.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsValidHostname(string value)
    {
        var name = value.Trim(HostsFileEntry._trimChars);
        if (!name.Any()) return false;
        if (name.Any(c =>
            {
                if(!char.IsAscii(c)) return true;
                return (c != '-' && c != '.' && !IsAlphaNum(c));
            })) return false;
        if (!char.IsAlphaNum(name[0]) || !IsAlphaNum(name[^1]))
        {
            return false;
        }

        return true;
    }
    
    private static bool IsAlphaNum(char c)
    {
        return char.IsLetter(c) || char.IsDigit(c);
    }
}