using System;
using System.Collections.Generic;
using DS.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DS.Core
{
    public class MultiDialogueHandler : MonoBehaviour
    {
        [SerializeField] private Transform targetParent;
        [SerializeField] private GameObject itemPrefab;

        private Action<string> _returnAction;

        private bool _observeDialogueEnd;
        
        public static MultiDialogueHandler Instance;
        
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

        private void Update()
        {
            if (!_observeDialogueEnd)
                return; 
            
            if (DialogueManager.Instance.CheckDialogueEnd())
            {
                _observeDialogueEnd = false;
                Invoke(nameof(ShowSelectMessage), 1f);
            }
        }

        public void Init(List<NodeLinkData> links, Action<string> returnAction)
        {
            _returnAction = returnAction;

            foreach (Transform child in targetParent)
            {
                Destroy(child.gameObject);
            }
            
            foreach (var link in links)
            {
                GameObject temp = Instantiate(itemPrefab, targetParent.position, Quaternion.identity);
                temp.transform.SetParent(targetParent);
                
                //TODO: 레거시 코드
                //temp.GetComponent<SelectMessageViewer>().Set(link.PortName, link.TargetNodeGuid);
            }

            _observeDialogueEnd = true;
        }
        
        private void ShowSelectMessage()
        {
            targetParent.gameObject.SetActive(true);
        }
        
        private void Exit()
        {
            targetParent.gameObject.SetActive(false);
        }

        public void SelectChoice(string guid)
        {
            //TODO: 레거시 코드
            //SoundManager.Instance.PlaySound("UI_Dia_Choose_Click");
            _returnAction?.Invoke(guid);
            Exit();
        }
    }
}