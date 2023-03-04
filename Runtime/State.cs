using System;
using UnityEngine;

namespace StateMachines.Runtime
{
    [Serializable]
    public class State
    {
        private bool _currentlyUsing;
        
        public void Enter()
        {
            if (_currentlyUsing)
                throw new InvalidOperationException("Can't enter state before leaving it");

            _currentlyUsing = true;
            OnEnter();
        }

        public void Exit()
        {
            if (_currentlyUsing == false)
                throw new InvalidOperationException("Can't exit state before entering");
            
            _currentlyUsing = false;
            OnExit();
        }

        public void Update(float deltaTime)
        {
            if (_currentlyUsing == false)
                throw new InvalidOperationException("State not using right now");
            OnUpdate(deltaTime);
        }
        
        public virtual void OnDisable() {}
        public virtual void Initialize() {}
        
        protected virtual void OnEnter() {}
        protected virtual void OnExit() {}

        protected virtual void OnUpdate(float deltaTime) { }
    }
}