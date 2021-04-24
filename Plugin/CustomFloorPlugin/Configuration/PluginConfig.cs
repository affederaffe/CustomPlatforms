using System.IO;
using System.Runtime.CompilerServices;

using IPA.Config.Stores;
using IPA.Utilities;


[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace CustomFloorPlugin.Configuration
{
    public class PluginConfig
    {
        public virtual bool AlwaysShowFeet { get; set; }
        public virtual bool ShowHeart { get; set; } = true;
        public virtual bool ShowInMenu { get; set; }
        public virtual bool ShufflePlatforms { get; set; }
        public virtual string SingleplayerPlatformPath { get; set; } = string.Empty;
        public virtual string MultiplayerPlatformPath { get; set; } = string.Empty;
        public virtual string A360PlatformPath { get; set; } = string.Empty;
        public virtual string CustomPlatformsDirectory { get; } = Path.Combine(UnityGame.InstallPath, "CustomPlatforms");
    }
}