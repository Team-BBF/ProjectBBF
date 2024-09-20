using System;
using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ProjectBBF.Singleton;
using UnityEngine;

namespace ProjectBBF.Persistence
{
    [Singleton(ESingletonType.Global)]
    public class PersistenceManager : MonoBehaviourSingleton<PersistenceManager>
    {
        public static readonly JsonDescriptor Descriptor = new JsonDescriptor();

        private Dictionary<string, object> _objTable;

        public override void PostInitialize()
        {
            _objTable = new Dictionary<string, object>();
        }

        public override void PostRelease()
        {
            _objTable.Clear();
            _objTable = null;
        }

        public string SaveFileName { get; set; } = "Default";
        public static readonly string UserDataFileName = "User";
        public static readonly string FileExtension = "save";
        
        public void SaveGameData(string saveFileName)
        {
            var pairs = _objTable
                .Where(x => IsDecorated<GameDataAttribute>(x.Value))
                .Select(x => new KeyValuePair<string, object>(x.Key, x.Value));
            
            var buffer = Descriptor.ToBytes(pairs);
            
            SaveFile(saveFileName, buffer);
        }
        

        public void Load(string saveFileName)
        {
            var buffer = LoadFile(saveFileName);

            if (buffer is not null)
            {
                var messages = Descriptor.FromBytes(buffer);
                _objTable = new Dictionary<string, object>(messages);
                
                return;
            }
            
            _objTable = new Dictionary<string, object>();
        }

        public void SaveGameDataCurrentFileName() => SaveGameData(SaveFileName);
        public void LoadGameDataCurrentFileName() => Load(SaveFileName);
        public void SaveUserData()
        {
            var pairs = _objTable
                .Where(x => IsDecorated<UserDataAttribute>(x.Value))
                .Select(x => new KeyValuePair<string, object>(x.Key, x.Value));
            
            var buffer = Descriptor.ToBytes(pairs);
            
            SaveFile(UserDataFileName, buffer);
        }

        public static string[] GetAllSaveDataName(bool fullPath = false)
        {
            var path = Application.persistentDataPath;

            string[] files = Directory.GetFiles(path, $"*.{FileExtension}");

            if (fullPath is false)
            {
                files = files
                    .Select(x => x.Split('/', '\\').Last())
                    .ToArray();
            }
            
            return files;
        }
        
        public static void SaveFile(string fileName, byte[] data)
        {
            var path = CombinePathAndFile(fileName);
            using var stream = new BufferedStream(File.Open(path, FileMode.Create));
            
            stream.Write(data, 0, data.Length);
        }

        public static byte[] LoadFile(string fileName)
        {
            var path = CombinePathAndFile(fileName);
            
            List<byte> data = new List<byte>(1024);

            try
            {
                using var stream = new BufferedStream(File.Open(path, FileMode.Open));

                while (true)
                {
                    int v = stream.ReadByte();
                    if (v is -1) break;

                    data.Add((byte)v);
                }
            }
            catch (Exception e) when (e is not FileNotFoundException)
            {
                Debug.LogException(e);
                return null;
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            
            return data.ToArray();
        }

        private static string CombinePathAndFile(string fileName) => Application.persistentDataPath + $"/{fileName}.{FileExtension}";

        private static bool IsDecorated<T>(object obj) where T : DataTagAttribute
        {
            return obj.GetType().GetCustomAttribute(typeof(T), true) is not null;
        }
        
        public T LoadOrCreate<T>(string key) where T : new()
        {
            object cachedObj = GetCachedPersistenceObj(ref key);
            if (cachedObj is null)
            {
                cachedObj = new T();
                Debug.Log($"PersistenceManager object 생성(key: {typeof(T)}, type: {key})");
                
                _objTable[key] = cachedObj;
            }

            if (cachedObj is T t)
            {
                return t;
            }

            throw new Exception(
                $"Persistence 에러! Key({key}), cachedObject Type({cachedObj.GetType()}), Acquire Type({typeof(T)})");
        }

        public List<KeyValuePair<string, object>> GetAllData()
        {
            return _objTable.ToList();
        }

        public object GetCachedPersistenceObj(ref string key)
        {
            return _objTable.GetValueOrDefault(key);
        }
    }
}