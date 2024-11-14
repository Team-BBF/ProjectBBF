using System;
using System.Collections.Generic;
using UnityEngine;


namespace ProjectBBF.Input
{
    public abstract class BaseInput<TOwner>
    {
        public TOwner Owner { get; private set; }

        public BaseInput<TOwner> Init(TOwner owner)
        {
            Debug.Assert(owner is not null);
            Owner = owner;

            OnInit();

            return this;
        }

        public abstract void OnInit();
        public abstract void Update();
        public abstract void Release();
    }

    public interface IInputController<TOwner, in TFactory>
        where TFactory : IInputFactory<TOwner>
    {
        public void BindInput(TFactory factory);
        public void Release();
        public void Update();
    }
    
    public static class InputAbstractFactory
    {
        public static TFactory CreateFactory<TOwner, TFactory>(TOwner owner)
        where TFactory : IInputFactory<TOwner>, new()
        {
            TFactory factory = new TFactory();
            factory.Init(owner);
            return factory;
        }
    }
    
    public abstract class BaseInputController<TOwner, TFactory>
        : IInputController<TOwner, TFactory>
        where TFactory : IInputFactory<TOwner>
    {
        public TOwner Owner { get; private set; }

        public void Init(TOwner owner)
        {
            Debug.Assert(owner is not null);

            Owner = owner;
            
            OnInit();
        }

        public abstract void BindInput(TFactory factory);
        
        public abstract void OnInit();
        public abstract void Release();
        public abstract void Update();
    }

    public interface IInputFactory<in TOwner>
    {
        public void Init(TOwner owner);
    }
}