using UnityEngine;

namespace StateMachines.Runtime
{
    public abstract class StateMachineRuntime : MonoBehaviour
    {
        private StateMachine _machine;
        
        private void Awake()
        {
            _machine = new StateMachine(SpecifyStates());
        }

        private void OnEnable()
        {
            _machine.OnEnable();
        }

        private void OnDisable()
        {
            _machine.OnDisable();
        }

        private void Update()
        {
            _machine.Update(Time.deltaTime);
        }

        protected abstract State[] SpecifyStates();
    }
}