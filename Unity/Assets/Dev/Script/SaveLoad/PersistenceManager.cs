using System;
using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ProjectBBF.Singleton;
using UnityEditor;
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

        public Metadata CurrentMetadata { get; set; } = new()
        {
            SaveFileName = "Default",
            PlayerName = "Default Player",
            Day = 1,
        };
        public static readonly string UserDataFileName = "userData";
        public static readonly string UserDataExtension = "user";
        public static readonly string GameDataExtension = "save";
        public static readonly string MetadataExtension = "meta";
        
        public void SaveGameData(Metadata metadata)
        {
            var pairs = _objTable
                .Where(x => IsDecorated<GameDataAttribute>(x.Value))
                .Select(x => new KeyValuePair<string, object>(x.Key, x.Value));
            
            var buffer = Descriptor.ToBytes(pairs);
            
            SaveFile(metadata.SaveFileName, GameDataExtension, buffer);
            SaveMetadata(metadata);
        }

        public static void SaveMetadata(Metadata metadata)
        {
            var json = JsonUtility.ToJson(metadata, false);
            var buf = Encoding.BigEndianUnicode.GetBytes(json);
            
            SaveFile(metadata.SaveFileName, MetadataExtension, buf);
        }

        public static Metadata LoadMetadata(string fileName)
        {
            var buf = LoadFile(fileName, MetadataExtension);

            if (buf is null)
            {
                return null;
            }
            
            var json = Encoding.BigEndianUnicode.GetString(buf);
            
            var metadata = JsonUtility.FromJson<Metadata>(json);

            return metadata;
        }
        

        public void Load(string saveFileName)
        {
            var buffer = LoadFile(saveFileName, GameDataExtension);

            if (buffer is not null)
            {
                var messages = Descriptor.FromBytes(buffer);
                _objTable = new Dictionary<string, object>(messages);
                
                return;
            }
            
            _objTable = new Dictionary<string, object>();
        }

        public void SaveGameDataCurrentFileName() => SaveGameData(CurrentMetadata);
        public void LoadGameDataCurrentFileName() => Load(CurrentMetadata.SaveFileName);
        public void SaveUserData()
        {
            var pairs = _objTable
                .Where(x => IsDecorated<UserDataAttribute>(x.Value))
                .Select(x => new KeyValuePair<string, object>(x.Key, x.Value));
            
            var buffer = Descriptor.ToBytes(pairs);
            
            SaveFile(UserDataFileName, UserDataExtension, buffer);
        }

        public static Metadata[] GetAllSaveFileMetadata(bool fullPath = false)
        {
            var path = Application.persistentDataPath;

            string[] files = Directory.GetFiles(path, $"*.{MetadataExtension}");

            if (fullPath is false)
            {
                files = files
                    .Select(Path.GetFileNameWithoutExtension)
                    .ToArray();
            }
            
            List<Metadata> metadatas = new List<Metadata>(files.Length);
            for (int i = 0; i < files.Length; i++)
            {
                var metadata = LoadMetadata(files[i]);

                if (metadata is null) continue;
                metadatas.Add(metadata);
            }
            
            return metadatas.ToArray();
        }
        
        public static void SaveFile(string fileName, string extension, byte[] data)
        {
            var path = CombinePathAndFile(fileName, extension);
            using var stream = new BufferedStream(File.Open(path, FileMode.Create));
            
            stream.Write(data, 0, data.Length);
        }

        public static byte[] LoadFile(string fileName, string extension)
        {
            var path = CombinePathAndFile(fileName, extension);
            
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

        private static string CombinePathAndFile(string fileName, string extension) => Application.persistentDataPath + $"/{fileName}.{extension}";

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