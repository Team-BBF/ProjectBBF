using System;
using System.Collections;
using Cinemachine;
using TMPro;
using UnityEngine;

namespace DS.Core
{
    public class DialogueHandler : MonoBehaviour
    {
        [SerializeField] private string ID;
        [SerializeField] private string Name;
        
        [SerializeField] private DialogueSetter dialogueSetter;

        [SerializeField] private CinemachineFreeLook cinemachineFreeLook;

        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
        }

        private void OnEnable()
        {
            StartCoroutine(RegisterToDialogueManager());
        }

        private IEnumerator RegisterToDialogueManager()
        {
            yield return null;

            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.Handlers[ID] = this;
            }
            else
            {
                Debug.LogError("DialogueManager instance is not ready yet.");
            }
        }
        
        public void LookTarget(float xAxis)
        {
            if (cinemachineFreeLook == null)
                return;
            
            cinemachineFreeLook.m_XAxis.Value = xAxis;
            cinemachineFreeLook.Priority = 11;
        }
        
        public void DisableLookTarget()
        {
            if (cinemachineFreeLook == null)
                return;
            cinemachineFreeLook.Priority = 9;
        }
        
        public float GetXAxis()
        {
            if (cinemachineFreeLook == null)
                return 0f;
            return cinemachineFreeLook.m_XAxis.Value;
        }

        public string GetName()
        {
            return Name;
        }

        public void SetName(string newName)
        {
            Name = newName;
        }

        public void SetAnimation(string id)
        {
            if (_animator == null)
                return;
            
            if (!_animator.HasState(0, Animator.StringToHash(id)))
            {
                Debug.LogWarning($"{id} 애니메이션이 존재하지 않습니다.");
                return;
            }
            
            _animator.CrossFade(id, 0f);
        }

        public void PlayDialogue(string text, bool skipTyping = false)
        {
            dialogueSetter.nameBox.text = Name;
            if (!skipTyping)
            {
                //TODO: 레거시 코드
                //SoundManager.Instance.PlaySound("UI_Dia_Check");
            }
            DialogueManager.Instance.PlayDialogue(dialogueSetter.textBox, text, skipTyping);
        }
    }
}

