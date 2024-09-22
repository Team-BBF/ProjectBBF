using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace DS.Editor
{
    public class DialogueNodeTypeAttribute : Attribute
    {
        public string TypeName { get; }

        public string MenuName { get; }

        public DialogueNodeTypeAttribute(string menuName, string typeName)
        {
            TypeName = typeName;
            MenuName = menuName;
        }
    }

    public class DialogueGraphView : GraphView
    {
        public readonly Vector2 DefaultNodeSize = new Vector2(150, 200);

        public string SaveDirectory;

        public DialogueGraphView()
        {
            LoadStyleSheet();
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            SetupBackground();
            AddManipulators();
            AddElement(DialogueNodeFactory.CreateEntryPointNode(this));
        }

        private void LoadStyleSheet()
        {
            var styleSheet =
                AssetDatabase.LoadAssetAtPath<StyleSheet>(DialogueEditorResourcesPath.DIALOGUE_GRAPH_STYLESHEET_PATH);
            styleSheets.Add(styleSheet);
        }

        private void SetupBackground()
        {
            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();
        }

        private void AddManipulators()
        {
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            this.AddManipulator(CreateNodeContextualMenu());
        }

        private IManipulator CreateNodeContextualMenu()
        {
            return new ContextualMenuManipulator(menuEvent =>
            {
                Matrix4x4 transformationMatrix = contentViewContainer.worldTransform;
                Vector2 mousePosition = menuEvent.mousePosition;
                Vector2 localMousePosition = transformationMatrix.inverse.MultiplyPoint3x4(mousePosition);

                foreach (var metadata in DialogueNodeFactory.NodeMetadatas)
                {
                    var att = metadata.Attribute;
                    var type = metadata.Type;

                    menuEvent.menu.AppendAction(
                        att.MenuName,
                        _ => DialogueNodeFactory.CreateEmptyNode(att.TypeName, this, localMousePosition)
                    );
                }
            });
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.Where(port => startPort != port && startPort.node != port.node).ToList();
        }
    }
}