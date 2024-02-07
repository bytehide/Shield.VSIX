using System;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace ShieldVSExtension.Common.Extensions;

public static class DirectoryInfoExtensions
{
    public static bool IsReadable(this DirectoryInfo directoryInfo)
    {
        AuthorizationRuleCollection rules;
        WindowsIdentity identity;
        try
        {
            rules = directoryInfo?.GetAccessControl().GetAccessRules(true, true, typeof(SecurityIdentifier));
            identity = WindowsIdentity.GetCurrent();
        }
        catch (UnauthorizedAccessException uae)
        {
            Debug.WriteLine(uae.ToString());
            return false;
        }

        if (null == rules || 0 == rules.Count) return false;

        var isAllow = false;
        var uSid = identity.User?.Value;

        foreach (FileSystemAccessRule rule in rules)
        {
            if (null == identity.Groups) continue;
            if (rule.IdentityReference.ToString() != uSid &&
                !identity.Groups.Contains(rule.IdentityReference)) continue;

            if ((rule.FileSystemRights.HasFlag(FileSystemRights.Read) ||
                 rule.FileSystemRights.HasFlag(FileSystemRights.ReadAttributes) ||
                 rule.FileSystemRights.HasFlag(FileSystemRights.ReadData)) &&
                rule.AccessControlType == AccessControlType.Deny)
                return false;

            if (rule.FileSystemRights.HasFlag(FileSystemRights.Read) &&
                rule.FileSystemRights.HasFlag(FileSystemRights.ReadAttributes) &&
                rule.FileSystemRights.HasFlag(FileSystemRights.ReadData) &&
                rule.AccessControlType == AccessControlType.Allow)
                isAllow = true;
        }

        return isAllow;
    }

    public static bool IsWritable(this DirectoryInfo directoryInfo)
    {
        AuthorizationRuleCollection rules;
        WindowsIdentity identity;
        try
        {
            rules = directoryInfo?.GetAccessControl().GetAccessRules(true, true, typeof(SecurityIdentifier));
            identity = WindowsIdentity.GetCurrent();
        }
        catch (UnauthorizedAccessException uae)
        {
            Debug.WriteLine(uae.ToString());
            return false;
        }

        if (null == rules || 0 == rules.Count) return false;

        var isAllow = false;
        var uSid = identity.User?.Value;

        foreach (FileSystemAccessRule rule in rules)
        {
            if (null == identity.Groups) continue;
            if (rule.IdentityReference.ToString() != uSid &&
                !identity.Groups.Contains(rule.IdentityReference)) continue;

            if ((rule.FileSystemRights.HasFlag(FileSystemRights.Write) ||
                 rule.FileSystemRights.HasFlag(FileSystemRights.WriteAttributes) ||
                 rule.FileSystemRights.HasFlag(FileSystemRights.WriteData) ||
                 rule.FileSystemRights.HasFlag(FileSystemRights.CreateDirectories) ||
                 rule.FileSystemRights.HasFlag(FileSystemRights.CreateFiles)) &&
                rule.AccessControlType == AccessControlType.Deny)
                return false;
            if (rule.FileSystemRights.HasFlag(FileSystemRights.Write) &&
                rule.FileSystemRights.HasFlag(FileSystemRights.WriteAttributes) &&
                rule.FileSystemRights.HasFlag(FileSystemRights.WriteData) &&
                rule.FileSystemRights.HasFlag(FileSystemRights.CreateDirectories) &&
                rule.FileSystemRights.HasFlag(FileSystemRights.CreateFiles) &&
                rule.AccessControlType == AccessControlType.Allow)
                isAllow = true;
        }

        return isAllow;
    }
}