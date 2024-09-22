using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;

namespace ProjectBBF.Persistence
{
    public class JsonDescriptor
    {
        private const bool PRETTY_PRINT = 
        #if UNITY_EDITOR
            true;
        #else
            false;
        #endif
        
        [Serializable]
        private class Wrapper
        {
            public string Key;
            public string Type;
            public string Json;
        }
        
        [Serializable]
        private class JsonWrapper
        {
            public List<Wrapper> Wrappers = new List<Wrapper>(10);
        }
        
        public byte[] ToBytes(IEnumerable<KeyValuePair<string, object>> objList)
        {
            JsonWrapper jsonWrapper = new JsonWrapper();
            
            foreach (var pair in objList)
            {
                string json = JsonUtility.ToJson(pair.Value, PRETTY_PRINT);

                var wrapper = new Wrapper
                {
                    Key = pair.Key,
                    Type = pair.Value.GetType().FullName,
                    Json = json,
                };
                
                json = JsonUtility.ToJson(wrapper, PRETTY_PRINT);

                if (string.IsNullOrEmpty(json))
                {
                    Debug.LogError($"key({pair.Key}) 데이터를 저장하는데 실패");
                    continue;
                }
                
                jsonWrapper.Wrappers.Add(wrapper);
            }
            
            string totalJson = JsonUtility.ToJson(jsonWrapper, PRETTY_PRINT);
            
            return Encoding.BigEndianUnicode.GetBytes(totalJson);
        }


        public IEnumerable<KeyValuePair<string, object>> FromBytes(byte[] bytes)
        {
            string json = Encoding.BigEndianUnicode.GetString(bytes);
            JsonWrapper jsonWrapper = JsonUtility.FromJson(json, typeof(JsonWrapper)) as JsonWrapper;

            if (jsonWrapper is null)
            {
                Debug.LogError("데이터를 불러오는데 실패");
                return new List<KeyValuePair<string, object>>();
            }

            List<KeyValuePair<string, object>> objList = new List<KeyValuePair<string, object>>();
            
            foreach (Wrapper wrapper in jsonWrapper.Wrappers)
            {
                if (string.IsNullOrEmpty(wrapper.Json))
                {
                    Debug.LogError($"key({wrapper.Key}) 데이터를 불러오는데 실패");
                    continue;
                }
                
                var obj = JsonUtility.FromJson(wrapper.Json, Type.GetType(wrapper.Type));
                if (obj is not object dataMessage)
                {
                    Debug.LogError($"key({wrapper.Key}) 데이터를 불러오는데 실패");
                    continue;
                }
                
                objList.Add(new KeyValuePair<string, object>(wrapper.Key, dataMessage));
            }

            return objList;
        }

    }
}