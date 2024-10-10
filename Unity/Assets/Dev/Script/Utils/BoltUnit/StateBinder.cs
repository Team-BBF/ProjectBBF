using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

public abstract class StateBehaviour : MonoBehaviour
{
}

public class BehaviourBinder : MonoBehaviour
{
    private Dictionary<Type, StateBehaviour> _table;

    private const string DECLARATION = "__BehaviourBinder__";
    private const string DECLARATION_GAMEOBJECT = "__BehaviourBinder_GameObject__";

    public Dictionary<Type, StateBehaviour> Table
    {
        get
        {
            if (_table == null)
            {
                _table = new Dictionary<Type, StateBehaviour>();
            }

            return _table;
        }
    }

    public void Clear()
    {
        Table.Clear();
        _table = null;
    }

    public static BehaviourBinder GetBinder(GameObject gameObject)
    {
        VariableDeclarations declarations =gameObject.GetComponent<Variables>()?.declarations;

        Debug.Assert(declarations != null);

        if (declarations.IsDefined(DECLARATION))
        {
            return declarations.Get<BehaviourBinder>(DECLARATION);
        }


        var parentObject = declarations.Get<GameObject>(DECLARATION_GAMEOBJECT);

        var obj = new GameObject(DECLARATION);
        obj.transform.SetParent(parentObject.transform);
        obj.transform.position = Vector3.zero;
        obj.transform.localScale = Vector3.one;
        obj.transform.rotation = Quaternion.identity;

        var com = obj.AddComponent<BehaviourBinder>();

        declarations.Set(DECLARATION, com);

        return com;
    }

    public static BehaviourBinder GetBinder(Flow flow)
    {
        return GetBinder(flow.stack.gameObject);
    }

    public void GetBehaviour<T>(out T value)
        where T : StateBehaviour
    {
        Type t = typeof(T);
        if (Table.TryGetValue(t, out var v))
        {
            Debug.Assert(v is T, "타입과 매칭되지 않는 StateBehaviour 입니다.");
            value = v as T;
        }
        else
        {
            var com = gameObject.AddComponent<T>();

            value = com;
            Table.Add(t, com);
        }
    }

    public T GetBehaviour<T>()
        where T : StateBehaviour
    {
        Type t = typeof(T);
        if (Table.TryGetValue(t, out var v))
        {
            Debug.Assert(v is T, "타입과 매칭되지 않는 StateBehaviour 입니다.");
            return v as T;
        }

        var com = gameObject.AddComponent<T>();

        Table.Add(t, com);

        return com;
    }
}