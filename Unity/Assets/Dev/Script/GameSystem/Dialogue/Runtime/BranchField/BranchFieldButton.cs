using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyBox;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace DS.Runtime
{
    public class BranchResultButton : DialogueBranchResult
    {
    }
    
    public class BranchFieldButton : DialogueBranchField, IChooseable
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _text;

        public string Text
        {
            get => _text.text;
            set
            {
                _text.text = value;
                
                LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.As<RectTransform>());
            }
        }

        public BranchFieldButton Init(string text)
        {
            Text = text;

            return this;
        }
        public async UniTask<DialogueBranchResult> GetResult(CancellationToken token = default)
        {
            CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(token, this.GetCancellationTokenOnDestroy());

            gameObject.SetActive(true);
            _ =  await _button.OnClickAsync(cts.Token).SuppressCancellationThrow();
            
            return new BranchResultButton();
        }

        public override void DestroySelf()
        {
            Destroy(gameObject);
        }
    }

}