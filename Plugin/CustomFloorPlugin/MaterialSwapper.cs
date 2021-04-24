using System.Linq;
using System.Threading.Tasks;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    /// <summary>
    /// The <see cref="MaterialSwapper"/>'s job is to swap the fake <see cref="Material"/>s on loaded resources after they are spawned against real ones<br/>
    /// Primary reason for this is the absence of proper custom <see cref="Shader"/>s (or decompiled source <see cref="Shader"/>s) and a lack of knowledge about their inner workings...<br/>
    /// Part of the documentation for this file is omitted because it's a clusterfuck and under construction.
    /// </summary>
    public class MaterialSwapper
    {
        private readonly GameScenesManager _gameScenesManager;
        private readonly TaskCompletionSource<(Material, Material, Material)> _taskSource;

        internal readonly Task<(Material DarkEnvSimpleMaterial, Material TransparentGlowMaterial, Material OpaqueGlowMaterial)> LoadMaterialsTask;

        public MaterialSwapper(GameScenesManager gameScenesManager)
        {
            _gameScenesManager = gameScenesManager;
            _gameScenesManager.installEarlyBindingsEvent += OnInstallEarlyBindings;
            _taskSource = new();
            LoadMaterialsTask = _taskSource.Task;
        }

        private void OnInstallEarlyBindings(ScenesTransitionSetupDataSO setupData, DiContainer container)
        {
            _gameScenesManager.installEarlyBindingsEvent -= OnInstallEarlyBindings;
            Material[] materials = Resources.FindObjectsOfTypeAll<Material>();
            Material darkEnvSimpleMaterial = materials.First(x => x.name == "DarkEnvironmentSimple");
            Material transparentGlowMaterial = materials.First(x => x.name == "EnvLight");
            Material opaqueGlowMaterial = materials.First(x => x.name == "EnvLightOpaque");
            opaqueGlowMaterial = new Material(opaqueGlowMaterial);
            opaqueGlowMaterial.DisableKeyword("ENABLE_HEIGHT_FOG");
            _taskSource.TrySetResult((darkEnvSimpleMaterial, transparentGlowMaterial, opaqueGlowMaterial));
        }

        /// <summary>
        /// Replaces all fake <see cref="Material"/>s on all <see cref="Renderer"/>s under a given <see cref="GameObject"/>
        /// </summary>
        /// <param name="gameObject"><see cref="GameObject"/> to search for <see cref="Renderer"/>s</param>
        internal void ReplaceMaterials(GameObject gameObject)
        {
            foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>(true))
            {
                ReplaceForRenderer(renderer);
            }
        }

        /// <summary>
        /// Replaces all fake <see cref="Material"/>s on a given <see cref="Renderer"/>
        /// </summary>
        /// <param name="renderer">What <see cref="Renderer"/> to replace materials for</param>
        private async void ReplaceForRenderer(Renderer renderer)
        {
            await LoadMaterialsTask;
            Material[] materialsCopy = renderer.materials;
            bool materialsDidChange = false;
            for (int i = 0; i < materialsCopy.Length; i++)
            {
                if (materialsCopy[i] != null)
                {
                    switch (materialsCopy[i].name)
                    {
                        case "_dark_replace (Instance)":
                            materialsCopy[i] = LoadMaterialsTask.Result.DarkEnvSimpleMaterial;
                            materialsDidChange = true;
                            break;
                        case "_transparent_glow_replace (Instance)":
                            materialsCopy[i] = LoadMaterialsTask.Result.TransparentGlowMaterial;
                            materialsDidChange = true;
                            break;
                        case "_glow_replace (Instance)":
                            materialsCopy[i] = LoadMaterialsTask.Result.OpaqueGlowMaterial;
                            materialsDidChange = true;
                            break;
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