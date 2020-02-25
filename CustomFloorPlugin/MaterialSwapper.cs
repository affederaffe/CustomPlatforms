using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static CustomFloorPlugin.Utilities.UnityObjectSearching;

namespace CustomFloorPlugin {
    static class MaterialSwapper {
        private static readonly Material dark;
        private static readonly Material glow;
        private static readonly Material opaqueGlow;

        private const string fakeDarkMatName = "_dark_replace (Instance)";
        private const string fakeGlowMatName = "_transparent_glow_replace (Instance)";
        private const string fakeOpaqueGlowMatName = "_glow_replace (Instance)";

        private const string realDarkMatName = "DarkEnvironmentSimple";
        private const string realGlowMatName = "EnvLight";
        private const string realOpaqueGlowMatName = "EnvLightOpaque";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1810:Initialize reference type static fields inline", Justification = "Performance reasons")]
        static MaterialSwapper() {
            Material[] materials = Resources.FindObjectsOfTypeAll<Material>();

            dark = materials.First(x => x.name == realDarkMatName);
            opaqueGlow = materials.First(x => x.name == realOpaqueGlowMatName);
            glow = materials.First(x => x.name == realGlowMatName);
        }

        internal static void ReplaceMaterials(Scene scene) {
            foreach(Renderer renderer in FindAll<Renderer>(scene)) {
                ReplaceForRenderer(renderer);
            }
        }
        internal static void ReplaceMaterials(GameObject gameObject) {
            foreach(Renderer renderer in FindAll<Renderer>(gameObject)) {
                ReplaceForRenderer(renderer);
            }
        }
        private static void ReplaceForRenderer(Renderer renderer) {
            Material[] materialsCopy = renderer.materials;
            bool materialsDidChange = false;
            for(int i = 0; i < materialsCopy.Length; i++) {
                if(materialsCopy[i] != null) {
                    if(materialsCopy[i].name.Equals(fakeDarkMatName, Constants.StrInv)) {
                        materialsCopy[i] = dark;
                        materialsDidChange = true;
                    } else if(materialsCopy[i].name.Equals(fakeGlowMatName, Constants.StrInv)) {
                        materialsCopy[i] = glow;
                        materialsDidChange = true;
                    } else if(materialsCopy[i].name.Equals(fakeOpaqueGlowMatName, Constants.StrInv)) {
                        materialsCopy[i] = opaqueGlow;
                        materialsDidChange = true;
                    }
                }
            }
            if(materialsDidChange) {
                renderer.sharedMaterials = materialsCopy;
            }
        }
    }
}
