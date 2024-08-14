using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IActorStrategy
{
}
public interface IBAFavorablity : IActorBehaviour
{
    public FavorablityContainer FavorablityContainer { get; }
}

public interface IBANameKey : IActorBehaviour
{
    public string ActorKey { get; }
}

public interface IBAStateTransfer : IActorBehaviour
{
    public void TranslateState(string stateKey);
}