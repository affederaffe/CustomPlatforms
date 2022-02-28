using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using SiraUtil.Zenject;

using UnityEngine;


namespace CustomFloorPlugin
{
    /// <summary>
    /// The <see cref="MaterialSwapper"/>'s job is to swap the fake <see cref="Material"/>s on loaded resources after they are spawned against real ones<br/>
    /// Primary reason for this is the absence of proper custom <see cref="Shader"/>s (or decompiled source <see cref="Shader"/>s) and a lack of knowledge about their inner workings...<br/>
    /// Part of the documentation for this file is omitted because it's a clusterfuck and under construction.
    /// </summary>
    public class MaterialSwapper : IAsyncInitializable
    {
        internal Material? DarkEnvSimpleMaterial { get; private set; }
        internal Material? TransparentGlowMaterial { get; private set; }
        internal Material? OpaqueGlowMaterial { get; private set; }

        public async Task InitializeAsync(CancellationToken token)
        {
            while (DarkEnvSimpleMaterial is null || TransparentGlowMaterial is null || OpaqueGlowMaterial is null)
            {
                await Task.Yield();
                Material[] materials = Resources.FindObjectsOfTypeAll<Material>();
                DarkEnvSimpleMaterial ??= materials.FirstOrDefault(static x => x.name == "DarkEnvironmentSimple");
                TransparentGlowMaterial ??= materials.FirstOrDefault(static x => x.name == "EnvLight");
                OpaqueGlowMaterial ??= materials.FirstOrDefault(static x => x.name == "EnvLightOpaque");
            }

            OpaqueGlowMaterial.DisableKeyword("ENABLE_HEIGHT_FOG");
            TransparentGlowMaterial.DisableKeyword("ENABLE_HEIGHT_FOG");
        }

        /// <summary>
        /// Replaces all fake <see cref="Material"/>s on all <see cref="Renderer"/>s under a given <see cref="GameObject"/>
        /// </summary>
        /// <param name="gameObject"><see cref="GameObject"/> to search for <see cref="Renderer"/>s</param>
        public void ReplaceMaterials(GameObject gameObject)
        {
            foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>(true))
                ReplaceForRenderer(renderer);
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
                switch (materialsCopy[i].name)
                {
                    case "_dark_replace (Instance)":
                        materialsCopy[i] = DarkEnvSimpleMaterial!;
                        materialsDidChange = true;
                        break;
                    case "_transparent_glow_replace (Instance)":
                        materialsCopy[i] = TransparentGlowMaterial!;
                        materialsDidChange = true;
                        break;
                    case "_glow_replace (Instance)":
                        materialsCopy[i] = OpaqueGlowMaterial!;
                        materialsDidChange = true;
                        break;
                }
            }

            if (materialsDidChange)
                renderer.sharedMaterials = materialsCopy;
        }
    }
}