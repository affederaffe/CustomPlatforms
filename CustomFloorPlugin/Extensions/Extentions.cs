using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using BS_Utils.Utilities;

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
            while (true) {
                path.Insert(0, "/" + gameObject.name);
                if (gameObject.transform.parent == null) {
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
            foreach (T thing in list) {
                convertedList.Add(thing);
            }
            return convertedList;
        }


        /// <summary>
        /// Fills a <see cref="LightWithIdManager"/> with colors so Platforms' lights don't appear black
        /// </summary>
        /// <param name="manager">The <see cref="LightWithIdManager"/> to fill the colors in</param>
        /// <param name="colors">What Colors to use</param>
        internal static void FillManager(this LightWithIdManager manager, Color?[] colors = null) {
            if (colors == null) {
                ColorScheme scheme = GlobalCollection.PDM.playerData.colorSchemesSettings.GetOverrideColorScheme();
                colors = new Color?[] {
                    scheme.environmentColor0,
                    scheme.environmentColor1,
                    scheme.obstaclesColor,
                    scheme.saberAColor,
                    scheme.saberBColor,
                    scheme.environmentColor0,
                    scheme.environmentColor1,
                    scheme.obstaclesColor,
                    scheme.saberAColor,
                    scheme.saberBColor,
                    scheme.environmentColor0,
                    scheme.environmentColor1,
                    scheme.obstaclesColor,
                    scheme.saberAColor,
                    scheme.saberBColor,
                    scheme.environmentColor0,
                    scheme.environmentColor1,
                    scheme.obstaclesColor,
                    scheme.saberAColor,
                    scheme.saberBColor,
                };
            }
            manager.SetField("_colors", colors);
        }
    }
}
