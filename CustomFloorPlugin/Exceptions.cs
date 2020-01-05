using System;
using System.Reflection;

namespace CustomFloorPlugin.Exceptions {
    internal sealed class BeatmapObjectCallbackControllerNotFoundException:ComponentNotFoundException {
        internal BeatmapObjectCallbackControllerNotFoundException(TypeInfo T) :
            base(T) {

        }
    }
    internal class ManagerNotFoundException:Exception {
        internal ManagerNotFoundException() :
            base("No Manager could be found!") {

        }
        internal ManagerNotFoundException(Exception e) :
            base("No Manager could be found!", e) {

        }
    }
    internal class EnvironmentSceneNotFoundException:Exception {
        internal EnvironmentSceneNotFoundException() :
            base("No Environment Scene could be found!") {

        }
    }
    public class ComponentNotFoundException:Exception {
        public TypeInfo TypeInfo;
        internal ComponentNotFoundException(TypeInfo T) :
            base("No such Component currently present on any GameObject in any scene: " + T.AssemblyQualifiedName) {
            TypeInfo = T;
        }
    }
}
