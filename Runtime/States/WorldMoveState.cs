using System;
using UnityEngine;

namespace Hermer29.Almasury.States
{
    public class WorldMoveState : State
    {
        private readonly Transform _transform;
        private readonly Func<Vector3> _positionGetter;
        private readonly float _movingTime;
        private readonly Type _nextState;
        
        private Vector3 _targetPosition;
        private float _passedTime;
        private Vector3 _startPosition;
        private bool _ended;

        public WorldMoveState(Transform transform, Func<Vector3> positionGetter, float movingTime, Type nextState)
        {
            _transform = transform;
            _positionGetter = positionGetter;
            _movingTime = movingTime;
            _nextState = nextState;
        }

        protected override void OnEnter()
        {
            _ended = false;
            _passedTime = 0;
            _targetPosition = _positionGetter.Invoke();
            _startPosition = _transform.position;
        }

        protected override void OnUpdate(float deltaTime)
        {
            _passedTime += deltaTime;
            var movingPercent = _passedTime / _movingTime;

            if (movingPercent >= 1)
            {
                _transform.position = _targetPosition;
                _ended = true;
                return;
            }
            
            _transform.position = Vector3.Lerp(_startPosition, _targetPosition, movingPercent);
        }

        [Transition]
        public bool OnMoved(out Type nextState)
        {
            nextState = _nextState;
            return _ended;
        }
    }
}