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

        internal PlatformSpawnerMenu(DiContainer container)
        {
            _container = container;
        }

        public void Initialize()
        {
            SharedCoroutineStarter.instance.StartCoroutine(WaitForSceneTransitionFinish());
            IEnumerator<WaitUntil> WaitForSceneTransitionFinish()
            {
                yield return _gameScenesManager.waitUntilSceneTransitionFinish;
                _platformManager.allPlatforms = _platformLoader.CreateAllDescriptors(_platformManager.transform);
                _platformManager.GetLastSelectedPlatforms();
                if (_config.ShowInMenu)
                {
                    ChangeToPlatform(PlatformType.Singleplayer);
                }
            }
        }

        public void Dispose()
        {
            ChangeToPlatform(0);
        }
    }
}
