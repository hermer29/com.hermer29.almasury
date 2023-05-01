using UnityEngine;

namespace Hermer29.Almasury
{
    public abstract class StateMachineInstaller : MonoBehaviour
    {
        private StateMachine _stateMachine;
        protected DiContainer Container { get; private set; }

        private void Awake()
        {
            Container = new DiContainer();
            Install();
            _stateMachine = new StateMachine(Container);
            _stateMachine.OnEnable();
        }

        private void OnDisable()
        {
            _stateMachine?.OnDisable();
        }

        private void Update()
        {
            _stateMachine?.Update(Time.deltaTime);
        }

        protected abstract void Install();
    }
}