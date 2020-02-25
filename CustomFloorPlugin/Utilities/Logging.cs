using CustomFloorPlugin.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using Level = IPA.Logging.Logger.Level;

namespace CustomFloorPlugin.Utilities {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This is a Logger, it needs to catch general exceptions types!", Scope = "type", Target = "~T:CustomFloorPlugin.Logger")]
    internal static class Logging {

        internal static IPA.Logging.Logger logger;

        internal static void Log(string message = "<3", Level level = Level.Info) {
            logger.Log(level, message);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        internal static void Log(Exception e, Level level = Level.Notice) {
            Log("An error has been caught:\n" + e.GetType().Name + "\nAt:\n" + e.StackTrace + "\nWith message:\n" + e.Message, level);
            if(e.InnerException != null) {
                Log("---Inner Exception:---", level);
                Log(e, level);
            }
        }
        internal static void Log<T>(List<T> messages, Level level = Level.Info) {
            foreach(T message in messages) {
                try {
                    if(message is GameObject) {
                        Log(message as GameObject, level);
                    } else if(message is Component) {
                        Log(message as Component, level);
                    } else {
                        Log(message, level);
                    }
                } catch(Exception e) {
                    Log(e, Level.Error);
                }
            }
        }
        internal static void Log<T>(T[] messages, Level level = Level.Info) {
            foreach(T message in messages) {
                try {
                    if(message is GameObject) {
                        Log(message as GameObject, level);
                    } else if(message is Component) {
                        Log(message as Component, level);
                    } else {
                        Log(message, level);
                    }
                } catch(Exception e) {
                    Log(e, Level.Error);
                }
            }
        }
        internal static void Log<T>(HashSet<T> messages, Level level = Level.Info) {
            foreach(T message in messages) {
                try {
                    if(message is GameObject) {
                        Log(message as GameObject, level);
                    } else if(message is Component) {
                        Log(message as Component, level);
                    } else {
                        Log(message, level);
                    }
                } catch(Exception e) {
                    Log(e, Level.Error);
                }
            }
        }
        internal static void Log(Component message, Level level = Level.Info) {
            try {
                Log(message.GetFullPath(), level);
            } catch(Exception e) {
                Log(e, Level.Error);
            }
        }
        internal static void Log(GameObject message, Level level = Level.Info) {
            try {
                Log(message.GetFullPath(), level);
            } catch(Exception e) {
                Log(e, Level.Error);
            }
        }
        internal static void Log(object message, Level level = Level.Info) {
            try {
                Log(message.ToString(), level);
            } catch(Exception e) {
                Log(e, Level.Error);
            }
        }
    }
}
