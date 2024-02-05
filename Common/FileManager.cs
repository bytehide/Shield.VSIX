using System.Diagnostics;
using System.IO;
using ShieldVSExtension.Common.Extensions;

namespace ShieldVSExtension.Common;

internal static class FileManager
{
    public static bool ExistShieldConfigurationFile(string path)
    {
        if (path.IsEmpty()) return false;

        var file = new FileInfo(path);
        return file.Exists;
    }

    public static (bool, string) CopyFileToPath(string sourcePath, string targetPath, string name,
        string ext = "", bool move = false,
        bool check = false, string rename = "")
    {
        if (!Directory.Exists(sourcePath) || !Directory.Exists(targetPath)) return (false, string.Empty);

        if (check)
        {
            if (!CheckDirectory(sourcePath) || !CheckDirectory(targetPath))
            {
                return (false, string.Empty);
            }
        }

        if (null != ext && !ext.IsEmpty()) name = $"{name}.{ext}";

        string target;

        try
        {
            var source = Path.Combine(sourcePath, name);
            if (!File.Exists(source)) return (false, string.Empty);

            target = Path.Combine(targetPath, rename.IsEmpty() ? name : rename);

            if (File.Exists(target)) return (false, string.Empty);

            if (move) File.Move(source, target);
            else File.Copy(source, target);
        }
        catch (IOException ioe)
        {
            Debug.WriteLine(ioe.ToString());
            return (false, string.Empty);
        }

        return (!target.IsEmpty() && File.Exists(target), target);
    }

    public static bool CheckDirectory(string path, bool isDir = true, bool checkR = true, bool checkW = true)
    {
        DirectoryInfo dir;
        if (isDir)
        {
            dir = new DirectoryInfo(path);
        }
        else
        {
            FileInfo file = new(path);
            if (!file.Exists) return false;
            dir = file.Directory;
        }

        if (dir is not { Exists: true }) return false;

        return checkR switch
        {
            true when checkW => dir.IsReadable() && dir.IsWritable(),
            true => dir.IsReadable(),
            _ => checkW && dir.IsWritable()
        };
    }

    public static string GetParentDirFromFile(string path)
    {
        if (path.IsEmpty()) return string.Empty;

        FileInfo source = new(path);
        if (!source.Exists) return string.Empty;

        var dir = source.Directory;
        return dir is not { Exists: true } ? string.Empty : dir.FullName;
    }

    public static bool WriteJsonShieldConfiguration(string path, string content)
    {
        if (path.IsEmpty() || content.IsEmpty()) return false;

        if (!CheckDirectory(path)) return false;

        var file = new FileInfo(Path.Combine(path, "shield.config.json"));
        // if (file.Exists) return false;

        try
        {
            File.WriteAllText(file.FullName, content);
        }
        catch (IOException ioe)
        {
            Debug.WriteLine(ioe.ToString());
            return false;
        }

        return true;
    }

    public static string ReadJsonShieldConfiguration(string path)
    {
        if (path.IsEmpty()) return string.Empty;

        var file = new FileInfo(Path.Combine(path, "shield.config.json"));
        if (!file.Exists) return string.Empty;

        try
        {
            return File.ReadAllText(file.FullName);
        }
        catch (IOException ioe)
        {
            Debug.WriteLine(ioe.ToString());
            return string.Empty;
        }
    }
}