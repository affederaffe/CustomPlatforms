using System.Runtime.CompilerServices;

using IPA.Config.Stores;


[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace CustomFloorPlugin.Configuration
{
    public class PluginConfig
    {
        public virtual string? SingleplayerPlatformHash { get; set; }
        public virtual string? MultiplayerPlatformHash { get; set; }
        public virtual string? A360PlatformHash { get; set; }
        public virtual string? MenuPlatformHash { get; set; }
    }
}