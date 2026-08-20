using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FGUFW;
using UnityEngine;

namespace FGUFW.LocalSave
{
    /// <summary>
    /// 轻量本地存档服务，将多个不同类型的对象保存到同一个文件。
    /// </summary>
    public sealed class LocalSaveService : ISaveService
    {
        private const string DefaultDirectoryName = "FGUFW";
        private const string DefaultFileName = "LocalSave.json";

        private readonly string filePath;
        private readonly SemaphoreSlim ioLock = new SemaphoreSlim(1, 1);

        // 已恢复为具体类型的存档对象，同一种类型只保留一个实例。
        private readonly Dictionary<string, object> loadedObjects = new Dictionary<string, object>();

        // 尚未访问的存档数据保留为Json文本，在Get<T>时延迟反序列化。
        private Dictionary<string, string> serializedEntries = new Dictionary<string, string>();

        private bool loaded;

        /// <summary>
        /// 使用默认存档路径创建服务。
        /// </summary>
        public LocalSaveService() : this(Path.Combine(Application.persistentDataPath, DefaultDirectoryName, DefaultFileName))
        {
        }

        /// <summary>
        /// 使用指定存档文件路径创建服务。
        /// </summary>
        public LocalSaveService(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("Save file path cannot be empty.", nameof(filePath));
            }

            this.filePath = filePath;
        }

        /// <summary>
        /// 加载存档文件。重复调用不会重复读取。
        /// </summary>
        public async Task LoadAsync()
        {
            if (loaded)
            {
                return;
            }

            await LoadInternalAsync();
        }

        /// <summary>
        /// 获取指定类型的存档对象。不存在时创建并加入缓存。
        /// </summary>
        public T Get<T>() where T : class, new()
        {
            EnsureLoaded();

            var key = GetSaveKey<T>();
            if (loadedObjects.TryGetValue(key, out var loadedObject))
            {
                if (loadedObject is T typedObject)
                {
                    return typedObject;
                }

                throw new InvalidDataException($"Save entry '{key}' contains {loadedObject.GetType().FullName}, expected {typeof(T).FullName}.");
            }

            T value;
            if (serializedEntries.TryGetValue(key, out var json))
            {
                value = fg.toObject<T>(json) ?? new T();
            }
            else
            {
                value = new T();
            }

            loadedObjects.Add(key, value);
            return value;
        }

        /// <summary>
        /// 保存当前全部存档对象，未加载的旧条目会原样保留。
        /// </summary>
        public async Task SaveAsync()
        {
            EnsureLoaded();

            await ioLock.WaitAsync();
            try
            {
                var json = CreateSaveJson();
                var temporaryPath = filePath + ".tmp";

                // 先写临时文件，写入成功后再覆盖正式存档。
                EnsureSaveDirectory();
                await File.WriteAllTextAsync(temporaryPath, json);
                CommitTemporaryFile(temporaryPath);
            }
            finally
            {
                ioLock.Release();
            }
        }

        /// <summary>
        /// 在应用退出、进入后台或发生未处理异常时同步保存。
        /// 如果存档尚未加载或已有IO操作正在执行，则不重复写入。
        /// </summary>
        internal void SaveImmediately()
        {
            if (!loaded || !ioLock.Wait(0))
            {
                return;
            }

            try
            {
                var temporaryPath = filePath + ".tmp";
                EnsureSaveDirectory();
                File.WriteAllText(temporaryPath, CreateSaveJson());
                CommitTemporaryFile(temporaryPath);
            }
            finally
            {
                ioLock.Release();
            }
        }

        /// <summary>
        /// 执行实际读取，并通过锁避免重复加载。
        /// </summary>
        private async Task LoadInternalAsync()
        {
            await ioLock.WaitAsync();
            try
            {
                if (loaded)
                {
                    return;
                }

                loadedObjects.Clear();
                serializedEntries.Clear();

                if (File.Exists(filePath))
                {
                    var json = await File.ReadAllTextAsync(filePath);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        serializedEntries = fg.toObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
                    }
                }

                loaded = true;
            }
            finally
            {
                ioLock.Release();
            }
        }

        /// <summary>
        /// 确保存档已经完成加载。
        /// </summary>
        private void EnsureLoaded()
        {
            if (!loaded)
            {
                throw new InvalidOperationException("Local save data is not loaded. Call LoadAsync() first.");
            }
        }

        /// <summary>
        /// 将已恢复的对象更新到序列化字典，并生成完整存档文本。
        /// </summary>
        private string CreateSaveJson()
        {
            foreach (var entry in loadedObjects)
            {
                serializedEntries[entry.Key] = fg.toJson(entry.Value);
            }

            return fg.toJson(serializedEntries);
        }

        /// <summary>
        /// 确保存档目录存在。
        /// </summary>
        private void EnsureSaveDirectory()
        {
            var directoryPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        /// <summary>
        /// 使用已经完整写入的临时文件覆盖正式存档。
        /// </summary>
        private void CommitTemporaryFile(string temporaryPath)
        {
            File.Copy(temporaryPath, filePath, true);
            File.Delete(temporaryPath);
        }

        /// <summary>
        /// 使用类型全名作为每种存档对象的唯一Key。
        /// </summary>
        private static string GetSaveKey<T>()
        {
            return typeof(T).FullName ?? typeof(T).Name;
        }
    }
}
