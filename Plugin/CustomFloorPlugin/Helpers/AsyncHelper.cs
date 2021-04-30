using System.Collections.Generic;
using System.Threading.Tasks;

using IPA.Utilities.Async;

using UnityEngine;


namespace CustomFloorPlugin.Helpers
{
    public static class AsyncHelper
    {
        private static WaitForEndOfFrame WaitForEndOfFrame => _waitForEndOfFrame ??= new WaitForEndOfFrame();
        private static WaitForEndOfFrame? _waitForEndOfFrame;
        
        /// <summary>
        /// Waits one frame via a Coroutine
        /// </summary>
        public static async Task WaitForEndOfFrameAsync()
        {
            await Coroutines.AsTask(WaitForEndOfFrameCoroutine());
            static IEnumerator<WaitForEndOfFrame> WaitForEndOfFrameCoroutine() { yield return WaitForEndOfFrame; }
        }
    }
}