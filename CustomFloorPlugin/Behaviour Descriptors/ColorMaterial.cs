using UnityEngine;


namespace CustomFloorPlugin
{
    [RequireComponent(typeof(Renderer))]
    public class ColorMaterial : MonoBehaviour
    {
        public string propertyName;
        public MaterialColorType materialColorType;
        private Renderer _renderer;
        private ColorManager _colorManager;

        internal void SetColorManager(ColorManager colorManager)
        {
            _colorManager = colorManager;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void Start()
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
            else
                _renderer.material.color = color;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void OnEnable()
        {
            if (_colorManager != null && _renderer != null)
                ChangeColors();
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
