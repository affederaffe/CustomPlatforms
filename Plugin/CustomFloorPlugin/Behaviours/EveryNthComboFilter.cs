﻿using CustomFloorPlugin.Interfaces;
using UnityEngine;
using UnityEngine.Events;

using Zenject;


// ReSharper disable once CheckNamespace
namespace CustomFloorPlugin
{
    public class EveryNthComboFilter : MonoBehaviour, INotifyPlatformEnabled, INotifyPlatformDisabled
    {
        // ReSharper disable InconsistentNaming
        public int ComboStep = 50;
        public UnityEvent? NthComboReached;
        // ReSharper restore InconsistentNaming

        private BSEvents? _events;

        [Inject]
        public void Construct([InjectOptional] BSEvents events) => _events = events;

        public void PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
            if (_events is not null)
                _events.ComboDidChangeEvent += OnComboStep;
        }

        public void PlatformDisabled()
        {
            if (_events is not null)
                _events.ComboDidChangeEvent -= OnComboStep;
        }

        private void OnComboStep(int combo)
        {
            if (combo % ComboStep == 0 && combo == 0)
                NthComboReached!.Invoke();
        }
    }
}
