//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.7.0
//     from Assets/Dev/Script/InputSystem/DefaultKeymap.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @DefaultKeymap: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @DefaultKeymap()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""DefaultKeymap"",
    ""maps"": [
        {
            ""name"": ""PlayerControl"",
            ""id"": ""2a21b6ac-1b20-440f-b6f5-3e06dc8fdfd2"",
            ""actions"": [
                {
                    ""name"": ""Movement"",
                    ""type"": ""Value"",
                    ""id"": ""5b51b5fa-c834-48bd-8767-15860c5e0051"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Sprint"",
                    ""type"": ""PassThrough"",
                    ""id"": ""690a4b11-ab1b-41db-af4f-b43422d821e9"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""BuilderMode"",
                    ""type"": ""Button"",
                    ""id"": ""2a2a3034-9b6b-411e-9456-a6d73a735b34"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Collect"",
                    ""type"": ""Button"",
                    ""id"": ""5bb6e3fe-0e7f-46c4-84ab-0423b6fde981"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ShakingMiniGameInteraction"",
                    ""type"": ""Button"",
                    ""id"": ""9dd94aa4-44a8-45d1-9612-51c8c02574a7"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""DialogueSkip"",
                    ""type"": ""Button"",
                    ""id"": ""6d024a56-b456-4478-bac8-ddaf4c6f4925"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""MoveLeftIngredientSelection"",
                    ""type"": ""Button"",
                    ""id"": ""b18ce12a-00bb-47bf-b046-0294ff010ec3"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""MoveRightIngredientSelection"",
                    ""type"": ""Button"",
                    ""id"": ""f87f2400-9937-4a86-90a6-a60f98493195"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""DestroyObject"",
                    ""type"": ""Button"",
                    ""id"": ""5a61ab1f-d552-456e-b080-856e8b62fffe"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""QuickSlotScroll"",
                    ""type"": ""PassThrough"",
                    ""id"": ""8de32bb8-4940-4d21-ab65-5f8c42879b7e"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""QuickSlotScrollButton"",
                    ""type"": ""Value"",
                    ""id"": ""038c79a3-707f-420e-8739-50d47db3c0e0"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""WASD"",
                    ""id"": ""46219aa2-8bbd-4014-9c6f-2e21b5d8f522"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": ""NormalizeVector2"",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""02a1f54e-7310-414a-8ed4-c4571d571d34"",
                    ""path"": ""<Keyboard>/#(W)"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""WinPCScheme"",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""e7250860-4bad-417f-9468-46c3f33c812b"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""WinPCScheme"",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""f0308ced-dc32-4b57-8780-36fe9e15a523"",
                    ""path"": ""<Keyboard>/#(A)"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""WinPCScheme"",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""eddaaf4d-f106-4b13-a6b7-b666f65a64e2"",
                    ""path"": ""<Keyboard>/#(D)"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""WinPCScheme"",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""905ea5a5-328d-4777-9017-1ac8bf617cdc"",
                    ""path"": ""<Keyboard>/leftShift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""WinPCScheme"",
                    ""action"": ""Sprint"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7ed886e7-4c6e-4f49-8ebb-ad5a37594b0c"",
                    ""path"": ""<Keyboard>/b"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""WinPCScheme"",
                    ""action"": ""BuilderMode"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""109f2777-561c-45fb-acf6-4c913737fc3a"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""WinPCScheme"",
                    ""action"": ""Collect"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a064210b-bcf6-419d-bb76-d811aa971ffe"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""WinPCScheme"",
                    ""action"": ""ShakingMiniGameInteraction"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8432545f-fa34-4943-b0e6-97ca55756fbd"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""WinPCScheme"",
                    ""action"": ""DialogueSkip"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""6fef4a9e-962f-40cf-8f0c-dac8e773fdea"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""WinPCScheme"",
                    ""action"": ""DialogueSkip"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7551d58b-7182-4a53-8aa6-37a9c1d874c5"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""WinPCScheme"",
                    ""action"": ""DialogueSkip"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""6b5099e9-86b9-4fc7-b520-cc553e322b9e"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""WinPCScheme"",
                    ""action"": ""MoveLeftIngredientSelection"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1e9ad895-3356-4a12-adcb-3cbb2d670543"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""WinPCScheme"",
                    ""action"": ""MoveRightIngredientSelection"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8c53c89b-1df0-4e25-adcc-0c1157f539cb"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""WinPCScheme"",
                    ""action"": ""DestroyObject"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""644ecf5c-139e-4942-8a51-fee02b3ea77e"",
                    ""path"": ""<Mouse>/scroll/y"",
                    ""interactions"": """",
                    ""processors"": ""Invert"",
                    ""groups"": ""WinPCScheme"",
                    ""action"": ""QuickSlotScroll"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""608c7edb-0bfd-4655-a0ea-3778c189688b"",
                    ""path"": ""<Keyboard>/1"",
                    ""interactions"": """",
                    ""processors"": ""Scale"",
                    ""groups"": """",
                    ""action"": ""QuickSlotScrollButton"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""34879d20-6597-4562-a779-d651c2fcfba5"",
                    ""path"": ""<Keyboard>/2"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=2)"",
                    ""groups"": """",
                    ""action"": ""QuickSlotScrollButton"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""367c4166-6ce4-486f-96c3-c1f68998b0a3"",
                    ""path"": ""<Keyboard>/3"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=3)"",
                    ""groups"": """",
                    ""action"": ""QuickSlotScrollButton"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e32d8a83-1a1d-4b69-b25b-8f90702f27db"",
                    ""path"": ""<Keyboard>/4"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=4)"",
                    ""groups"": """",
                    ""action"": ""QuickSlotScrollButton"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c2b24ba5-75eb-4477-8376-25d811eea5a0"",
                    ""path"": ""<Keyboard>/5"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=5)"",
                    ""groups"": """",
                    ""action"": ""QuickSlotScrollButton"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c0468866-70a7-46f0-829d-19ff6eea8472"",
                    ""path"": ""<Keyboard>/6"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=6)"",
                    ""groups"": """",
                    ""action"": ""QuickSlotScrollButton"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7abcd911-2a27-41c6-a423-ab275cb335f0"",
                    ""path"": ""<Keyboard>/7"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=7)"",
                    ""groups"": """",
                    ""action"": ""QuickSlotScrollButton"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""50d8f083-2b32-42c2-b7c7-7d4d6dbadf40"",
                    ""path"": ""<Keyboard>/8"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=8)"",
                    ""groups"": """",
                    ""action"": ""QuickSlotScrollButton"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""77e3e6f3-741b-4fb9-8f38-dd471a33194a"",
                    ""path"": ""<Keyboard>/9"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=9)"",
                    ""groups"": """",
                    ""action"": ""QuickSlotScrollButton"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2d35e34f-6eed-48d4-8541-78f3bb3f425c"",
                    ""path"": ""<Keyboard>/0"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=10)"",
                    ""groups"": """",
                    ""action"": ""QuickSlotScrollButton"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""WinPCScheme"",
            ""bindingGroup"": ""WinPCScheme"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Mouse>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // PlayerControl
        m_PlayerControl = asset.FindActionMap("PlayerControl", throwIfNotFound: true);
        m_PlayerControl_Movement = m_PlayerControl.FindAction("Movement", throwIfNotFound: true);
        m_PlayerControl_Sprint = m_PlayerControl.FindAction("Sprint", throwIfNotFound: true);
        m_PlayerControl_BuilderMode = m_PlayerControl.FindAction("BuilderMode", throwIfNotFound: true);
        m_PlayerControl_Collect = m_PlayerControl.FindAction("Collect", throwIfNotFound: true);
        m_PlayerControl_ShakingMiniGameInteraction = m_PlayerControl.FindAction("ShakingMiniGameInteraction", throwIfNotFound: true);
        m_PlayerControl_DialogueSkip = m_PlayerControl.FindAction("DialogueSkip", throwIfNotFound: true);
        m_PlayerControl_MoveLeftIngredientSelection = m_PlayerControl.FindAction("MoveLeftIngredientSelection", throwIfNotFound: true);
        m_PlayerControl_MoveRightIngredientSelection = m_PlayerControl.FindAction("MoveRightIngredientSelection", throwIfNotFound: true);
        m_PlayerControl_DestroyObject = m_PlayerControl.FindAction("DestroyObject", throwIfNotFound: true);
        m_PlayerControl_QuickSlotScroll = m_PlayerControl.FindAction("QuickSlotScroll", throwIfNotFound: true);
        m_PlayerControl_QuickSlotScrollButton = m_PlayerControl.FindAction("QuickSlotScrollButton", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }

    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // PlayerControl
    private readonly InputActionMap m_PlayerControl;
    private List<IPlayerControlActions> m_PlayerControlActionsCallbackInterfaces = new List<IPlayerControlActions>();
    private readonly InputAction m_PlayerControl_Movement;
    private readonly InputAction m_PlayerControl_Sprint;
    private readonly InputAction m_PlayerControl_BuilderMode;
    private readonly InputAction m_PlayerControl_Collect;
    private readonly InputAction m_PlayerControl_ShakingMiniGameInteraction;
    private readonly InputAction m_PlayerControl_DialogueSkip;
    private readonly InputAction m_PlayerControl_MoveLeftIngredientSelection;
    private readonly InputAction m_PlayerControl_MoveRightIngredientSelection;
    private readonly InputAction m_PlayerControl_DestroyObject;
    private readonly InputAction m_PlayerControl_QuickSlotScroll;
    private readonly InputAction m_PlayerControl_QuickSlotScrollButton;
    public struct PlayerControlActions
    {
        private @DefaultKeymap m_Wrapper;
        public PlayerControlActions(@DefaultKeymap wrapper) { m_Wrapper = wrapper; }
        public InputAction @Movement => m_Wrapper.m_PlayerControl_Movement;
        public InputAction @Sprint => m_Wrapper.m_PlayerControl_Sprint;
        public InputAction @BuilderMode => m_Wrapper.m_PlayerControl_BuilderMode;
        public InputAction @Collect => m_Wrapper.m_PlayerControl_Collect;
        public InputAction @ShakingMiniGameInteraction => m_Wrapper.m_PlayerControl_ShakingMiniGameInteraction;
        public InputAction @DialogueSkip => m_Wrapper.m_PlayerControl_DialogueSkip;
        public InputAction @MoveLeftIngredientSelection => m_Wrapper.m_PlayerControl_MoveLeftIngredientSelection;
        public InputAction @MoveRightIngredientSelection => m_Wrapper.m_PlayerControl_MoveRightIngredientSelection;
        public InputAction @DestroyObject => m_Wrapper.m_PlayerControl_DestroyObject;
        public InputAction @QuickSlotScroll => m_Wrapper.m_PlayerControl_QuickSlotScroll;
        public InputAction @QuickSlotScrollButton => m_Wrapper.m_PlayerControl_QuickSlotScrollButton;
        public InputActionMap Get() { return m_Wrapper.m_PlayerControl; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerControlActions set) { return set.Get(); }
        public void AddCallbacks(IPlayerControlActions instance)
        {
            if (instance == null || m_Wrapper.m_PlayerControlActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_PlayerControlActionsCallbackInterfaces.Add(instance);
            @Movement.started += instance.OnMovement;
            @Movement.performed += instance.OnMovement;
            @Movement.canceled += instance.OnMovement;
            @Sprint.started += instance.OnSprint;
            @Sprint.performed += instance.OnSprint;
            @Sprint.canceled += instance.OnSprint;
            @BuilderMode.started += instance.OnBuilderMode;
            @BuilderMode.performed += instance.OnBuilderMode;
            @BuilderMode.canceled += instance.OnBuilderMode;
            @Collect.started += instance.OnCollect;
            @Collect.performed += instance.OnCollect;
            @Collect.canceled += instance.OnCollect;
            @ShakingMiniGameInteraction.started += instance.OnShakingMiniGameInteraction;
            @ShakingMiniGameInteraction.performed += instance.OnShakingMiniGameInteraction;
            @ShakingMiniGameInteraction.canceled += instance.OnShakingMiniGameInteraction;
            @DialogueSkip.started += instance.OnDialogueSkip;
            @DialogueSkip.performed += instance.OnDialogueSkip;
            @DialogueSkip.canceled += instance.OnDialogueSkip;
            @MoveLeftIngredientSelection.started += instance.OnMoveLeftIngredientSelection;
            @MoveLeftIngredientSelection.performed += instance.OnMoveLeftIngredientSelection;
            @MoveLeftIngredientSelection.canceled += instance.OnMoveLeftIngredientSelection;
            @MoveRightIngredientSelection.started += instance.OnMoveRightIngredientSelection;
            @MoveRightIngredientSelection.performed += instance.OnMoveRightIngredientSelection;
            @MoveRightIngredientSelection.canceled += instance.OnMoveRightIngredientSelection;
            @DestroyObject.started += instance.OnDestroyObject;
            @DestroyObject.performed += instance.OnDestroyObject;
            @DestroyObject.canceled += instance.OnDestroyObject;
            @QuickSlotScroll.started += instance.OnQuickSlotScroll;
            @QuickSlotScroll.performed += instance.OnQuickSlotScroll;
            @QuickSlotScroll.canceled += instance.OnQuickSlotScroll;
            @QuickSlotScrollButton.started += instance.OnQuickSlotScrollButton;
            @QuickSlotScrollButton.performed += instance.OnQuickSlotScrollButton;
            @QuickSlotScrollButton.canceled += instance.OnQuickSlotScrollButton;
        }

        private void UnregisterCallbacks(IPlayerControlActions instance)
        {
            @Movement.started -= instance.OnMovement;
            @Movement.performed -= instance.OnMovement;
            @Movement.canceled -= instance.OnMovement;
            @Sprint.started -= instance.OnSprint;
            @Sprint.performed -= instance.OnSprint;
            @Sprint.canceled -= instance.OnSprint;
            @BuilderMode.started -= instance.OnBuilderMode;
            @BuilderMode.performed -= instance.OnBuilderMode;
            @BuilderMode.canceled -= instance.OnBuilderMode;
            @Collect.started -= instance.OnCollect;
            @Collect.performed -= instance.OnCollect;
            @Collect.canceled -= instance.OnCollect;
            @ShakingMiniGameInteraction.started -= instance.OnShakingMiniGameInteraction;
            @ShakingMiniGameInteraction.performed -= instance.OnShakingMiniGameInteraction;
            @ShakingMiniGameInteraction.canceled -= instance.OnShakingMiniGameInteraction;
            @DialogueSkip.started -= instance.OnDialogueSkip;
            @DialogueSkip.performed -= instance.OnDialogueSkip;
            @DialogueSkip.canceled -= instance.OnDialogueSkip;
            @MoveLeftIngredientSelection.started -= instance.OnMoveLeftIngredientSelection;
            @MoveLeftIngredientSelection.performed -= instance.OnMoveLeftIngredientSelection;
            @MoveLeftIngredientSelection.canceled -= instance.OnMoveLeftIngredientSelection;
            @MoveRightIngredientSelection.started -= instance.OnMoveRightIngredientSelection;
            @MoveRightIngredientSelection.performed -= instance.OnMoveRightIngredientSelection;
            @MoveRightIngredientSelection.canceled -= instance.OnMoveRightIngredientSelection;
            @DestroyObject.started -= instance.OnDestroyObject;
            @DestroyObject.performed -= instance.OnDestroyObject;
            @DestroyObject.canceled -= instance.OnDestroyObject;
            @QuickSlotScroll.started -= instance.OnQuickSlotScroll;
            @QuickSlotScroll.performed -= instance.OnQuickSlotScroll;
            @QuickSlotScroll.canceled -= instance.OnQuickSlotScroll;
            @QuickSlotScrollButton.started -= instance.OnQuickSlotScrollButton;
            @QuickSlotScrollButton.performed -= instance.OnQuickSlotScrollButton;
            @QuickSlotScrollButton.canceled -= instance.OnQuickSlotScrollButton;
        }

        public void RemoveCallbacks(IPlayerControlActions instance)
        {
            if (m_Wrapper.m_PlayerControlActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IPlayerControlActions instance)
        {
            foreach (var item in m_Wrapper.m_PlayerControlActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_PlayerControlActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public PlayerControlActions @PlayerControl => new PlayerControlActions(this);
    private int m_WinPCSchemeSchemeIndex = -1;
    public InputControlScheme WinPCSchemeScheme
    {
        get
        {
            if (m_WinPCSchemeSchemeIndex == -1) m_WinPCSchemeSchemeIndex = asset.FindControlSchemeIndex("WinPCScheme");
            return asset.controlSchemes[m_WinPCSchemeSchemeIndex];
        }
    }
    public interface IPlayerControlActions
    {
        void OnMovement(InputAction.CallbackContext context);
        void OnSprint(InputAction.CallbackContext context);
        void OnBuilderMode(InputAction.CallbackContext context);
        void OnCollect(InputAction.CallbackContext context);
        void OnShakingMiniGameInteraction(InputAction.CallbackContext context);
        void OnDialogueSkip(InputAction.CallbackContext context);
        void OnMoveLeftIngredientSelection(InputAction.CallbackContext context);
        void OnMoveRightIngredientSelection(InputAction.CallbackContext context);
        void OnDestroyObject(InputAction.CallbackContext context);
        void OnQuickSlotScroll(InputAction.CallbackContext context);
        void OnQuickSlotScrollButton(InputAction.CallbackContext context);
    }
}
