using IPA.Utilities;

using UnityEngine;

using Zenject;


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

        public void PlatformEnabled(DiContainer container)
        {
            if (_mirror == null)
            {
                _mirror = gameObject.AddComponent<Mirror>();
                _mirror.SetField("_renderer", GetComponent<MeshRenderer>());
                _mirror.SetField("_mirrorRenderer", MirrorRenderer);
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
            mirrorMaterial.SetTexture(NormalTexId, normalTexture);
            mirrorMaterial.SetTextureScale(NormalTexId, normalUVScale);
            mirrorMaterial.SetTextureOffset(NormalTexId, normalUVOffset);
            mirrorMaterial.SetFloat(BumpIntensityId, bumpIntensity);
            mirrorMaterial.SetColor(TintColorId, tintColor);
            if (enableDirt)
            {
                mirrorMaterial.EnableKeyword("ENABLE_DIRT");
                mirrorMaterial.SetTexture(DirtTexId, dirtTexture);
                mirrorMaterial.SetTextureScale(DirtTexId, dirtUVScale);
                mirrorMaterial.SetTextureOffset(DirtTexId, dirtUVOffset);
                mirrorMaterial.SetFloat(DirtIntensityId, dirtIntensity);
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
            if (enableDirt)
            {
                noMirrorMaterial.EnableKeyword("DIRT");
                noMirrorMaterial.EnableKeyword("ENABLE_DIRT");
                noMirrorMaterial.SetTexture(DirtTexId, dirtTexture);
                noMirrorMaterial.SetTextureScale(DirtTexId, dirtUVScale);
                noMirrorMaterial.SetTextureOffset(DirtTexId, dirtUVOffset);
                noMirrorMaterial.SetFloat(DirtIntensityId, dirtIntensity);
            }

            return noMirrorMaterial;
        }

        private static MirrorRendererSO MirrorRenderer => _mirrorRenderer ??= Resources.FindObjectsOfTypeAll<MirrorRendererSO>()[0];
        private static MirrorRendererSO? _mirrorRenderer;
        private static Shader MirrorShader => _mirrorShader ??= Shader.Find("Custom/Mirror");
        private static Shader? _mirrorShader;
        private static Shader NoMirrorShader => _noMirrorShader ??= Shader.Find("Custom/SimpleLit");
        private static Shader? _noMirrorShader;

        private static readonly int NormalTexId = Shader.PropertyToID("_NormalTex");
        private static readonly int BumpIntensityId = Shader.PropertyToID("_BumpIntensity");
        private static readonly int DirtTexId = Shader.PropertyToID("_DirtTex");
        private static readonly int DirtIntensityId = Shader.PropertyToID("_DirtIntensity");
        private static readonly int TintColorId = Shader.PropertyToID("_TintColor");
    }
}