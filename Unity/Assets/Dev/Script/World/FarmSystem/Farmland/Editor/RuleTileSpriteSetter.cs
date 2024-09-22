using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "ProjectBBF/Tool/RuleTimeSpriteSetter", fileName = "RuleSpriteSetter")]
public class RuleTileSpriteSetter : ScriptableObject
{
    [SerializeField] private RuleTile _ruleTile;
    [SerializeField] private Sprite[] _sprs;
    
    
    [ButtonMethod]
    private void Set()
    {
        if (_sprs is null) return;
        if (_ruleTile is null) return;
        if (_sprs.Length == 0) return;

        int i = 0;
        foreach (var rule in _ruleTile.m_TilingRules)
        {
            if(_sprs.Length <= i)return;
            
            rule.m_Sprites[0] = _sprs[i];
            i++;
        }
        
        EditorUtility.SetDirty(_ruleTile);
    }
    
    [ButtonMethod]
    private void Clear()
    {
        if (_sprs is null) return;
        if (_ruleTile is null) return;
        if (_sprs.Length == 0) return;

        foreach (var rule in _ruleTile.m_TilingRules)
        {
            for (int r = 0; r < rule.m_Sprites.Length; r++)
            {
                rule.m_Sprites[r] = null;
            }
        }
        
        EditorUtility.SetDirty(_ruleTile);
    }
}
