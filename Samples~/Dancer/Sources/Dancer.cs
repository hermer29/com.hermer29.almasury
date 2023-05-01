using System;
using Hermer29.Almasury.States;
using UnityEngine;

namespace Hermer29.Almasury.Samples
{
    public class Dancer : StateMachineInstaller
    {
        [SerializeField] private Transform _dancer;
        [SerializeField] private float _timeOffset;
        [SerializeField] private float _radius = 2;
        [SerializeField] private float _waitingTime = .5f;
        [SerializeField] private bool _inverse;

        protected override void Install()
        {
            Container.EnableLogging();
            Container.Add(
                    new WorldMoveState(transform, GetNextPoint, 1, typeof(WaitState)))
                .AsFirstState();
            Container.Add(
                new WaitState(_waitingTime, typeof(WorldMoveState)));
        }

        private Vector3 GetNextPoint()
        {
            return new Vector3(
                x: Mathf.Sin(GetTime()), 
                y: Mathf.Cos(GetTime())) * (_radius);
        }

        private float GetTime()
        {
            return Time.time + _timeOffset;
        }

        private int GetInverseModifier()
        {
            return _inverse ? -1 : 1;
        }
    }
}