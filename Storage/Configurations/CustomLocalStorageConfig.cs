using System;
using System.IO;
using DeviceId;

namespace ShieldVSExtension.Storage.Configurations
{
    public class CustomLocalStorageConfig : ISecureLocalStorageConfig
    {
        public CustomLocalStorageConfig(string defaultPath, string applicationName)
        {
            DefaultPath     = defaultPath ?? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            ApplicationName = applicationName;
            StoragePath     = Path.Combine(DefaultPath, applicationName);
        }

        public CustomLocalStorageConfig(string defaultPath, string applicationName, string key)
        {
            DefaultPath         = defaultPath ?? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            ApplicationName     = applicationName;
            StoragePath         = Path.Combine(DefaultPath, applicationName);
            BuildLocalSecureKey = () => key;
        }

        public CustomLocalStorageConfig WithDefaultKeyBuilder()
        {
            BuildLocalSecureKey = () => new DeviceIdBuilder()
                                       .AddMachineName()
                                       .AddOsVersion()
                                       .OnWindows(windows => windows
                                                            .AddProcessorId()
                                                            .AddMotherboardSerialNumber()
                                                            .AddSystemDriveSerialNumber())
                                       .ToString();

            return this;
        }

        public string DefaultPath { get; }
        public string ApplicationName { get; }
        public string StoragePath { get; }
        public Func<string> BuildLocalSecureKey { get; set; }
    }
}