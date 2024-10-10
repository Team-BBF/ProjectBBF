using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DS.Core
{
    public class DialogueAnimator : MonoBehaviour
    {
        public static DialogueAnimator Instance;

        [Header("Wave Animation Settings")]
        [SerializeField] private float waveSpeed = 7f;
        [SerializeField] private float waveHeight = 0.1f;
        [SerializeField] private float waveDifference = 1f;

        [Header("Shake Animation Settings")]
        [SerializeField] private float shakeMagnitude = 0.04f;

        private TMP_Text _textBox;
        private List<DialogueUtility.Command> _commands = new();
        private float _originTextFontSize;
        private float _currentTextSpeed;
        private string _renderText;

        #region Unity.Event

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void LateUpdate()
        {
            if (_textBox == null 
                || _textBox.text.Length == 0)
                return;

            UpdateMeshInfo();
        }
        
        #endregion

        public void ChangeTextBox(TMP_Text textBox)
        {
            _textBox = textBox;
        }

        public void StopCurrentAnimation()
        {
            if (_textBox == null || _textBox.text.Length == 0)
                return;

            _textBox.text = "";
            _renderText = "";
            _originTextFontSize = _textBox.fontSize;
        }

        public IEnumerator AnimateTextIn(List<DialogueUtility.Command> commands, Action onComplete, bool skipTyping = false)
        {
            InitializeAnimation(commands);
            
            foreach (var command in _commands)
            {
                if (skipTyping)
                {
                    FastExecuteCommand(command);
                    continue;
                }
        
                yield return ExecuteCommand(command);
            }
            
            yield return null;
            
            onComplete?.Invoke();
        }

        private void InitializeAnimation(List<DialogueUtility.Command> commands)
        {
            _textBox.text = "";
            _renderText = "";
            _commands = commands;
            _originTextFontSize = _textBox.fontSize;
            HandleTextSpeedChange(DialogueUtility.TextAnimationSpeed["normal"]);
        }

        private IEnumerator ExecuteCommand(DialogueUtility.Command command)
        {
            switch (command.commandType)
            {
                case DialogueUtility.CommandType.Pause:
                    yield return new WaitForSeconds(command.floatValue);
                    break;
                case DialogueUtility.CommandType.TextSpeedChange:
                    HandleTextSpeedChange(command.floatValue);
                    break;
                case DialogueUtility.CommandType.Size:
                    AppendToTextBox($"</size><size={_originTextFontSize * command.floatValue}>");
                    break;
                case DialogueUtility.CommandType.State:
                    HandleState(command.stringValue);
                    break;
                default:
                    yield return TypeText(command.stringValue);
                    break;
            }
        }
        
        private void FastExecuteCommand(DialogueUtility.Command command)
        {
            switch (command.commandType)
            {
                case DialogueUtility.CommandType.Pause:
                case DialogueUtility.CommandType.TextSpeedChange:
                    break;
                case DialogueUtility.CommandType.Size:
                    AppendToTextBox($"</size><size={_originTextFontSize * command.floatValue}>");
                    break;
                case DialogueUtility.CommandType.State:
                    HandleState(command.stringValue);
                    break;
                default:
                    AppendToTextBox(command.stringValue);
                    break;
            }
        }

        private void HandleTextSpeedChange(float newSpeed)
        {
            _currentTextSpeed = newSpeed;
        }
        
        private void HandleState(string state)
        {
            var match = Regex.Match(state, @"(\w+)@(\w+)");
            if (match.Success)
            {
                string id = match.Groups[1].Value;
                string anim = match.Groups[2].Value; // 애니메이션은 'Idle'과 같은 형태로 들어가지 않고 'Player@Idle'처럼 들어갑니다
                
                DialogueManager.Instance.GetHandler(id)?.SetAnimation(state);
            }
        }

        private IEnumerator TypeText(string text)
        {
            foreach (var character in text)
            {
                AppendToTextBox($"{character}");
                yield return new WaitForSeconds(CalculateTextSpeed());
            }
        }

        private float CalculateTextSpeed()
        {
            return _currentTextSpeed;
        }

        private void AppendToTextBox(string text)
        {
            _renderText += text;
            _textBox.text = $"<size={_originTextFontSize}>{_renderText}</size>";
        }

        private void UpdateMeshInfo()
        {
            _textBox.ForceMeshUpdate();

            TMP_TextInfo textInfo = _textBox.textInfo;
            
            if (textInfo == null)
                return;
            
            AnimateText(textInfo);
            UpdateGeometry(textInfo);
        }

        private void AnimateText(TMP_TextInfo textInfo)
        {
            foreach (var command in _commands)
            {
                if (command.commandType != DialogueUtility.CommandType.Animation
                    || command.textAnimationType == DialogueUtility.TextAnimationType.None)
                    continue;

                switch (command.textAnimationType)
                {
                    case DialogueUtility.TextAnimationType.Wave:
                        AnimateWave(textInfo, command);
                        break;
                    case DialogueUtility.TextAnimationType.Shake:
                        AnimateShake(textInfo, command);
                        break;
                }
            }
        }

        private void UpdateGeometry(TMP_TextInfo textInfo)
        {
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                TMP_MeshInfo meshInfo = textInfo.meshInfo[i];
                meshInfo.mesh.vertices = meshInfo.vertices;
                _textBox.UpdateGeometry(meshInfo.mesh, i);
            }
        }

        private void AnimateWave(TMP_TextInfo textInfo, DialogueUtility.Command command)
        {
            for (int i = 0; i < textInfo.characterCount; i++)
            {
                if (i < command.startIndex || i > command.endIndex)
                    continue;

                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                if (!charInfo.isVisible)
                    continue;

                var vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

                for (int j = 0; j < 4; j++)
                {
                    Vector3 vertex = vertices[charInfo.vertexIndex + j];
                    vertex.y += Mathf.Sin(Time.time * waveSpeed + vertex.x * waveDifference) * waveHeight;
                    vertices[charInfo.vertexIndex + j] = vertex;
                }
            }
        }

        private void AnimateShake(TMP_TextInfo textInfo, DialogueUtility.Command command)
        {
            for (int i = 0; i < textInfo.characterCount; i++)
            {
                if (i < command.startIndex || i > command.endIndex)
                    continue;

                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                if (!charInfo.isVisible)
                    continue;

                var vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

                for (int j = 0; j < 4; j++)
                {
                    Vector3 vertex = vertices[charInfo.vertexIndex + j];
                    var offsetX = Random.Range(-shakeMagnitude, shakeMagnitude);
                    var offsetY = Random.Range(-shakeMagnitude, shakeMagnitude);
                    vertex.x += offsetX;
                    vertex.y += offsetY;
                    vertices[charInfo.vertexIndex + j] = vertex;
                }
            }
        }
    }
}
