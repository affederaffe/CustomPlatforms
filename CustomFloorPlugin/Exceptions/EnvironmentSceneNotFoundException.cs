using System;

namespace CustomFloorPlugin.Exceptions {


    public sealed class EnvironmentSceneNotFoundException : Exception {
        internal EnvironmentSceneNotFoundException() :
            base("No Environment Scene could be found!") {

        }
    }
}
