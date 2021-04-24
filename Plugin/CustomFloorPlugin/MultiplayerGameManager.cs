using System;
using System.Collections.Generic;

using IPA.Utilities.Async;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    internal class MultiplayerGameManager : IInitializable, IDisposable
    {
        private readonly PlatformManager _platformManager;
        private readonly PlatformSpawner _platformSpawner;
        private readonly MultiplayerPlayersManager _multiplayerPlayersManager;
        private readonly DiContainer _container;

        //private GameObject? _lightEffects;
        
        public MultiplayerGameManager(PlatformManager platformManager,
                                     PlatformSpawner platformSpawner,
                                     MultiplayerPlayersManager multiplayerPlayersManager,
                                     DiContainer container)
        {
            _platformManager = platformManager;
            _platformSpawner = platformSpawner;
            _multiplayerPlayersManager = multiplayerPlayersManager;
            _container = container;
        }

        public async void Initialize()
        {
            int platformIndex = _platformManager.GetIndexForType(PlatformType.Multiplayer);
            if (platformIndex != 0)
            {
                _multiplayerPlayersManager.playerDidFinishEvent += OnPlayerDidFinish;
                await Coroutines.AsTask(WaitForEndOfFrameCoroutine());
                static IEnumerator<WaitForEndOfFrame> WaitForEndOfFrameCoroutine() { yield return new WaitForEndOfFrame(); }
                await _platformSpawner.SetContainerAndShowAsync(platformIndex, _container);
                //_lightEffects = SpawnLightEffects();
            }
        }

        public void Dispose()
        {
            _multiplayerPlayersManager.playerDidFinishEvent -= OnPlayerDidFinish;
            /*if (_lightEffects != null)
                UnityEngine.Object.Destroy(_lightEffects);*/
        }

        private async void OnPlayerDidFinish(LevelCompletionResults results)
        {
            await _platformSpawner.ChangeToPlatformAsync(0);
        }

        /*/// <summary>
        /// Creates <see cref="LightSwitchEventEffect"/>s
        /// </summary>
        private GameObject SpawnLightEffects()
        {
            GameObject lightEffects = new("LightEffects");
            for (int i = 0; i < 5; i++)
            {
                LightSwitchEventEffect lse = _container.InstantiateComponent<LightSwitchEventEffect>(lightEffects);
                lse.SetField("_lightsID", i+1);
                lse.SetField("_event", (BeatmapEventType)i);
                lse.SetField("_colorBoostEvent", BeatmapEventType.Event5);
                lse.SetField("_lightColor0", );
                lse.SetField("_lightColor1");
                lse.SetField("_highlightColor0");
                lse.SetField("_highlightColor1");
                lse.SetField("_lightColor0Boost");
                lse.SetField("_lightColor1Boost");
                lse.SetField("_highlightColor0Boost);
                lse.SetField("_highlightColor1Boost");
            }
            return lightEffects;
        }*/
    }
}