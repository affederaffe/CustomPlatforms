using System;
using System.Linq;

using UnityEngine;


namespace CustomFloorPlugin
{
    /// <summary>
    /// The <see cref="MaterialSwapper"/>'s job is to swap the fake <see cref="Material"/>s on loaded resources after they are spawned against real ones<br/>
    /// Primary reason for this is the absence of proper custom <see cref="Shader"/>s (or decompiled source <see cref="Shader"/>s) and a lack of knowledge about their inner workings...<br/>
    /// Part of the documentation for this file is omitted because it's a clusterfuck and under construction.
    /// </summary>
    public class MaterialSwapper
    {
        private Material[]? _materials;

        public MaterialSwapper()
        {
            DarkEnvSimpleMaterial = new Lazy<Material?>(() => FindMaterial("DarkEnvironmentSimple"));
            TransparentGlowMaterial = new Lazy<Material?>(() => FindMaterialDisableHeightFog("EnvLight"));
            OpaqueGlowMaterial = new Lazy<Material?>(() => FindMaterialDisableHeightFog("EnvLightOpaque"));
        }

        internal Lazy<Material?> DarkEnvSimpleMaterial { get; }

        internal Lazy<Material?> TransparentGlowMaterial { get; }

        internal Lazy<Material?> OpaqueGlowMaterial { get; }

        private Material? FindMaterial(string name)
        {
            _materials ??= Resources.FindObjectsOfTypeAll<Material>();
            Material? material = _materials.FirstOrDefault(x => x.name == name);
            if (material is not null) return material;
            _materials = Resources.FindObjectsOfTypeAll<Material>();
            return _materials.FirstOrDefault(x => x.name == name);
        }

        private Material? FindMaterialDisableHeightFog(string name)
        {
            Material? material = FindMaterial(name);
            if (material is null) return null;
            material.DisableKeyword("ENABLE_HEIGHT_FOG");
            return material;
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
                        materialsCopy[i] = DarkEnvSimpleMaterial.Value!;
                        materialsDidChange = true;
                        break;
                    case "_transparent_glow_replace (Instance)":
                        materialsCopy[i] = TransparentGlowMaterial.Value!;
                        materialsDidChange = true;
                        break;
                    case "_glow_replace (Instance)":
                        materialsCopy[i] = OpaqueGlowMaterial.Value!;
                        materialsDidChange = true;
                        break;
                }
            }

            if (materialsDidChange)
                renderer.sharedMaterials = materialsCopy;
        }
    }
}