using System;
using System.Reflection;

namespace CustomFloorPlugin.Exceptions {


    public sealed class ComponentNotFoundException : Exception {
        public TypeInfo TypeInfo { get; }

        internal ComponentNotFoundException(TypeInfo typeInfo) :
            base("No such Component currently present on any GameObject in any scene: " + typeInfo.AssemblyQualifiedName) {
            TypeInfo = typeInfo;
        }
    }
}
