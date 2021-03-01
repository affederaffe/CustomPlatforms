﻿using System.Runtime.CompilerServices;

using IPA.Config.Stores;


[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace CustomFloorPlugin.Configuration
{
    internal class PluginConfig
    {
        public virtual bool AlwaysShowFeet { get; set; } = false;
        public virtual bool ShowHeart { get; set; } = true;
        public virtual bool ShowInMenu { get; set; } = false;
        public virtual bool LoadCustomScripts { get; set; } = false;
        public virtual string SingleplayerPlatformPath { get; set; }
        public virtual string MultiplayerPlatformPath { get; set; }
        public virtual string A360PlatformPath { get; set; }
    }
}