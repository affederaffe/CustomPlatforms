using System.Linq;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    /// <summary>
    /// The <see cref="MaterialSwapper"/>'s job is to swap the fake <see cref="Material"/>s on loaded resources after they are spawned against real ones<br/>
    /// Primary reason for this is the absence of proper custom <see cref="Shader"/>s (or decompiled source <see cref="Shader"/>s) and a lack of knowledge about their inner workings...<br/>
    /// Part of the documentation for this file is omited because it's a clusterfuck and under construction.
    /// </summary>
    internal class MaterialSwapper : IInitializable
    {
        [Inject]
        private readonly ColorManager _colorManager;

        private Material dark;
        private Material glow;
        private Material opaqueGlow;

        private const string fakeDarkMatName = "_dark_replace (Instance)";
        private const string fakeGlowMatName = "_transparent_glow_replace (Instance)";
        private const string fakeOpaqueGlowMatName = "_glow_replace (Instance)";

        private const string realDarkMatName = "DarkEnvironmentSimple";
        private const string realGlowMatName = "EnvLight";
        private const string realOpaqueGlowMatName = "EnvLightOpaque";

        /// <summary>
        /// Automatically initializes needed variables
        /// </summary>
        public void Initialize()
        {
            Material[] materials = Resources.FindObjectsOfTypeAll<Material>();
            dark = materials.First(x => x.name == realDarkMatName);
            opaqueGlow = materials.First(x => x.name == realOpaqueGlowMatName);
            glow = materials.First(x => x.name == realGlowMatName);
        }

        /// <summary>
        /// Replaces all fake <see cref="Material"/>s on all <see cref="Renderer"/>s under a given <see cref="GameObject"/>
        /// </summary>
        /// <param name="gameObject"><see cref="GameObject"/> to search for <see cref="Renderer"/>s</param>
        internal void ReplaceMaterials(GameObject gameObject)
        {
            Renderer[] renderers = gameObject.GetComponents<Renderer>().Concat(gameObject.GetComponentsInChildren<Renderer>(true)).ToArray();
            foreach (Renderer renderer in renderers)
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
                    if (materialsCopy[i].name.Equals(fakeDarkMatName))
                    {
                        materialsCopy[i] = dark;
                        materialsDidChange = true;
                    }
                    else if (materialsCopy[i].name.Equals(fakeGlowMatName))
                    {
                        materialsCopy[i] = glow;
                        materialsDidChange = true;
                    }
                    else if (materialsCopy[i].name.Equals(fakeOpaqueGlowMatName))
                    {
                        materialsCopy[i] = opaqueGlow;
                        materialsDidChange = true;
                    }
                    // If the shader has a float named _UseLeftColor or _UseRightColor, swap them out with the proper ColorManger colors 
                    if (materialsCopy[i].HasProperty("_UseLeftColor") && materialsCopy[i].GetFloat("_UseLeftColor") != 0)
                    {
                        materialsCopy[i].SetColor("_Color", _colorManager.ColorForSaberType(SaberType.SaberA));
                    }
                    else if (materialsCopy[i].HasProperty("_UseRightColor") && materialsCopy[i].GetFloat("_UseRightColor") != 0)
                    {
                        materialsCopy[i].SetColor("_Color", _colorManager.ColorForSaberType(SaberType.SaberB));
                    }
                    // If the shader has a color named _LeftPlatformColor or _RightPlatformColor, swap them out with the proper ColorManger colors 
                    if (materialsCopy[i].HasProperty("_LeftPlatformColor"))
                    {
                        materialsCopy[i].SetColor("_LeftPlatformColor", _colorManager.ColorForSaberType(SaberType.SaberA));
                    }
                    else if (materialsCopy[i].HasProperty("_RightPlatformColor"))
                    {
                        materialsCopy[i].SetColor("_RightPlatformColor", _colorManager.ColorForSaberType(SaberType.SaberB));
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