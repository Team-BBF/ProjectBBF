using System;
using System.IO;
using System.Linq;
using DS.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DS.Editor
{
    public class DialogueGraph : EditorWindow
    {
        private DialogueGraphView _graphView;
        private Label _titleLabel;

        [MenuItem("Dialogue Graph/Editor")]
        public static void OpenDialogueGraphView()
        {
            var window = GetWindow<DialogueGraph>();
            window.titleContent = new GUIContent("Dialogue Graph");
        }
        

        [MenuItem("Dialogue Graph/Debug Print On", false)]
        private static void OnAutoPlayModeOn()
        {
            EditorPrefs.SetBool("__DEBUG_PRINT_MODE__", true);
        }

        [MenuItem("Dialogue Graph/Debug Print Off", false)]
        private static void OnAutoPlayModeOff()
        {
            EditorPrefs.SetBool("__DEBUG_PRINT_MODE__", false);
        }

        [MenuItem("Dialogue Graph/Debug Print On", true)]
        private static bool ValidateOnAutoPlayModeOn()
        {
            return EditorPrefs.GetBool("__DEBUG_PRINT_MODE__") is false;
        }

        [MenuItem("Dialogue Graph/Debug Print Off", true)]
        private static bool ValidateOnAutoPlayModeOff()
        {
            return EditorPrefs.GetBool("__DEBUG_PRINT_MODE__");
        }

        private void OnEnable()
        {
            InitializeGraphView();
            GenerateToolbar();
            GenerateMiniMap();
        }

        private void OnDisable()
        {
            RemoveGraphView();
        }

        private void InitializeGraphView()
        {
            _graphView = new DialogueGraphView
            {
                name = "Dialogue Graph"
            };

            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }

        private void RemoveGraphView()
        {
            rootVisualElement.Remove(_graphView);
        }

        private void GenerateMiniMap()
        {
            var miniMap = new MiniMap { anchored = true };
            miniMap.SetPosition(new Rect(5, 35, 200, 140));
            _graphView.Add(miniMap);
        }

        private void GenerateToolbar()
        {
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(DialogueEditorResourcesPath.DIALOGUE_TOOLBAR_STYLESHEET_PATH);
            var toolbar = new Toolbar();
            toolbar.Add(CreateNewDialogueButton());
            toolbar.Add(CreateSaveButton());
            toolbar.Add(CreateLoadButton());
            toolbar.Add(CreateTitle());
            toolbar.styleSheets.Add(styleSheet);
            rootVisualElement.Add(toolbar);
        }

        private VisualElement CreateTitle()
        {
            _titleLabel = new Label("title: ");
            return _titleLabel;
        }

        private void OnChangeDialogueContainer(string fullPath)
        {
            string path = GraphSaveUtility.ConvertFullToRelativePath(fullPath);
            _titleLabel.text = $"title: {path}";
        }

        private Button CreateNewDialogueButton()
        {
            return new Button(CreateNewDialogue)
            {
                text = "Create New Dialogue"
            };
        }

        private Button CreateSaveButton()
        {
            return new Button(() => RequestDataOperation(true))
            {
                text = "Save"
            };
        }

        private Button CreateLoadButton()
        {
            return new Button(() => RequestDataOperation(false))
            {
                text = "Load"
            };
        }
        
        private void CreateNewDialogue()
        {
            var saveUtility = GraphSaveUtility.GetInstance(_graphView);
            
            // 수정사항이 있다면 저장하겠습니까 팝업을 띄움
            if (_graphView.edges.ToList().Any())
            {
                if (_graphView.SaveDirectory == null // 한 번도 저장하지 않았거나
                    || saveUtility.CheckAnyChange(_graphView.SaveDirectory)) // 변경사항이 있다면
                {
                    bool doSave = EditorUtility.DisplayDialog("Save Confirmation", "Would you like to save?", "Yes", "No");
                    if (doSave)
                    {
                        RequestDataOperation(true);
                    }
                }
            }
            
            saveUtility.ClearGraph();
            _graphView.SaveDirectory = String.Empty;
            OnChangeDialogueContainer(_graphView.SaveDirectory);
        }

        private void RequestDataOperation(bool save)
        {
            var saveUtility = GraphSaveUtility.GetInstance(_graphView);

            if (save)
            {
                if (string.IsNullOrEmpty(_graphView.SaveDirectory))
                {
                    string fullPath = EditorUtility.SaveFilePanel("Dialogue Graph", $"{Application.dataPath}", "", "asset");
                    _graphView.SaveDirectory = fullPath;
                }
                
                if (string.IsNullOrEmpty(_graphView.SaveDirectory))
                    return;
                
                saveUtility.SaveGraph(_graphView.SaveDirectory);
                OnChangeDialogueContainer(_graphView.SaveDirectory);
            }
            else
            {
                // 수정사항이 있다면 저장하겠습니까 팝업을 띄움
                if (_graphView.edges.ToList().Any())
                {
                    if (_graphView.SaveDirectory == null // 한 번도 저장하지 않았거나
                        || saveUtility.CheckAnyChange(_graphView.SaveDirectory)) // 변경사항이 있다면
                    {
                        bool doSave = EditorUtility.DisplayDialog("Save Confirmation", "Would you like to save?", "Yes", "No");
                        if (doSave)
                        {
                            RequestDataOperation(true);
                        }
                    }
                }
                    
                string fullPath = EditorUtility.OpenFilePanel("Dialogue Graph", $"{Application.dataPath}/1. Data/Dialogue/", "asset");
                
                if (string.IsNullOrEmpty(fullPath))
                    return;
                
                _graphView.SaveDirectory = fullPath;
                
                saveUtility.LoadGraph(_graphView.SaveDirectory);
                OnChangeDialogueContainer(_graphView.SaveDirectory);
            }
        }
    }
}
