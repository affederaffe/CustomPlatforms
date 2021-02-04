using System.Linq;

using UnityEngine;


namespace CustomFloorPlugin {


    /// <summary>
    /// The <see cref="MaterialSwapper"/>'s job is to swap the fake <see cref="Material"/>s on loaded resources after they are spawned against real ones<br/>
    /// Primary reason for this is the absence of proper custom <see cref="Shader"/>s (or decompiled source <see cref="Shader"/>s) and a lack of knowledge about their inner workings...<br/>
    /// Part of the documentation for this file is omited because it's a clusterfuck and under construction.
    /// </summary>
    internal static class MaterialSwapper {

        private static readonly Material dark;
        private static readonly Material glow;
        private static readonly Material opaqueGlow;

        private const string fakeDarkMatName = "_dark_replace (Instance)";
        private const string fakeGlowMatName = "_transparent_glow_replace (Instance)";
        private const string fakeOpaqueGlowMatName = "_glow_replace (Instance)";

        private const string realDarkMatName = "DarkEnvironmentSimple";
        private const string realGlowMatName = "EnvLight";
        private const string realOpaqueGlowMatName = "EnvLightOpaque";


        /// <summary>
        /// Automatically initializes needed variables
        /// </summary>
        static MaterialSwapper() {
            Material[] materials = Resources.FindObjectsOfTypeAll<Material>();

            dark = materials.First(x => x.name == realDarkMatName);
            opaqueGlow = materials.First(x => x.name == realOpaqueGlowMatName);
            glow = materials.First(x => x.name == realGlowMatName);
        }

        /// <summary>
        /// Replaces all fake <see cref="Material"/>s on all <see cref="Renderer"/>s under a given <see cref="GameObject"/>
        /// </summary>
        /// <param name="gameObject"><see cref="GameObject"/> to search for <see cref="Renderer"/>s</param>
        internal static void ReplaceMaterials(GameObject gameObject) {
            Renderer[] renderers = gameObject.GetComponents<Renderer>().Concat(gameObject.GetComponentsInChildren<Renderer>(true)).ToArray();
            foreach (Renderer renderer in renderers) {
                ReplaceForRenderer(renderer);
            }
        }


        /// <summary>
        /// Replaces all fake <see cref="Material"/>s on a given <see cref="Renderer"/>
        /// </summary>
        /// <param name="renderer">What <see cref="Renderer"/> to replace materials for</param>
        /// <param name="colorManager">What <see cref="ColorManager"/> to use when coloring</param>
        private static void ReplaceForRenderer(Renderer renderer) {
            Material[] materialsCopy = renderer.materials;
            bool materialsDidChange = false;
            for (int i = 0; i < materialsCopy.Length; i++) {
                if (materialsCopy[i] != null) {
                    if (materialsCopy[i].name.Equals(fakeDarkMatName)) {
                        materialsCopy[i] = dark;
                        materialsDidChange = true;
                    }
                    else if (materialsCopy[i].name.Equals(fakeGlowMatName)) {
                        materialsCopy[i] = glow;
                        materialsDidChange = true;
                    }
                    else if (materialsCopy[i].name.Equals(fakeOpaqueGlowMatName)) {
                        materialsCopy[i] = opaqueGlow;
                        materialsDidChange = true;
                    }
                }
            }
            if (materialsDidChange) {
                renderer.sharedMaterials = materialsCopy;
            }
        }
    }
}