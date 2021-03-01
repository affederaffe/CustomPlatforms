using System;
using System.Collections.Generic;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    internal class PlatformSpawnerMenu : PlatformSpawner, IInitializable, IDisposable
    {
        [Inject]
        private readonly GameScenesManager _gameScenesManager;

        [Inject]
        private readonly IMultiplayerSessionManager _multiplayerSessionManager;

        internal PlatformSpawnerMenu(DiContainer container)
        {
            _container = container;
        }

        public void Initialize()
        {
            _multiplayerSessionManager.connectedEvent += HandleConnected;
            _multiplayerSessionManager.disconnectedEvent += HandleDisconnected;
            _gameScenesManager.transitionDidFinishEvent += HandleCreditsCouldStart; 
            if (_config.ShowInMenu)
                ChangeToPlatform(PlatformType.Singleplayer);
        }

        public void Dispose()
        {
            _multiplayerSessionManager.connectedEvent -= HandleConnected;
            _multiplayerSessionManager.disconnectedEvent -= HandleDisconnected;
            _gameScenesManager.transitionDidFinishEvent -= HandleCreditsCouldStart;
            ChangeToPlatform(0);
        }

        private void HandleCreditsCouldStart(ScenesTransitionSetupDataSO setupData, DiContainer container)
        {
            if (setupData is CreditsScenesTransitionSetupDataSO)
            {
                _hider.HideObjectsForPlatform(_platformManager.currentSingleplayerPlatform);
                PlatformManager.Heart.SetActive(false);
                (setupData as CreditsScenesTransitionSetupDataSO).didFinishEvent -= DidFinish;
                (setupData as CreditsScenesTransitionSetupDataSO).didFinishEvent += DidFinish;
                void DidFinish(CreditsScenesTransitionSetupDataSO creditsSetupData) 
                {
                    // Just for the visuals, otherwise the heart would instantly be shown on button click
                    SharedCoroutineStarter.instance.StartCoroutine(WaitAndShowHeart());
                    IEnumerator<WaitUntil> WaitAndShowHeart()
                    {
                        yield return _gameScenesManager.waitUntilSceneTransitionFinish;
                        PlatformManager.Heart.SetActive(_config.ShowHeart);
                        PlatformManager.Heart.GetComponent<InstancedMaterialLightWithId>().ColorWasSet(Color.magenta);
                    }
                }
            }
        }

        private void HandleConnected()
        {
            ChangeToPlatform(0);
            PlatformManager.Heart.SetActive(false);
        }

        private void HandleDisconnected(DisconnectedReason reason)
        {
            PlatformManager.Heart.SetActive(_config.ShowHeart);
            PlatformManager.Heart.GetComponent<InstancedMaterialLightWithId>().ColorWasSet(Color.magenta);
            if (_config.ShowInMenu)
            {
                ChangeToPlatform(PlatformType.Singleplayer);
            }
        }
    }
}
