using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ProjectBBF.Persistence.Editor
{
    public class PersistenceEditorWindow : EditorWindow
    {
        private int _saveDataDropdownSelectedIndex = -1;
        private Vector2  _elementScrollPosition = Vector2.zero;
        private Vector2  _fieldScrollPosition = Vector2.zero;
        private int _beforeSaveDataDropdownSelectedIndex = -1;
        private IEnumerable<KeyValuePair<string, object>> _pairs;
        private string _selectedKey = string.Empty;
        private Dictionary<string, bool> _dirtyElementTable = new ();
        private Dictionary<string, bool> _dirtyFieldTable = new ();

        [MenuItem("Window/ProjectBBF/Save data Editor")]
        public static void ShowWindow()
        {
            GetWindow<PersistenceEditorWindow>("Save data Editor");
        }

        private void OnGUI()
        {
            Rect windowRect = position;
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("세이브 데이터를 보거나, 편집할 수 있는 에디어 창입니다.", EditorStyles.boldLabel);
                GUILayout.Space(20);
            }
            GUILayout.EndHorizontal();
            
            
            GUILayout.BeginHorizontal();
            {
                
                GUILayout.BeginVertical();
                {
                    var fileNames = PersistenceManager.GetAllSaveDataName();

                    _saveDataDropdownSelectedIndex =
                        EditorGUILayout.Popup("SaveData", _saveDataDropdownSelectedIndex, fileNames);

                    if (_beforeSaveDataDropdownSelectedIndex != _saveDataDropdownSelectedIndex)
                    {
                        _beforeSaveDataDropdownSelectedIndex = _saveDataDropdownSelectedIndex;
                        
                        _dirtyFieldTable.Clear();
                        _dirtyElementTable.Clear();
                    }

                    if (_pairs is null || _saveDataDropdownSelectedIndex == -1)
                    {
                        GUI.enabled = false;
                    }
                    if (GUILayout.Button("저장"))
                    {
                        var buf = PersistenceManager.Descriptor.ToBytes(_pairs);
                        string curFileName = fileNames[_saveDataDropdownSelectedIndex];
                        curFileName = curFileName.Split('.')[0];
                        PersistenceManager.SaveFile(curFileName, buf);
                        _dirtyElementTable.Clear();
                        _dirtyFieldTable.Clear();
                    }
                    GUI.enabled = true;
                    
                    
                    if (_saveDataDropdownSelectedIndex == -1)
                    {
                        GUI.enabled = false;
                    }
                    if (GUILayout.Button("다시 불러오기"))
                    {
                        string curFileName = fileNames[_saveDataDropdownSelectedIndex];
                        curFileName = curFileName.Split('.')[0];
                        var buf = PersistenceManager.LoadFile(curFileName);
                        _pairs = PersistenceManager.Descriptor.FromBytes(buf);
                        _dirtyElementTable.Clear();
                        _dirtyFieldTable.Clear();
                    }
                    GUI.enabled = true;
                

                    if ((GUI.changed || _pairs is null) && _saveDataDropdownSelectedIndex != -1)
                    {
                        string curFileName = fileNames[_saveDataDropdownSelectedIndex];
                        curFileName = curFileName.Split('.')[0];
            
                        byte[] bytes = PersistenceManager.LoadFile(curFileName);
                        _pairs = PersistenceManager.Descriptor.FromBytes(bytes);
                    }

                    if (_pairs is not null)
                    {
                        _elementScrollPosition = GUILayout.BeginScrollView(_elementScrollPosition, GUILayout.Width(400), GUILayout.Height(windowRect.height));
                        foreach (var pair in _pairs)
                        {
                            string dirtyTag = "";

                            if (_dirtyElementTable.TryGetValue(pair.Key, out var dirty) && dirty)
                            {
                                dirtyTag = " *";
                            }
                            bool result = GUILayout.Button(pair.Key + dirtyTag, new GUIStyle(GUI.skin.button)
                            {
                                normal = { background = null, textColor = Color.black },
                                hover = { textColor = Color.cyan },
                                active = { textColor = Color.red },
                                alignment = TextAnchor.MiddleLeft,
                                fontStyle = FontStyle.Normal,
                                padding = new RectOffset(5, 5, 5, 5) // 패딩 제거
                            });

                            if (result)
                            {
                                _selectedKey = pair.Key;
                                GUI.FocusControl(""); // 포커스를 잃게 함
                            }
                        }
                        GUILayout.EndScrollView();
                    }
                    
                }
                GUILayout.EndVertical();
                
                
                _fieldScrollPosition = GUILayout.BeginScrollView(_fieldScrollPosition, GUILayout.Width(400), GUILayout.Height(windowRect.height));
                {
                    if (string.IsNullOrEmpty(_selectedKey) is false && _pairs is not null)
                    {
                        foreach (var pair in _pairs)
                        {
                            if(pair.Key != _selectedKey)continue;
                        
                            object target = pair.Value;
                            var type = target.GetType();
                            var infos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );

                            foreach (FieldInfo info in infos)
                            {
                                if (info.IsPublic is false && info.GetCustomAttribute<SerializeField>() is null)
                                {
                                    continue;
                                }

                                object tempValue = info.GetValue(target);
                                if (DrawField(target, info, pair.Key))
                                {
                                    if (tempValue.Equals(info.GetValue(target)) is false)
                                    {
                                        _dirtyElementTable[pair.Key] = true;
                                        _dirtyFieldTable[pair.Key + "_" + info.Name] = true;
                                    }
                                }
                            }
                        }
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndHorizontal();

        }

        

        private bool DrawField(object obj, FieldInfo info, string key)
        {
            string dirtyTag = "";

            if (_dirtyFieldTable.TryGetValue(key + "_" + info.Name, out var dirty) && dirty)
            {
                dirtyTag = " *";
            }
            
            if (info.FieldType == typeof(int))
            {
                int value = (int)info.GetValue(obj);
                value = EditorGUILayout.IntField($"{info.Name}({info.FieldType.Name}){dirtyTag}", value);
                info.SetValue(obj, value);
                return true;
            }
            else if (info.FieldType == typeof(uint))
            {
                long value = (uint)info.GetValue(obj);
                value = EditorGUILayout.LongField($"{info.Name}({info.FieldType.Name}){dirtyTag}", value);
                info.SetValue(obj, value);
                return true;
            }
            else if (info.FieldType == typeof(byte))
            {
                long value = (byte)info.GetValue(obj);
                
                GUI.enabled = false;
                value = EditorGUILayout.LongField($"{info.Name}({info.FieldType.Name}){dirtyTag}", value);
                GUI.enabled = true;
                return true;
            }
            else if (info.FieldType == typeof(sbyte))
            {
                long value = (sbyte)info.GetValue(obj);
                
                GUI.enabled = false;
                value = EditorGUILayout.LongField($"{info.Name}({info.FieldType.Name}){dirtyTag}", value);
                GUI.enabled = true;
                return true;
            }
            else if (info.FieldType == typeof(long))
            {
                long value = (long)info.GetValue(obj);
                value = EditorGUILayout.LongField($"{info.Name}({info.FieldType.Name}){dirtyTag}", value);
                info.SetValue(obj, value);
                return true;
            }
            
            else if (info.FieldType == typeof(double))
            {
                double value = (double)info.GetValue(obj);
                value = EditorGUILayout.DoubleField($"{info.Name}({info.FieldType.Name}){dirtyTag}", value);
                info.SetValue(obj, value);
                return true;
            }
            else if (info.FieldType == typeof(float))
            {
                float value = (float)info.GetValue(obj);
                value = EditorGUILayout.FloatField($"{info.Name}({info.FieldType.Name}){dirtyTag}", value);
                info.SetValue(obj, value);
                return true;
            }
            
            else if (info.FieldType == typeof(bool))
            {
                bool value = (bool)info.GetValue(obj);
                value = EditorGUILayout.Toggle($"{info.Name}({info.FieldType.Name}){dirtyTag}", value);
                info.SetValue(obj, value);
                return true;
            }
            else if (info.FieldType == typeof(string))
            {
                string value = (string)info.GetValue(obj);
                value = EditorGUILayout.TextField($"{info.Name}({info.FieldType.Name}){dirtyTag}", value);
                info.SetValue(obj, value);
                return true;
            }
            
            
            else if (info.FieldType == typeof(Vector2))
            {
                Vector2 value = (Vector2)info.GetValue(obj);
                value = EditorGUILayout.Vector2Field($"{info.Name}({info.FieldType.Name}){dirtyTag}", value);
                info.SetValue(obj, value);
                return true;
            }
            else if (info.FieldType == typeof(Vector3))
            {
                Vector3 value = (Vector3)info.GetValue(obj);
                value = EditorGUILayout.Vector3Field($"{info.Name}({info.FieldType.Name}){dirtyTag}", value);
                info.SetValue(obj, value);
                return true;
            }
            else if (info.FieldType == typeof(Vector4))
            {
                Vector4 value = (Vector4)info.GetValue(obj);
                value = EditorGUILayout.Vector4Field($"{info.Name}({info.FieldType.Name}){dirtyTag}", value);
                info.SetValue(obj, value);
                return true;
            }
            
            
            else if (info.FieldType == typeof(Vector2Int))
            {
                Vector2Int value = (Vector2Int)info.GetValue(obj);
                value = EditorGUILayout.Vector2IntField($"{info.Name}({info.FieldType.Name}){dirtyTag}", value);
                info.SetValue(obj, value);
                return true;
            }
            else if (info.FieldType == typeof(Vector3Int))
            {
                Vector3Int value = (Vector3Int)info.GetValue(obj);
                value = EditorGUILayout.Vector3IntField($"{info.Name}({info.FieldType.Name}){dirtyTag}", value);
                info.SetValue(obj, value);
                return true;
            }
            
            return false;
        }
    }
}