using System.Collections;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using Zenject;

namespace CustomFloorPlugin
{
    internal class GameShaders(ICoroutineStarter coroutineStarter) : IInitializable
    {
        private readonly ICoroutineStarter _coroutineStarter = coroutineStarter;

        private Shader? _mirror;
        private Shader? _simpleLit;

        public Shader Mirror { get => _mirror ??= InternalErrorShader; set => _mirror = value; }
        public Shader SimpleLit { get => _simpleLit ??= InternalErrorShader; set => _simpleLit = value; }

        private static Shader InternalErrorShader => Shader.Find("Hidden/InternalErrorShader");

        public void Initialize() => _coroutineStarter.StartCoroutine(LoadShaders());

        private IEnumerator LoadShaders()
        {
            var mirrorShaderHandle = Addressables.LoadAssetAsync<Shader>("Assets/Visuals/Shaders/Mirror.shader");
            yield return mirrorShaderHandle;
            if (mirrorShaderHandle.Status == AsyncOperationStatus.Succeeded)
                Mirror = mirrorShaderHandle.Result;

            var simpleLitShaderHandle = Addressables.LoadAssetAsync<Shader>("Assets/Visuals/Shaders/SimpleLit.shader");
            yield return simpleLitShaderHandle;
            if (simpleLitShaderHandle.Status == AsyncOperationStatus.Succeeded)
                SimpleLit = simpleLitShaderHandle.Result;
        }
    }
}
