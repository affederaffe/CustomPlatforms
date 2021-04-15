using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    [RequireComponent(typeof(Renderer))]
    public class ColorMaterial : MonoBehaviour, INotifyPlatformEnabled
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

        private Renderer Renderer => _renderer ??= GetComponent<Renderer>();
        private Renderer? _renderer;

        [Inject]
        public void Construct(ColorManager colorManager)
        {
            _colorManager = colorManager;
        }

        void INotifyPlatformEnabled.PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
            ChangeColors();
        }

        private void ChangeColors()
        {
            if (!Renderer.material.HasProperty(propertyName)) return;
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