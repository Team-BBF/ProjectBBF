using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ProjectBBF.Event;
using MyBox;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerStateTranslator : MonoBehaviour, IPlayerStrategy
{
    public EPlayerControlState PeekState { get; set; }

    public void Init(PlayerController controller)
    {
    }

    public EPlayerControlState Pop()
    {
        var state = PeekState;
        PeekState = EPlayerControlState.None;
        return state;
    }

    public void OnUpdate()
    {
        
        // collecting
        if (InputManager.Map.Player.Interaction.triggered)
        {
            PeekState = EPlayerControlState.Collection;
        }
    }
}

[System.Serializable]
public enum EInputType
{
    Trigger,
    Pressed,
}

[UnitCategory("ProjectBBF/InputAction")]
public class InputActionUnit : Unit
{
    private ControlInput _input;
    private ControlOutput _outputSuccess;
    private ControlOutput _outputFail;

    private ValueInput _actionMap;
    private ValueInput _action;
    private ValueInput _type;
    
    private ValueOutput _vSuccess;
    
    protected override void Definition()
    {
        _input = ControlInput("", Execute);
        _outputSuccess = ControlOutput("Success");
        _outputFail = ControlOutput("Fail");

        _actionMap = ValueInput<string>("Input Action Map");
        _action = ValueInput<string>("Input Action");
        _type = ValueInput<EInputType>("Input Type");
        
        _vSuccess = ValueOutput<bool>("IsSuccess");
    }

    private ControlOutput Execute(Flow flow)
    {
        var actionStr = flow.GetValue<string>(_action);
        var actionMapStr = flow.GetValue<string>(_actionMap);

        var actionMap = InputManager.Map.asset.FindActionMap(actionMapStr);
        var action= actionMap.FindAction(actionStr);
        Debug.Assert(action != null, $"map: '{actionStr}', action: {actionMapStr}' input action을 찾을 수 없습니다.");
        
        var type = flow.GetValue<EInputType>(_type);

        ControlOutput output = null;

        switch (type)
        {
            case EInputType.Trigger:
                output = action.triggered ? _outputSuccess : _outputFail;
                break;
            case EInputType.Pressed:
                output = action.IsPressed() ? _outputSuccess : _outputFail;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        flow.SetValue(_vSuccess, output == _outputSuccess);
        return output;
    }
}

public class PlayerStateTranslatorUnit : Unit
{
    private ControlInput _input;
    private ControlOutput _outputSuccess;
    private ControlOutput _outputFailure;

    private ValueInput _state;
    private ValueInput _playerController;
    
    
    protected override void Definition()
    {
        _input = ControlInput("", Update);
        _outputSuccess = ControlOutput("Success");
        _outputFailure = ControlOutput("Failure");

        _state = ValueInput<EPlayerControlState>("State");
        _playerController = ValueInput<PlayerController>("PlayerController");
    }

    private ControlOutput Update(Flow flow)
    {
        var controller = flow.GetValue<PlayerController>(_playerController);
        var translator = controller.Translator;

        if (flow.GetValue<EPlayerControlState>(_state) == translator.PeekState)
        {
            return _outputSuccess;
        }

        return _outputFailure;
    }
}