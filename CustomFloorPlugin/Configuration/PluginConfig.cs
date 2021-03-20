using System.IO;
using System.Runtime.CompilerServices;

using IPA.Config.Stores;
using IPA.Utilities;


[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace CustomFloorPlugin.Configuration
{
    public class PluginConfig
    {
        public virtual bool AlwaysShowFeet { get; set; } = false;
        public virtual bool ShowHeart { get; set; } = true;
        public virtual bool ShowInMenu { get; set; } = false;
        public virtual bool ShufflePlatforms { get; set; } = false;
        public virtual string SingleplayerPlatformPath { get; set; }
        public virtual string MultiplayerPlatformPath { get; set; }
        public virtual string A360PlatformPath { get; set; }
        public virtual string CustomPlatformsDirectory { get; set; } = Path.Combine(UnityGame.InstallPath, "CustomPlatforms");
    }
}
