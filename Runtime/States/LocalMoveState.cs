using System;
using UnityEngine;

namespace Hermer29.Almasury.States
{
    public class LocalMoveState : State
    {
        private readonly Transform _transform;
        private readonly Vector3 _direction;
        private readonly float _movingTime;
        private readonly Type _nextState;
        
        private Vector3 _targetPosition;
        private float _passedTime;
        private Vector3 _startPosition;
        private bool _ended;

        public LocalMoveState(Transform transform, Vector3 direction, float movingTime, Type nextState)
        {
            _transform = transform;
            _direction = direction;
            _movingTime = movingTime;
            _nextState = nextState;
        }

        protected override void OnEnter()
        {
            _ended = false;
            _passedTime = 0;
            _targetPosition = _transform.position + _direction;
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