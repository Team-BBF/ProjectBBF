using System;
using System.Collections;
using System.Collections.Generic;
using ProjectBBF.Event;
using ProjectBBF.Input;
using UnityEngine;

public class BakeryStoreage : BakeryFlowBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private StorageInventoryPresenter _storageInventory;
    [SerializeField] private Animator _ani;

    [SerializeField] private List<ItemData> _defaultItems;

    private IEnumerator _coObserveOpen;

    private void Start()
    {
        _storageInventory.OnInit += _ => Init();
        _storageInventory.Init();
    }

    private void Init()
    {
        Visible = false;

        foreach (ItemData item in _defaultItems)
        {
            _storageInventory.Model.PushItem(item, item.MaxStackCount);
        }
    }

    private bool Visible
    {
        get => _panel.activeSelf;
        set
        {
            _panel.SetActive(value);
            _storageInventory.PlayerView.Visible = value;
            _storageInventory.StorageView.Visible = value;
        }
    }

    private IEnumerator CoUpdateInteraction(CollisionInteractionMono activator)
    {
        if (activator.Owner is not PlayerController pc) yield break;

        yield return null;

        while (true)
        {
            if (InputManager.Map.Player.Interaction.triggered ||
                InputManager.Map.UI.Inventory.triggered ||
                InputManager.Map.UI.Setting.triggered)
            {
                yield return null;
                yield return null;

                Visible = false;
                pc.Interactor.IsInteracting = false;
                pc.HudController.Visible = true;
                pc.Inventory.QuickInvVisible = true;

                pc.InputController.BindInput(InputAbstractFactory
                    .CreateFactory<PlayerController, DefaultPlayerInputFactory>(pc));
                break;
            }

            yield return null;
        }
    }

    protected override void OnActivate(BakeryFlowObject flowObject, CollisionInteractionMono activator)
    {
    }

    protected override void OnInteraction(BakeryFlowObject flowObject, CollisionInteractionMono activator)
    {
        if (activator.Owner is not PlayerController pc) return;

        Visible = true;
        pc.MoveStrategy.ResetVelocity();
        pc.Blackboard.IsInteractionStopped = true;
        pc.Blackboard.IsMoveStopped = true;
        pc.HudController.Visible = false;
        pc.Inventory.QuickInvVisible = false;

        pc.InputController.Move.Value = null;
        pc.InputController.Tool.Value = null;
        pc.InputController.Interact.Value = null;

        StopAllCoroutines();
        StartCoroutine(CoUpdateInteraction(activator));
    }

    protected override void OnEnter(BakeryFlowObject flowObject, CollisionInteractionMono activator)
    {
        if (activator.Owner is not PlayerController pc) return;

        StartCoroutine(_coObserveOpen = CoObserveOpen(flowObject, activator));
    }

    private IEnumerator CoObserveOpen(BakeryFlowObject flowObject, CollisionInteractionMono activator)
    {
        if (activator.Owner is not PlayerController pc) yield break;

        while (true)
        {
            if (pc.Interactor.CloserObject == flowObject.Interaction)
            {
                _ani.SetTrigger("Open");
            }
            else
            {
                _ani.SetTrigger("Close");
            }

            yield return null;
        }
    }

    protected override void OnExit(BakeryFlowObject flowObject, CollisionInteractionMono activator)
    {
        if (activator.Owner is not PlayerController pc) return;
        pc.Blackboard.IsInteractionStopped = false;
        pc.Blackboard.IsMoveStopped = false;
        pc.HudController.Visible = true;
        pc.Inventory.QuickInvVisible = true;
        pc.InputController.BindInput(InputAbstractFactory
            .CreateFactory<PlayerController, DefaultPlayerInputFactory>(pc));

        Visible = false;
        _ani.SetTrigger("Close");

        if (_coObserveOpen is not null)
        {
            StopCoroutine(_coObserveOpen);
        }
    }
}