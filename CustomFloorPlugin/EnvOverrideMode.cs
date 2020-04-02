namespace CustomFloorPlugin {


    /// <summary>
    /// Used to determine what Scene will be loaded after hitting the play button, if the <see cref="EnvironmentSceneOverrider"/> is active.
    /// </summary>
    internal enum EnvOverrideMode {
        Default,
        Nice,
        BigMirror,
        Triangle,
        KDA,
        Monstercat
    };
}