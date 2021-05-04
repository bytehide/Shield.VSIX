using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ShieldVSExtension.InternalSecureStorage
{
    public class SecureLocalStorage : ISecureLocalStorage
    {
        internal ISecureLocalStorageConfig Config { get; }
        internal Dictionary<string,string> StoredData { get; set; }
        private byte[] Key { get; }

        internal void CreateIfNotExists(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
        public SecureLocalStorage(ISecureLocalStorageConfig configuration)
        {
            Config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            CreateIfNotExists(Config.StoragePath);
            Key = Encoding.UTF8.GetBytes(Config.BuildLocalSecureKey());
            Read();
        }


        internal byte[] EncryptData(string data, byte[] key, DataProtectionScope scope)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length <= 0)
                throw new ArgumentException("data");
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (key.Length <= 0)
                throw new ArgumentException("key");

            return ProtectedData.Protect(Encoding.UTF8.GetBytes(data), key, scope);
        }

        internal string DecryptData(byte[] data, byte[] key, DataProtectionScope scope)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length <= 0)
                throw new ArgumentException("data");
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (key.Length <= 0)
                throw new ArgumentException("key");

            return Encoding.UTF8.GetString(ProtectedData.Unprotect(data, key, scope));
        }

        internal void Read()
            => StoredData = 
                File.Exists(Path.Combine(Config.StoragePath, "default")) ?
                    JsonSerializer.Deserialize<Dictionary<string, string>>(DecryptData(File.ReadAllBytes(Path.Combine(Config.StoragePath,"default")), Key, DataProtectionScope.LocalMachine)) :
                new Dictionary<string,string>();
        

        internal void Write()
            => File.WriteAllBytes(Path.Combine(Config.StoragePath, "default"),
                EncryptData(JsonSerializer.Serialize(StoredData), Key, DataProtectionScope.LocalMachine));

        public int Count => StoredData.Count;

        public void Clear()
        
         => File.Delete(Path.Combine(Config.StoragePath, "default"));
        

        public bool Exists()
            => File.Exists(Path.Combine(Config.StoragePath, "default"));

        public bool Exists(string key)
            => StoredData.ContainsKey(key);

        public string Get(string key)
        
            => !StoredData.TryGetValue(key, out var value) ? default : JsonSerializer.Deserialize<string>(value ?? string.Empty);
        

        public T Get<T>(string key)
        
            => !StoredData.TryGetValue(key, out var value) ? default : JsonSerializer.Deserialize<T>(value ?? string.Empty);
        

        public IReadOnlyCollection<string> Keys()
        
            => StoredData.Keys;
        

        public void Remove(string key)
        {
            StoredData.Remove(key);
            Write();
        }

        public void Set<T>(string key, T data)
        {
            if(Exists(key))
                Remove(key);
            StoredData.Add(key, JsonSerializer.Serialize(data));
            Write();
        }
    }
}
