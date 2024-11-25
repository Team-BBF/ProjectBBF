


using DS.Core;
using UnityEngine;

public class MapDialogueEventReceiverStoveFrog : MapDialogueEventReceiver
{
    [SerializeField] private DialogueContainer _stoveFrog;

    public override DialogueContainer Container
    {
        get
        {
#if STOVE_BUILD
            return _stoveFrog;
#endif

            return base.Container;
        }
    }
}