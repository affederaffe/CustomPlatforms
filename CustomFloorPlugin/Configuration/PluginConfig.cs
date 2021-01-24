using System.Runtime.CompilerServices;

using IPA.Config.Stores;


[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace CustomFloorPlugin.Configuration {


    internal class PluginConfig {

        public virtual bool AlwaysShowFeet { get; set; } = false;
        public virtual bool ShowHeart { get; set; } = true;
        public virtual bool LoadCustomScripts { get; set; } = false;
        public virtual bool UseIn360 { get; set; } = false;
        public virtual bool UseInMultiplayer { get; set; } = false;
        public virtual string CustomPlatformPath { get; set; }
    }
}
