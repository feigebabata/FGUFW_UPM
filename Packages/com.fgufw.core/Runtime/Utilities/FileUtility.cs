using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace FGUFW
{
    public static class FileUtility
    {
        public static Encoding GetEncoding(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var reader = new BinaryReader(stream, Encoding.Default, true))
            {
                var bytes = reader.ReadBytes((int)stream.Length);
                if (bytes.Length == 0 || HasPrefix(bytes, 0xEF, 0xBB, 0xBF) || IsUtf8(bytes))
                {
                    return Encoding.UTF8;
                }

                if (HasPrefix(bytes, 0xFE, 0xFF))
                {
                    return Encoding.BigEndianUnicode;
                }

                if (HasPrefix(bytes, 0xFF, 0xFE))
                {
                    return Encoding.Unicode;
                }

                return Encoding.Default;
            }
        }

        public static void LocalWrite(string localPath, byte[] data)
        {
            Write(Path.Combine(Application.persistentDataPath, localPath), data);
        }

        public static void LocalWrite(string localPath, string text)
        {
            Write(Path.Combine(Application.persistentDataPath, localPath), text);
        }

        public static byte[] LocalRead(string localPath)
        {
            var path = Path.Combine(Application.persistentDataPath, localPath);
            return File.Exists(path) ? File.ReadAllBytes(path) : null;
        }

        public static string LocalReadText(string localPath)
        {
            var path = Path.Combine(Application.persistentDataPath, localPath);
            return File.Exists(path) ? File.ReadAllText(path) : null;
        }

        [Obsolete("Use LocalRead instead.")]
        public static byte[] LocaRead(string localPath)
        {
            return LocalRead(localPath);
        }

        [Obsolete("Use LocalReadText instead.")]
        public static string LocaReadText(string localPath)
        {
            return LocalReadText(localPath);
        }

        public static void Write(string path, byte[] data)
        {
            EnsureParentDirectory(path);
            File.WriteAllBytes(path, data);
        }

        public static void Write(string path, string text)
        {
            EnsureParentDirectory(path);
            File.WriteAllText(path, text);
        }

        public static IEnumerator LoadStreaming(string localPath, Action<string, DownloadHandler> callback)
        {
            var path = Path.Combine(Application.streamingAssetsPath, localPath);
            using (var request = UnityWebRequest.Get(new Uri(path)))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                yield return request.SendWebRequest();
                callback(request.error, request.downloadHandler);
            }
        }

        public static void ClearDirectory(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                return;
            }

            foreach (var file in Directory.GetFiles(folderPath))
            {
                File.Delete(file);
            }

            foreach (var directory in Directory.GetDirectories(folderPath))
            {
                Directory.Delete(directory, true);
            }
        }

        public static void CopyFile(string from, string to)
        {
            EnsureParentDirectory(to);
            File.Copy(from, to, true);
        }

        public static void CopyDirectory(string from, string to)
        {
            var sourceDirectory = new DirectoryInfo(from);
            if (!sourceDirectory.Exists)
            {
                throw new DirectoryNotFoundException($"Source directory was not found: {from}");
            }

            Directory.CreateDirectory(to);
            foreach (var file in sourceDirectory.GetFiles())
            {
                file.CopyTo(Path.Combine(to, file.Name), true);
            }

            foreach (var directory in sourceDirectory.GetDirectories())
            {
                CopyDirectory(directory.FullName, Path.Combine(to, directory.Name));
            }
        }

        public sealed class DirectoryBrowser
        {
            private readonly List<string> directories = new List<string>();
            private readonly List<string> files = new List<string>();

            public IReadOnlyList<string> Directories => directories;
            public IReadOnlyList<string> Files => files;
            public string CurrentPath { get; private set; }

            public DirectoryBrowser(string initialPath = null)
            {
                if (string.IsNullOrWhiteSpace(initialPath) || !TryNavigateTo(initialPath))
                {
                    LoadRoots();
                }
            }

            public bool TryNavigateTo(string path)
            {
                if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                {
                    return false;
                }

                try
                {
                    var nextPath = Path.GetFullPath(path);
                    var nextDirectories = new List<string>(Directory.GetDirectories(nextPath));
                    var nextFiles = new List<string>(Directory.GetFiles(nextPath));
                    nextDirectories.Sort(StringComparer.OrdinalIgnoreCase);
                    nextFiles.Sort(StringComparer.OrdinalIgnoreCase);

                    CurrentPath = nextPath;
                    directories.Clear();
                    directories.AddRange(nextDirectories);
                    files.Clear();
                    files.AddRange(nextFiles);
                    return true;
                }
                catch (IOException)
                {
                    return false;
                }
                catch (UnauthorizedAccessException)
                {
                    return false;
                }
            }

            public bool NavigateUp()
            {
                var parentPath = string.IsNullOrEmpty(CurrentPath)
                    ? null
                    : Directory.GetParent(CurrentPath)?.FullName;
                return !string.IsNullOrEmpty(parentPath) && TryNavigateTo(parentPath);
            }

            public void LoadRoots()
            {
                CurrentPath = null;
                directories.Clear();
                files.Clear();
                directories.AddRange(Directory.GetLogicalDrives());
                directories.Sort(StringComparer.OrdinalIgnoreCase);
            }
        }

        private static void EnsureParentDirectory(string path)
        {
            var directoryPath = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        private static bool HasPrefix(byte[] data, params byte[] prefix)
        {
            if (data.Length < prefix.Length)
            {
                return false;
            }

            for (var index = 0; index < prefix.Length; index++)
            {
                if (data[index] != prefix[index])
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsUtf8(byte[] data)
        {
            var remainingBytes = 0;
            for (var index = 0; index < data.Length; index++)
            {
                var currentByte = data[index];
                if (remainingBytes == 0)
                {
                    if ((currentByte & 0x80) == 0)
                    {
                        continue;
                    }

                    if ((currentByte & 0xE0) == 0xC0) remainingBytes = 1;
                    else if ((currentByte & 0xF0) == 0xE0) remainingBytes = 2;
                    else if ((currentByte & 0xF8) == 0xF0) remainingBytes = 3;
                    else return false;
                }
                else
                {
                    if ((currentByte & 0xC0) != 0x80)
                    {
                        return false;
                    }

                    remainingBytes--;
                }
            }

            return remainingBytes == 0;
        }
    }
}
