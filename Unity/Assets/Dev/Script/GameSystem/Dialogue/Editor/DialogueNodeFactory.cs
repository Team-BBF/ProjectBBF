using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DS.Editor;
using DS.Runtime;
using UnityEditor;
using UnityEngine;
using DS.Core;

namespace DS.Editor
{
    public class DialogueMetadata
    {
        public DialogueNodeTypeAttribute Attribute { get; private set; }
        public Type Type { get; private set; }

        public DialogueMetadata(DialogueNodeTypeAttribute attribute, Type type)
        {
            Attribute = attribute;
            Type = type;
        }
    }

    [InitializeOnLoad]
    public static class DialogueNodeFactory
    {
        private static readonly List<DialogueMetadata> _nodeMetas;

        public static IReadOnlyCollection<DialogueMetadata> NodeMetadatas => _nodeMetas;

        static DialogueNodeFactory()
        {
            // 현재 어셈블리의 클래스 중 DialogueNodeTypeAttribute 어트리뷰트를 수식한 클래스의 메타데이터를 가져옴
            _nodeMetas = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.GetCustomAttribute(typeof(DialogueNodeTypeAttribute), false) != null)
                .Select(t =>
                    new DialogueMetadata(
                        t.GetCustomAttribute(typeof(DialogueNodeTypeAttribute)) as DialogueNodeTypeAttribute, t))
                .ToList();
        }

        public static DialogueMetadata GetMetadata(string typeName)
            => _nodeMetas.FirstOrDefault(x => x.Attribute.TypeName == typeName);

        public static DialogueEditorNode CreateFromData(DialogueNodeData data, DialogueGraphView view,
            DialogueContainer containerCache)
        {
            var node = CreateEmptyNode(data.TypeName, view, data.Position);

            if (node == null)
            {
                return null;
            }

            var clonedData = data.Clone();
            if (clonedData == null)
            {
                return null;
            }
            
            node.FromSerializedData(clonedData, containerCache);
            return node;
        }

        public static DialogueEditorNode CreateEmptyNode(string typeName, DialogueGraphView view, Vector2 localMousePosition)
        {
            var metadata = GetMetadata(typeName);

            if (metadata == null)
            {
                Debug.LogError($"존재하지 않는 typeName({typeName})");
                return null;
            }

            if (Activator.CreateInstance(metadata.Type, view, metadata.Attribute.TypeName) is not DialogueEditorNode node)
            {
                Debug.LogError($"유효하지 않는 DialogueNode 타입({metadata.Type})");
                return null;
            }

            node.Initialize(localMousePosition);
            return node;
        }

        public static EntryPointEditorNode CreateEntryPointNode(DialogueGraphView view)
        {
            var node = new EntryPointEditorNode(view, "Start");
            node.Initialize(default);

            return node;
        }
    }
}