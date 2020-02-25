using System;

namespace CustomFloorPlugin.Exceptions {


    /// <summary>
    /// A previous request has not yet been consumed, confirm with <see cref="OverridePreviousRequest"/> if the override was intentional
    /// </summary>
    public sealed class StackedRequestsException:Exception {


        /// <summary>
        /// Creates a <see cref="StackedRequestsException"/> using the default message
        /// </summary>
        internal StackedRequestsException() :
            base("A previous request has not yet been consumed, override with RequestsStackedException.OverridePreviousRequest() if the override was intentional") {

        }


        /// <summary>
        /// Allows you to override the previous request<br/>
        /// Does nothing if there was no previous request
        /// </summary>
        public static void OverridePreviousRequest() {
            PlatformManager.OverridePreviousRequest();
        }
    }
}
