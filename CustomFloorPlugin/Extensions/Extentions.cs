using System.Reflection;
using System.Text;
using UnityEngine;

namespace CustomFloorPlugin.Extensions {
    internal static class Extentions {
        internal static void InvokePrivateMethod<T>(this object obj, string methodName, params object[] methodParams) {
            var method = typeof(T).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(obj, methodParams);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "bad design, i know")]
        internal static void SetPrivateField<T>(this T obj, string fieldName, object value) {
            try {
                typeof(T).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(obj, value);
            } catch {
                obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(obj, value);
            }
        }
        internal static T GetPrivateField<T>(this object obj, string fieldName) {

            return (T)obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(obj);

        }
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
        internal static string GetFullPath(this Component component) {
            StringBuilder path = new StringBuilder(component.gameObject.GetFullPath());
            path.Append("/" + component.GetType().Name);
            return path.ToString();
        }
    }
}
