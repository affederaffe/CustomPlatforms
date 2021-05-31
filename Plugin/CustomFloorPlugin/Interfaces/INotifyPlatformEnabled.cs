using Zenject;


namespace CustomFloorPlugin
{
    /// <summary>
    /// <see cref="PlatformEnabled"/> will be called on every <see cref="UnityEngine.Component"/>
    /// implementing this interface under the platform to be activated
    /// </summary>
    public interface INotifyPlatformEnabled
    {
        void PlatformEnabled(DiContainer container);
    }
}