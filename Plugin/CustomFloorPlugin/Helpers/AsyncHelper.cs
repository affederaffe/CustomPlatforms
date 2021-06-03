using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using IPA.Utilities.Async;

using UnityEngine;


namespace CustomFloorPlugin.Helpers
{
    internal static class AsyncHelper
    {
        private static WaitForEndOfFrame WaitForEndOfFrame => _waitForEndOfFrame ??= new WaitForEndOfFrame();
        private static WaitForEndOfFrame? _waitForEndOfFrame;

        /// <summary>
        /// Waits one frame via a Coroutine
        /// </summary>
        internal static async Task WaitForEndOfFrameAsync()
        {
            await Coroutines.AsTask(WaitForEndOfFrameCoroutine());
            static IEnumerator<WaitForEndOfFrame> WaitForEndOfFrameCoroutine() { yield return WaitForEndOfFrame; }
        }

        // https://thomaslevesque.com/2015/11/11/explicitly-switch-to-the-ui-thread-in-an-async-method/
        internal static SynchronizationContextAwaiter GetAwaiter(this SynchronizationContext synchronizationContext)
        {
            return new(synchronizationContext);
        }

        internal readonly struct SynchronizationContextAwaiter : INotifyCompletion
        {
            private static readonly SendOrPostCallback _postCallback = state => ((Action)state)();

            private readonly SynchronizationContext _synchronizationContext;

            public SynchronizationContextAwaiter(SynchronizationContext synchronizationContext)
            {
                _synchronizationContext = synchronizationContext;
            }

            public bool IsCompleted => _synchronizationContext == SynchronizationContext.Current;

            public void OnCompleted(Action continuation) => _synchronizationContext.Post(_postCallback, continuation);

            public void GetResult() { }
        }
    }
}