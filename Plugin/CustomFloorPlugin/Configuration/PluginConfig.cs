using System.Runtime.CompilerServices;

using IPA.Config.Stores;


[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace CustomFloorPlugin.Configuration
{
    public class PluginConfig
    {
        public virtual bool AlwaysShowFeet { get; set; }
        public virtual bool ShowHeart { get; set; } = true;
        public virtual bool ShowInMenu { get; set; }
        public virtual bool ShufflePlatforms { get; set; }
        public virtual string? SingleplayerPlatformPath { get; set; }
        public virtual string? MultiplayerPlatformPath { get; set; }
        public virtual string? A360PlatformPath { get; set; }
    }
}