using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using UnityEngine;


namespace CustomFloorPlugin.Extensions {


    /// <summary>
    /// This class holds extensions
    /// </summary>
    internal static class Extentions {


        /// <summary>
        /// Returns the full path of a GameObject in the scene hierarchy.
        /// </summary>
        /// <param name="gameObject">The instance of a GameObject to generate a path for.</param>
        /// <returns></returns>
        internal static string GetFullPath(this GameObject gameObject) {
            StringBuilder path = new StringBuilder();
            while(true) {
                path.Insert(0, "/" + gameObject.name);
                if(gameObject.transform.parent == null) {
                    path.Insert(0, gameObject.scene.name);
                    break;
                }
                gameObject = gameObject.transform.parent.gameObject;
            }
            return path.ToString();
        }


        /// <summary>
        /// Returns the full path of a Component in the scene hierarchy.
        /// </summary>
        /// <param name="component">The instance of a Component to generate a path for.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "No")]
        internal static string GetFullPath(this Component component) {
            StringBuilder path = new StringBuilder(component.gameObject.GetFullPath());
            path.Append("/" + component.GetType().Name);
            return path.ToString();
        }


        /// <summary>
        /// Converts a <see cref="List{T}"/> to a list of <see langword="object"/>s for BSML
        /// </summary>
        /// <typeparam name="T">Type of the original list</typeparam>
        /// <param name="list">The original list</param>
        internal static List<object> ToBoxedList<T>(this List<T> list) {
            List<object> convertedList = new List<object>();
            foreach(T thing in list) {
                convertedList.Add(thing);
            }
            return convertedList;
        }
    }
}
