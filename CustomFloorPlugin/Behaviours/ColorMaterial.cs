using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    [RequireComponent(typeof(Renderer))]
    public class ColorMaterial : MonoBehaviour, INotifyPlatformEnabled
    {
        public string propertyName = "_Color";
        public MaterialColorType materialColorType;
        private Renderer _renderer;
        private ColorManager _colorManager;

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
        }

        private void ChangeColors()
        {
            Color color = materialColorType switch
            {
                MaterialColorType.SaberColorA => _colorManager.ColorForSaberType(SaberType.SaberA),
                MaterialColorType.SaberColorB => _colorManager.ColorForSaberType(SaberType.SaberB),
                MaterialColorType.ColorTypeA => _colorManager.ColorForType(ColorType.ColorA),
                MaterialColorType.ColorTypeB => _colorManager.ColorForType(ColorType.ColorB),
                MaterialColorType.ObstacleColor => _colorManager.GetObstacleEffectColor(),
                _ => new Color(0f, 0f, 0f),
            };
            if (_renderer.material.HasProperty(propertyName))
                _renderer.material.SetColor(propertyName, color);
        }

        public enum MaterialColorType
        {
            SaberColorA,
            SaberColorB,
            ColorTypeA,
            ColorTypeB,
            ObstacleColor,
        }
    }
}
