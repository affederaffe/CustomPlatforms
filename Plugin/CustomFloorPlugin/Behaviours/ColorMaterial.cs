using CustomFloorPlugin.Interfaces;
using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    [RequireComponent(typeof(Renderer))]
    public class ColorMaterial : MonoBehaviour, INotifyPlatformEnabled, INotifyPlatformDisabled
    {
        public enum MaterialColorType
        {
            SaberColorA,
            SaberColorB,
            ColorTypeA,
            ColorTypeB,
            ObstacleColor
        }

        public string propertyName = "_Color";
        public MaterialColorType materialColorType;

        private LightWithIdManager? _lightWithIdManager;
        private ColorManager? _colorManager;

        private Renderer Renderer => _renderer ??= GetComponent<Renderer>();
        private Renderer? _renderer;

        [Inject]
        public void Construct(LightWithIdManager lightWithIdManager, [InjectOptional] ColorManager colorManager)
        {
            _lightWithIdManager = lightWithIdManager;
            _colorManager = colorManager;
        }

        public void PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
            if (_colorManager is null) return;
            _lightWithIdManager!.didChangeSomeColorsThisFrameEvent += OnColorsDidChange;
        }

        public void PlatformDisabled()
        {
            _lightWithIdManager!.didChangeSomeColorsThisFrameEvent -= OnColorsDidChange;
        }

        private void OnColorsDidChange()
        {
            Renderer.material.SetColor(propertyName, materialColorType switch
            {
                MaterialColorType.SaberColorA => _colorManager!.ColorForSaberType(SaberType.SaberA),
                MaterialColorType.SaberColorB => _colorManager!.ColorForSaberType(SaberType.SaberB),
                MaterialColorType.ColorTypeA => _colorManager!.ColorForType(ColorType.ColorA),
                MaterialColorType.ColorTypeB => _colorManager!.ColorForType(ColorType.ColorB),
                MaterialColorType.ObstacleColor => _colorManager!.GetObstacleEffectColor(),
                _ => Color.white,
            });
        }
    }
}