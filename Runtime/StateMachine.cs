using System;
using System.Linq;
using UnityEngine;

namespace Hermer29.Almasury
{
    public class StateMachine
    {
        private readonly State[] _states;
        private readonly TransitionMethodsBinder _transitionMethodsBinder;
        private State _current;
        private bool _logging;

        public StateMachine(DiContainer container)
        {
            _logging = container._logging;
            var injected = new InjectedContainer(container);
            _states = injected.GetStates().ToArray();
            _current = injected.GetFirst();
            _transitionMethodsBinder = new TransitionMethodsBinder(_states);
        }
        
        public void OnEnable()
        {
            foreach (State state in _states)
            {
                state.Initialize();
            }
            _current.Enter();
        }

        public void OnDisable()
        {
            foreach (var state in _states)
            {
                state.OnDisable();
            }
        }

        public void Update(float deltaTime)
        {
            if (_transitionMethodsBinder.IsTransitionRequired(_current, out Type target))
            {
                if(_logging)
                    Debug.Log($"[Almasure] Started transition from {_current} to {target}");
                _current.Exit();
                _current = _states.First(x => x.GetType() == target);
                _current.Enter();
            }
            _current.Update(deltaTime);
        }
    }
}