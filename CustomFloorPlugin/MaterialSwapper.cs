using System.Linq;

using UnityEngine;


namespace CustomFloorPlugin
{
    /// <summary>
    /// The <see cref="MaterialSwapper"/>'s job is to swap the fake <see cref="Material"/>s on loaded resources after they are spawned against real ones<br/>
    /// Primary reason for this is the absence of proper custom <see cref="Shader"/>s (or decompiled source <see cref="Shader"/>s) and a lack of knowledge about their inner workings...<br/>
    /// Part of the documentation for this file is omited because it's a clusterfuck and under construction.
    /// </summary>
    public class MaterialSwapper
    {
        private readonly AssetLoader _assetLoader;

        private Material _dark;
        private Material _glow;
        private Material _opaqueGlow;

        private const string kFakeDarkMatName = "_dark_replace (Instance)";
        private const string kFakeGlowMatName = "_transparent_glow_replace (Instance)";
        private const string kFakeOpaqueGlowMatName = "_glow_replace (Instance)";

        private const string kRealDarkMatName = "DarkEnvironmentSimple";
        private const string kRealGlowMatName = "EnvLight";
        private const string kRealOpaqueGlowMatName = "EnvLightOpaque";

        private bool isInitialized;

        public MaterialSwapper(AssetLoader assetLoader)
        {
            _assetLoader = assetLoader;
        }

        /// <summary>
        /// Initializes needed variables<br/>
        /// </summary>
        private void InitIfNeeded()
        {
            if (!isInitialized)
            {
                isInitialized = true;
                _dark = _assetLoader.AllMaterials.First(x => x.name == kRealDarkMatName);
                _glow = _assetLoader.AllMaterials.First(x => x.name == kRealGlowMatName);
                _opaqueGlow = _assetLoader.AllMaterials.First(x => x.name == kRealOpaqueGlowMatName);
            }
        }

        /// <summary>
        /// Replaces all fake <see cref="Material"/>s on all <see cref="Renderer"/>s under a given <see cref="GameObject"/>
        /// </summary>
        /// <param name="gameObject"><see cref="GameObject"/> to search for <see cref="Renderer"/>s</param>
        internal void ReplaceMaterials(GameObject gameObject)
        {
            InitIfNeeded();
            foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>(true))
            {
                ReplaceForRenderer(renderer);
            }
        }

        /// <summary>
        /// Replaces all fake <see cref="Material"/>s on a given <see cref="Renderer"/>
        /// </summary>
        /// <param name="renderer">What <see cref="Renderer"/> to replace materials for</param>
        private void ReplaceForRenderer(Renderer renderer)
        {
            Material[] materialsCopy = renderer.materials;
            bool materialsDidChange = false;
            for (int i = 0; i < materialsCopy.Length; i++)
            {
                if (materialsCopy[i] != null)
                {
                    if (materialsCopy[i].name == kFakeDarkMatName)
                    {
                        materialsCopy[i] = _dark;
                        materialsDidChange = true;
                    }
                    else if (materialsCopy[i].name == kFakeGlowMatName)
                    {
                        materialsCopy[i] = _glow;
                        materialsDidChange = true;
                    }
                    else if (materialsCopy[i].name == kFakeOpaqueGlowMatName)
                    {
                        materialsCopy[i] = _opaqueGlow;
                        materialsDidChange = true;
                    }
                }
            }
            if (materialsDidChange)
            {
                renderer.sharedMaterials = materialsCopy;
            }
        }
    }
}