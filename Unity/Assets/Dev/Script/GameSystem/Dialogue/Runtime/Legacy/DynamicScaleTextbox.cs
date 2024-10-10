using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DynamicScaleTextbox : MonoBehaviour
{
    [SerializeField] private TMP_Text textBox;
    [SerializeField] private GameObject targetObject;
    [SerializeField] private Vector2 sizeDefault = new(0, 0);
    [SerializeField] private Vector2 sizeOffset = new(0, 0);
    
    private Dictionary<Transform, Vector3> childAnchoredPositions = new();

    private void Start()
    {
        // 모든 자식의 위치 저장
        StoreAnchoredPositions(targetObject.transform);
    }

    private void Update()
    {
        if (textBox.text.Length == 0)
        {
            textBox.ForceMeshUpdate();
            targetObject.GetComponent<RectTransform>().sizeDelta = sizeOffset;
            targetObject.SetActive(false);
            return;
        }

        targetObject.SetActive(true);

        AdjustSizeAndRestorePositions();
    }

    private void StoreAnchoredPositions(Transform target)
    {
        foreach (Transform child in target)
        {
            RectTransform rectChild = child.GetComponent<RectTransform>();
            if (rectChild)
            {
                childAnchoredPositions[child] = rectChild.anchoredPosition;
            }
            StoreAnchoredPositions(child);
        }
    }

    private void RestoreAnchoredPositions(Transform target)
    {
        foreach (Transform child in target)
        {
            RectTransform rectChild = child.GetComponent<RectTransform>();
            if (rectChild && childAnchoredPositions.ContainsKey(child))
            {
                rectChild.anchoredPosition = childAnchoredPositions[child];
            }
            RestoreAnchoredPositions(child);
        }
    }

    private void AdjustSizeAndRestorePositions()
    {
        // 크기 조절
        var newSize = textBox.GetRenderedValues(true) + sizeOffset;
        newSize = new Vector2(Math.Max(newSize.x, sizeDefault.x), Math.Max(newSize.y, sizeDefault.y));
        targetObject.GetComponent<RectTransform>().sizeDelta = newSize;

        // 저장된 위치로 모든 자식의 위치 복원
        RestoreAnchoredPositions(targetObject.transform);
    }
}
