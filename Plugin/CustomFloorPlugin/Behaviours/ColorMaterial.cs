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

        private ColorManager? _colorManager;
        private LightWithIdManager? _lightWithIdManager;

        private Renderer Renderer => _renderer ??= GetComponent<Renderer>();
        private Renderer? _renderer;

        [Inject]
        public void Construct(ColorManager colorManager, LightWithIdManager lightWithIdManager)
        {
            _colorManager = colorManager;
            _lightWithIdManager = lightWithIdManager;
        }

        public void PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
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