using System;

using IPA.Utilities;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    internal class MultiplayerGameManager : IInitializable, IDisposable
    {
        private readonly DiContainer _container;
        private readonly PlatformManager _platformManager;
        private readonly PlatformSpawner _platformSpawner;
        private readonly MultiplayerPlayersManager _multiplayerPlayersManager;
        private readonly ColorScheme _colorScheme;

        private GameObject? _lightEffects;
        
        public MultiplayerGameManager(DiContainer container, 
                                     PlatformManager platformManager,
                                     PlatformSpawner platformSpawner,
                                     MultiplayerPlayersManager multiplayerPlayersManager,
                                     ColorScheme colorScheme)
        {
            _container = container;
            _platformManager = platformManager;
            _platformSpawner = platformSpawner;
            _multiplayerPlayersManager = multiplayerPlayersManager;
            _colorScheme = colorScheme;
        }

        public async void Initialize()
        {
            int platformIndex = _platformManager.GetIndexForType(PlatformType.Multiplayer);
            if (platformIndex != 0)
            {
                _multiplayerPlayersManager.playerDidFinishEvent += OnPlayerDidFinish;
                await Helpers.AsyncHelper.WaitForEndOfFrameAsync();
                await _platformSpawner.SetContainerAndShowAsync(platformIndex, _container);
                _lightEffects = CreateLightEffects();
            }
        }

        public void Dispose()
        {
            _multiplayerPlayersManager.playerDidFinishEvent -= OnPlayerDidFinish;
            if (_lightEffects != null)
                UnityEngine.Object.Destroy(_lightEffects);
        }

        /// <summary>
        /// Automatically change to the default platform when the player fails or finishes for a bette view
        /// </summary>
        private async void OnPlayerDidFinish(LevelCompletionResults results)
        {
            await _platformSpawner.ChangeToPlatformAsync(0);
        }

        /// <summary>
        /// Creates <see cref="LightSwitchEventEffect"/>s, without them lighting is a little boring
        /// </summary>
        private GameObject CreateLightEffects()
        {
            GameObject lightEffects = new("LightEffects");
            lightEffects.SetActive(false);

            Color normalColor = new(1f, 1f, 1f, 0.7490196f);
            Color highlightColor = Color.white;
            Color boostColor = new(1f, 1f, 1f, 0.8f);
            MultipliedColorSO lightColor0 = CreateMultipliedColorSOForColors(_colorScheme.environmentColor0, normalColor);
            MultipliedColorSO lightColor1 = CreateMultipliedColorSOForColors(_colorScheme.environmentColor1, normalColor);
            MultipliedColorSO highlightColor0 = CreateMultipliedColorSOForColors(_colorScheme.environmentColor0, highlightColor);
            MultipliedColorSO highlightColor1 = CreateMultipliedColorSOForColors(_colorScheme.environmentColor1, highlightColor);
            MultipliedColorSO lightColor0Boost = CreateMultipliedColorSOForColors(_colorScheme.environmentColor0Boost, boostColor);
            MultipliedColorSO lightColor1Boost = CreateMultipliedColorSOForColors(_colorScheme.environmentColor1Boost, boostColor);
            MultipliedColorSO highlightColor0Boost = CreateMultipliedColorSOForColors(_colorScheme.environmentColor0Boost, highlightColor);
            MultipliedColorSO highlightColor1Boost = CreateMultipliedColorSOForColors(_colorScheme.environmentColor1Boost, highlightColor);
            
            for (int i = 0; i < 5; i++)
            {
                LightSwitchEventEffect lse = _container.InstantiateComponent<LightSwitchEventEffect>(lightEffects);
                lse.SetField("_lightsID", i+1);
                lse.SetField("_event", (BeatmapEventType)i);
                lse.SetField("_colorBoostEvent", BeatmapEventType.Event5);
                lse.SetField("_lightColor0", (ColorSO)lightColor0);
                lse.SetField("_lightColor1", (ColorSO)lightColor1);
                lse.SetField("_highlightColor0", (ColorSO)highlightColor0);
                lse.SetField("_highlightColor1", (ColorSO)highlightColor1);
                lse.SetField("_lightColor0Boost", (ColorSO)lightColor0Boost);
                lse.SetField("_lightColor1Boost", (ColorSO)lightColor1Boost);
                lse.SetField("_highlightColor0Boost", (ColorSO)highlightColor0Boost);
                lse.SetField("_highlightColor1Boost", (ColorSO)highlightColor1Boost);
            }
            lightEffects.SetActive(true);
            return lightEffects;
        }

        /// <summary>
        /// Helper function to create a <see cref="MultipliedColorSO"/>
        /// </summary>
        private MultipliedColorSO CreateMultipliedColorSOForColors(Color baseColor, Color boostColor)
        {
            SimpleColorSO simpleColor = ScriptableObject.CreateInstance<SimpleColorSO>();
            simpleColor.hideFlags &= ~HideFlags.DontUnloadUnusedAsset;
            simpleColor.SetColor(baseColor);
            MultipliedColorSO multipliedColor = ScriptableObject.CreateInstance<MultipliedColorSO>();
            multipliedColor.hideFlags &= ~HideFlags.DontUnloadUnusedAsset;
            multipliedColor.SetField("_multiplierColor", boostColor);
            multipliedColor.SetField("_baseColor", simpleColor);
            return multipliedColor;
        }
    }
}