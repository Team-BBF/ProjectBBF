using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContestResultUI : MonoBehaviour
{
    [SerializeField] private Image _itemImage;
    [SerializeField] private TMP_Text _itemName;
    [SerializeField] private TMP_Text _firstDesc;
    [SerializeField] private TMP_Text _secondDesc;

    private void Awake()
    {
        Visible = false;
    }

    public void Set(Sprite itemIcon, string itemName, string firstDesc, string secondDesc)
    {
        _itemImage.sprite = itemIcon;
        _itemName.text = itemName;
        _firstDesc.text = firstDesc;
        _secondDesc.text = secondDesc;
    }

    public bool Visible
    {
        get => gameObject.activeSelf;
        set=> gameObject.SetActive(value);
    }
}