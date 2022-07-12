namespace CustomFloorPlugin.Interfaces
{
    /// <summary>
    /// <see cref="PlatformDisabled"/> will be called on every <see cref="UnityEngine.Component"/>
    /// implementing this interface under the platform to be deactivated
    /// </summary>
    public interface INotifyPlatformDisabled
    {
        void PlatformDisabled();
    }
}
