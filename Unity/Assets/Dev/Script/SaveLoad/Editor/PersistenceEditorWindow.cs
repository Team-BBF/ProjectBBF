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
        private Vector2 _elementScrollPosition = Vector2.zero;
        private Vector2 _fieldScrollPosition = Vector2.zero;
        private int _beforeSaveDataDropdownSelectedIndex = -1;
        private IEnumerable<KeyValuePair<string, object>> _diskPairs;
        private string _selectedKey = string.Empty;
        private Dictionary<string, bool> _dirtyElementTable = new();
        private Dictionary<string, bool> _dirtyFieldTable = new();

        private int _tabIndex;

        [MenuItem("Window/ProjectBBF/Save data Editor")]
        public static void ShowWindow()
        {
            GetWindow<PersistenceEditorWindow>("Save data Editor");
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("세이브 데이터를 보거나, 편집할 수 있는 에디어 창입니다.", EditorStyles.boldLabel);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("저장된 데이터", EditorStyles.miniButtonLeft))
                {
                    _tabIndex = 0;
                }

                if (GUILayout.Button("런타임 데이터", EditorStyles.miniButtonLeft))
                {
                    _tabIndex = 1;
                }
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            {
                switch (_tabIndex)
                {
                    case 0:
                        OnDisk();
                        break;  
                    case 1:
                        OnRuntime();
                        break;
                }
            }
            GUILayout.EndHorizontal();
        }

        private void OnRuntime()
        {
            Rect windowRect = position;

            if (Application.isPlaying is false)
            {
                GUILayout.Label("현재 모드: 런타임 데이터");
                GUILayout.Label("플레이 모드가 아닌 상태에서는 사용할 수 없습니다.");
                return;
            }
            
            var pairs = PersistenceManager.Instance.GetAllData();

            if (pairs.Count == 0)
            {
                GUILayout.Label("비었음");
                return;
            }

            GUILayout.BeginVertical();
            _elementScrollPosition = GUILayout.BeginScrollView(_elementScrollPosition, GUILayout.Width(400),
                GUILayout.Height(windowRect.height));
            {
                
                foreach (var pair in pairs)
                {
                    bool result = GUILayout.Button(pair.Key, new GUIStyle(GUI.skin.button)
                    {
                        normal = { background = null, textColor = pair.Key == _selectedKey ? Color.yellow : Color.black },
                        hover = { textColor = Color.cyan },
                        active = { textColor = Color.red },
                        alignment = TextAnchor.MiddleLeft,
                        fontStyle = FontStyle.Normal,
                        padding = new RectOffset(5, 5, 5, 5)
                    });

                    if (result)
                    {
                        _selectedKey = pair.Key;
                        GUI.FocusControl(""); // 포커스를 잃게 함
                    }
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            
            GUILayout.BeginVertical();
            _fieldScrollPosition = GUILayout.BeginScrollView(_fieldScrollPosition, GUILayout.Width(400),
                GUILayout.Height(windowRect.height));
            {
                if (string.IsNullOrEmpty(_selectedKey) is false && pairs is not null)
                {
                    foreach (var pair in pairs)
                    {
                        if (pair.Key != _selectedKey) continue;

                        object target = pair.Value;
                        var type = target.GetType();
                        var infos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic |
                                                   BindingFlags.Instance);

                        foreach (FieldInfo info in infos)
                        {
                            if (info.IsPublic is false && info.GetCustomAttribute<SerializeField>() is null)
                            {
                                continue;
                            }

                            DrawField(target, info, pair.Key);
                        }
                    }
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void OnDisk()
        {
            Rect windowRect = position;


            if (Application.isPlaying)
            {
                GUILayout.Label("현재 모드: 저장된 데이터");
                GUILayout.Label("플레이 모드에서는 사용할 수 없습니다.");
                return;
            }

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

                if (_diskPairs is null || _saveDataDropdownSelectedIndex == -1)
                {
                    GUI.enabled = false;
                }

                if (GUILayout.Button("저장"))
                {
                    var buf = PersistenceManager.Descriptor.ToBytes(_diskPairs);
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
                    _diskPairs = PersistenceManager.Descriptor.FromBytes(buf);
                    _dirtyElementTable.Clear();
                    _dirtyFieldTable.Clear();
                }

                GUI.enabled = true;


                if ((GUI.changed || _diskPairs is null) && _saveDataDropdownSelectedIndex != -1)
                {
                    string curFileName = fileNames[_saveDataDropdownSelectedIndex];
                    curFileName = curFileName.Split('.')[0];

                    byte[] bytes = PersistenceManager.LoadFile(curFileName);
                    _diskPairs = PersistenceManager.Descriptor.FromBytes(bytes);
                }

                if (_diskPairs is not null)
                {
                    _elementScrollPosition = GUILayout.BeginScrollView(_elementScrollPosition, GUILayout.Width(400),
                        GUILayout.Height(windowRect.height));
                    foreach (var pair in _diskPairs)
                    {
                        string dirtyTag = "";

                        if (_dirtyElementTable.TryGetValue(pair.Key, out var dirty) && dirty)
                        {
                            dirtyTag = " *";
                        }

                        bool result = GUILayout.Button(pair.Key + dirtyTag, new GUIStyle(GUI.skin.button)
                        {
                            normal = { background = null, textColor = pair.Key == _selectedKey ? Color.yellow : Color.black },
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


            _fieldScrollPosition = GUILayout.BeginScrollView(_fieldScrollPosition, GUILayout.Width(400),
                GUILayout.Height(windowRect.height));
            {
                if (string.IsNullOrEmpty(_selectedKey) is false && _diskPairs is not null)
                {
                    foreach (var pair in _diskPairs)
                    {
                        if (pair.Key != _selectedKey) continue;

                        object target = pair.Value;
                        var type = target.GetType();
                        var infos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic |
                                                   BindingFlags.Instance);

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