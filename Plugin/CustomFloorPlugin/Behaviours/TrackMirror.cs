using IPA.Utilities;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    [RequireComponent(typeof(MeshRenderer))]
    public class TrackMirror : MonoBehaviour, INotifyPlatformEnabled
    {
        public Texture? normalTexture;
        public float bumpIntensity;
        public bool enableDirt;
        public Texture? dirtTexture;
        public float dirtIntensity;
        public Color tintColor = Color.white;

        private Mirror? _mirror;

        public void PlatformEnabled(DiContainer container)
        {
            if (_mirror == null)
            {
                _mirror = gameObject.AddComponent<Mirror>();
                _mirror.SetField("_renderer", GetComponent<MeshRenderer>());
                _mirror.SetField("_mirrorRenderer", Instantiate(MirrorRenderer));
                _mirror.SetField("_mirrorMaterial", CreateMirrorMaterial());
                _mirror.SetField("_noMirrorMaterial", CreateNoMirrorMaterial());
            }
        }

        private Material CreateMirrorMaterial()
        {
            Material mirrorMaterial = new(MirrorShader);
            mirrorMaterial.EnableKeyword("ENABLE_MIRROR");
            mirrorMaterial.EnableKeyword("ETC1_EXTERNAL_ALPHA");
            mirrorMaterial.EnableKeyword("_EMISSION");
            mirrorMaterial.SetTexture(normalTexId, normalTexture);
            mirrorMaterial.SetFloat(bumpIntensityId, bumpIntensity);
            mirrorMaterial.SetColor(tintColorId, tintColor);
            if (enableDirt)
            {
                mirrorMaterial.EnableKeyword("ENABLE_DIRT");
                mirrorMaterial.SetTexture(dirtTexId, dirtTexture);
                mirrorMaterial.SetFloat(dirtIntensityId, dirtIntensity);
            }
            return mirrorMaterial;
        }

        private Material CreateNoMirrorMaterial()
        {
            Material noMirrorMaterial = new(NoMirrorShader);
            noMirrorMaterial.EnableKeyword("DIFFUSE");
            noMirrorMaterial.EnableKeyword("ENABLE_DIFFUSE");
            noMirrorMaterial.EnableKeyword("ENABLE_FOG");
            noMirrorMaterial.EnableKeyword("ENABLE_SPECULAR");
            noMirrorMaterial.EnableKeyword("FOG");
            noMirrorMaterial.EnableKeyword("SPECULAR");
            noMirrorMaterial.EnableKeyword("_EMISSION");
            noMirrorMaterial.EnableKeyword("_ENABLE_FOG_TINT");
            noMirrorMaterial.EnableKeyword("_RIMLIGHT_NONE");
            noMirrorMaterial.SetColor(colorId, new Color(0.25f, 0.25f, 0.25f, 0f));
            if (enableDirt)
            {
                noMirrorMaterial.EnableKeyword("DIRT");
                noMirrorMaterial.EnableKeyword("ENABLE_DIRT");
                noMirrorMaterial.SetTexture(dirtTexId, dirtTexture);
                noMirrorMaterial.SetFloat(dirtIntensityId, dirtIntensity);
            }
            return noMirrorMaterial;
        }

        private static MirrorRendererSO MirrorRenderer => _mirrorRenderer ??= Resources.FindObjectsOfTypeAll<MirrorRendererSO>()[0];
        private static MirrorRendererSO? _mirrorRenderer;
        private static Shader MirrorShader => _mirrorShader ??= Shader.Find("Custom/Mirror");
        private static Shader? _mirrorShader;
        private static Shader NoMirrorShader => _noMirrorShader ??= Shader.Find("Custom/SimpleLit");
        private static Shader? _noMirrorShader;
        
        private static readonly int normalTexId = Shader.PropertyToID("_NormalTex");
        private static readonly int bumpIntensityId = Shader.PropertyToID("_BumpIntensity");
        private static readonly int dirtTexId = Shader.PropertyToID("_DirtTex");
        private static readonly int dirtIntensityId = Shader.PropertyToID("_DirtIntensity");
        private static readonly int tintColorId = Shader.PropertyToID("_TintColor");
        private static readonly int colorId = Shader.PropertyToID("_Color");
    }
}