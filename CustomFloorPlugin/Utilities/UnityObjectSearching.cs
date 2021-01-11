using System.Collections.Generic;
using System.Reflection;

using CustomFloorPlugin.Exceptions;

using UnityEngine;
using UnityEngine.SceneManagement;


namespace CustomFloorPlugin.Utilities {


    /// <summary>
    /// Provides generic functionality for <see cref="GameObject"/> and <see cref="Component"/> searching
    /// </summary>
    internal static class UnityObjectSearching {


        /// <summary>
        /// Searches all currently loaded <see cref="Scene"/>s to find the first <see cref="Component"/> of type <typeparamref name="T"/> in the game, regardless if it's active or not.
        /// </summary>
        /// <typeparam name="T">What to look for</typeparam>
        /// <exception cref="ComponentNotFoundException"></exception>
        /// <returns></returns>
        internal static T FindFirst<T>() {
            for (int i = 0; i < SceneManager.sceneCount; i++) {
                Scene scene = SceneManager.GetSceneAt(i);
                foreach (GameObject root in scene.GetRootGameObjects()) {
                    if (InternalRecursiveFindFirst<T>(root.transform, out object component)) {
                        return (T)component;
                    }
                }
            }
            throw new ComponentNotFoundException(typeof(T).GetTypeInfo());
        }


        /// <summary>
        /// Searches the given <see cref="Scene"/> for the first <see cref="Component"/> of type <typeparamref name="T"/>, regardless if it's active or not.
        /// </summary>
        /// <typeparam name="T">What to look for</typeparam>
        /// <param name="scene"> Where to look</param>
        /// <exception cref="ComponentNotFoundException"></exception>
        /// <returns></returns>
        internal static T FindFirst<T>(Scene scene) {
            foreach (GameObject root in scene.GetRootGameObjects()) {
                if (InternalRecursiveFindFirst<T>(root.transform, out object component)) {
                    return (T)component;
                }
            }
            throw new ComponentNotFoundException(typeof(T).GetTypeInfo());
        }


        /// <summary>
        /// Searches under the given <see cref="GameObject"/> for the first <see cref="Component"/> of type <typeparamref name="T"/>, regardless if it's active or not.
        /// </summary>
        /// <typeparam name="T">What to look for</typeparam>
        /// <param name="gameObject"> Where to look</param>
        /// <exception cref="ComponentNotFoundException"></exception>
        /// <returns></returns>
        internal static T FindFirst<T>(GameObject gameObject) {
            if (InternalRecursiveFindFirst<T>(gameObject.transform, out object component)) {
                return (T)component;
            }
            throw new ComponentNotFoundException(typeof(T).GetTypeInfo());
        }


        /// <summary>
        /// Not to be used directly!<br/>
        /// Recursive variant of <see cref="FindFirst{T}()"/>
        /// </summary>
        private static bool InternalRecursiveFindFirst<T>(Transform transform, out object component) {
            component = transform.GetComponent<T>();
            if (component != null) {
                return true;
            }
            else if (transform.childCount != 0) {
                for (int i = 0; i < transform.childCount; i++) {
                    if (InternalRecursiveFindFirst<T>(transform.GetChild(i), out component)) {
                        return true;
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// Finds all <see cref="Component"/>s of Type <typeparamref name="T"/>, regardless if active or not.
        /// </summary>
        /// <typeparam name="T">What to look for</typeparam>
        /// <exception cref="ComponentNotFoundException"></exception>
        /// <returns></returns>
        public static List<T> FindAll<T>() {
            List<T> components = new List<T>();
            for (int i = 0; i < SceneManager.sceneCount; i++) {
                Scene scene = SceneManager.GetSceneAt(i);
                foreach (GameObject root in scene.GetRootGameObjects()) {
                    InternalRecursiveFindAll<T>(root.transform, ref components);
                }
            }
            if (components.Count == 0) {
                throw new ComponentNotFoundException(typeof(T).GetTypeInfo());
            }
            return components;
        }


        /// <summary>
        /// Finds all <see cref="Component"/>s of Type <typeparamref name="T"/> inside the specified <see cref="Scene"/>, regardless if active or not.
        /// </summary>
        /// <typeparam name="T">What to look for</typeparam>
        /// <param name="scene">Where to look</param>
        /// <exception cref="ComponentNotFoundException"></exception>
        /// <returns></returns>
        public static List<T> FindAll<T>(Scene scene) {
            List<T> components = new List<T>();
            foreach (GameObject root in scene.GetRootGameObjects()) {
                InternalRecursiveFindAll<T>(root.transform, ref components);
            }
            if (components.Count == 0) {
                throw new ComponentNotFoundException(typeof(T).GetTypeInfo());
            }
            return components;
        }


        /// <summary>
        /// Finds all <see cref="Component"/>s of Type <typeparamref name="T"/> under the specified <see cref="GameObject"/>, regardless if active or not.
        /// </summary>
        /// <typeparam name="T">What Type to look for</typeparam>
        /// <param name="gameObject">Where to look</param>
        /// <exception cref="ComponentNotFoundException"></exception>
        /// <returns></returns>
        public static List<T> FindAll<T>(GameObject gameObject) {
            List<T> components = new List<T>();
            InternalRecursiveFindAll<T>(gameObject.transform, ref components);
            if (components.Count == 0) {
                throw new ComponentNotFoundException(typeof(T).GetTypeInfo());
            }
            return components;
        }


        /// <summary>
        /// Not to be used directly!<br/>
        /// Recursive variant of <see cref="FindAll{T}()"/>
        /// </summary>
        private static void InternalRecursiveFindAll<T>(Transform transform, ref List<T> components) {
            components.AddRange(transform.GetComponents<T>());
            if (transform.childCount != 0) {
                for (int i = 0; i < transform.childCount; i++) {
                    InternalRecursiveFindAll<T>(transform.GetChild(i), ref components);
                }
            }
        }


        /// <summary>
        ///  Checks if a given <see cref="List{T}"/> is <see cref="null"/>, <see cref="0"/> or contains only <see cref="null"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool IsNullZeroOrContainsNull<T>(List<T> list) {
            if (list == null || list.Count == 0) {
                return true;
            }
            foreach (T t in list) {
                if (t.Equals(null)) {
                    return true;
                }
            }
            return false;
        }
    }
}