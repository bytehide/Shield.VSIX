using System;

namespace ShieldVSExtension.Storage.Configurations
{
    public interface ISecureLocalStorageConfig
    {
        string DefaultPath { get; }
        string ApplicationName { get; }
        string StoragePath { get; }
        Func<string> BuildLocalSecureKey { get; set;  }
    }
}
