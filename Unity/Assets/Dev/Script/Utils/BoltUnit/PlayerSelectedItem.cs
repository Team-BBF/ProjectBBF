using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

[UnitCategory("ProjectBBF")]
[UnitTitle("PlayerSelectedItem")]
public class PlayerSelectedItem : Unit
{
    private ControlInput _cInput;
    private ControlOutput _cOutputTrue;
    private ControlOutput _cOutputFalse;

    private ValueInput _vItem;
    private ValueInput _vPlayerController;
    
    protected override void Definition()
    {
        _cInput = ControlInput("", OnInput);
        _cOutputTrue = ControlOutput("True");
        _cOutputFalse = ControlOutput("False");

        _vItem = ValueInput<ItemData>("Check Item");
        _vPlayerController = ValueInput<PlayerController>("PlayerController");
    }

    private ControlOutput OnInput(Flow flow)
    {
        var itemData = flow.GetValue<ItemData>(_vItem);
        if (itemData is null) return null;
        
        var pc = flow.GetValue<PlayerController>(_vPlayerController);
        if(pc is null) return null;
        
        
        return pc.Inventory.CurrentItemData == itemData ? _cOutputTrue : _cOutputFalse; 
    }
}
