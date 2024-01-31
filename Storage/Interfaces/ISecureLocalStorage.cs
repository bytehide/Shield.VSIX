using System.Collections.Generic;

namespace ShieldVSExtension.Storage
{
    internal interface ISecureLocalStorage
    {
        int Count { get; }
        void Clear();
        bool Exists();
        bool Exists(string key);
        string Get(string key);
        T Get<T>(string key);
        IReadOnlyCollection<string> Keys();
        void Remove(string key);
        void Set<T>(string key, T data);
    }
}
