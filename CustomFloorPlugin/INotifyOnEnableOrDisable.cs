namespace CustomFloorPlugin
{
    /// <summary>
    /// Interface for CustomPlatform objects<br/>
    /// The <see cref="PlatformManager"/> will notify any objects under the affected <see cref="CustomPlatform"/> when it gets enabled or disabled
    /// </summary>
    internal interface INotifyOnEnableOrDisable
    {
        /// <summary>
        /// Called when the parent <see cref="CustomPlatform"/> is enabled.
        /// </summary>
        internal void PlatformEnabled();

        /// <summary>
        /// Called when the parent <see cref="CustomPlatform"/> is disabled.
        /// </summary>
        internal void PlatformDisabled();
    }
}
