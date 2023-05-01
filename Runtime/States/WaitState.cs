using System;

namespace Hermer29.Almasury.States
{
    public class WaitState : State
    {
        private readonly TimeSpan _time;
        private readonly Type _nextState;

        private float _passedTime;
        private bool _ended;

        public WaitState(TimeSpan time, Type nextState)
        {
            _time = time;
            _nextState = nextState;
        }

        public WaitState(float seconds, Type nextState) : this(TimeSpan.FromSeconds(seconds), nextState)
        {
        }

        protected override void OnUpdate(float deltaTime)
        {
            _passedTime += deltaTime;

            if (_passedTime >= _time.TotalSeconds)
            {
                _ended = true;
            }
        }

        protected override void OnExit()
        {
            _passedTime = 0;
            _ended = false;
        }

        [Transition]
        public bool TimeIsUp(out Type nextState)
        {
            nextState = _nextState;
            return _ended;
        }
    }
}