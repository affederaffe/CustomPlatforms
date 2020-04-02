// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "No Globalization planned. Stop bugging me about it Microsoft", Scope = "namespaceanddescendants", Target = "CustomFloorPlugin")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2237:Mark ISerializable types with serializable", Justification = "Beat Saber does not cross application domains or remoting boundries!", Scope = "namespaceanddescendants", Target = "CustomFloorPlugin.Exceptions")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "No further implementations needed", Scope = "namespaceanddescendants", Target = "CustomFloorPlugin.Exceptions")]