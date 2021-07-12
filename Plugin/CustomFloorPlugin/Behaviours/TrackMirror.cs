using CustomFloorPlugin.Interfaces;

using IPA.Utilities;

using UnityEngine;

using Zenject;


// ReSharper disable once CheckNamespace
namespace CustomFloorPlugin
{
    [RequireComponent(typeof(MeshRenderer))]
    public class TrackMirror : MonoBehaviour, INotifyPlatformEnabled
    {
        public Texture? normalTexture;
        public Vector2 normalUVScale = Vector2.one;
        public Vector2 normalUVOffset = Vector2.one;
        public float bumpIntensity;
        public bool enableDirt;
        public Texture? dirtTexture;
        public Vector2 dirtUVScale = Vector2.one;
        public Vector2 dirtUVOffset = Vector2.one;
        public float dirtIntensity;
        public Color tintColor = Color.white;

        private Mirror? _mirror;
        private MirrorRendererSO? _mirrorRenderer;

        [Inject]
        public void Construct(MirrorRendererSO mirrorRenderer)
        {
            _mirrorRenderer = mirrorRenderer;
        }

        public void PlatformEnabled(DiContainer container)
        {
            if (_mirror is not null) return;
            container.Inject(this);
            _mirror = gameObject.AddComponent<Mirror>();
            _mirror.SetField("_renderer", GetComponent<MeshRenderer>());
            _mirror.SetField("_mirrorRenderer", _mirrorRenderer);
            _mirror.SetField("_mirrorMaterial", CreateMirrorMaterial());
            _mirror.SetField("_noMirrorMaterial", CreateNoMirrorMaterial());
        }

        private Material CreateMirrorMaterial()
        {
            Shader mirrorShader = Shader.Find("Custom/Mirror");
            Material mirrorMaterial = new(mirrorShader);
            mirrorMaterial.EnableKeyword("ENABLE_MIRROR");
            mirrorMaterial.EnableKeyword("ETC1_EXTERNAL_ALPHA");
            mirrorMaterial.EnableKeyword("_EMISSION");
            mirrorMaterial.SetTexture(_normalTexId, normalTexture);
            mirrorMaterial.SetTextureScale(_normalTexId, normalUVScale);
            mirrorMaterial.SetTextureOffset(_normalTexId, normalUVOffset);
            mirrorMaterial.SetFloat(_bumpIntensityId, bumpIntensity);
            mirrorMaterial.SetColor(_tintColorId, tintColor);
            if (!enableDirt) return mirrorMaterial;
            mirrorMaterial.EnableKeyword("ENABLE_DIRT");
            mirrorMaterial.SetTexture(_dirtTexId, dirtTexture);
            mirrorMaterial.SetTextureScale(_dirtTexId, dirtUVScale);
            mirrorMaterial.SetTextureOffset(_dirtTexId, dirtUVOffset);
            mirrorMaterial.SetFloat(_dirtIntensityId, dirtIntensity);
            return mirrorMaterial;
        }

        private Material CreateNoMirrorMaterial()
        {
            Shader noMirrorShader = Shader.Find("Custom/SimpleLit");
            Material noMirrorMaterial = new(noMirrorShader);
            noMirrorMaterial.EnableKeyword("DIFFUSE");
            noMirrorMaterial.EnableKeyword("ENABLE_DIFFUSE");
            noMirrorMaterial.EnableKeyword("ENABLE_FOG");
            noMirrorMaterial.EnableKeyword("ENABLE_SPECULAR");
            noMirrorMaterial.EnableKeyword("FOG");
            noMirrorMaterial.EnableKeyword("NOISE_DITHERING");
            noMirrorMaterial.EnableKeyword("REFLECTION_PROBE");
            noMirrorMaterial.EnableKeyword("REFLECTION_PROBE_BOX_PROJECTION");
            noMirrorMaterial.EnableKeyword("_EMISSION");
            noMirrorMaterial.EnableKeyword("_ENABLE_FOG_TINT");
            noMirrorMaterial.EnableKeyword("_RIMLIGHT_NONE");
            noMirrorMaterial.color = new Color(0.15f, 0.15f, 0.15f, 0f);
            if (!enableDirt) return noMirrorMaterial;
            noMirrorMaterial.EnableKeyword("DIRT");
            noMirrorMaterial.EnableKeyword("ENABLE_DIRT");
            noMirrorMaterial.SetTexture(_dirtTexId, dirtTexture);
            noMirrorMaterial.SetTextureScale(_dirtTexId, dirtUVScale);
            noMirrorMaterial.SetTextureOffset(_dirtTexId, dirtUVOffset);
            noMirrorMaterial.SetFloat(_dirtIntensityId, dirtIntensity);
            return noMirrorMaterial;
        }

        private static readonly int _normalTexId = Shader.PropertyToID("_NormalTex");
        private static readonly int _bumpIntensityId = Shader.PropertyToID("_BumpIntensity");
        private static readonly int _dirtTexId = Shader.PropertyToID("_DirtTex");
        private static readonly int _dirtIntensityId = Shader.PropertyToID("_DirtIntensity");
        private static readonly int _tintColorId = Shader.PropertyToID("_TintColor");
    }
}