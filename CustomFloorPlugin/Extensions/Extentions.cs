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

        internal static float GetLastNoteTime(this BeatmapData beatmapData) {
            float lastTime = 0f;
            IReadOnlyList<IReadonlyBeatmapLineData> beatmapLinesData = beatmapData.beatmapLinesData;
            foreach (BeatmapLineData beatMapLineData in beatmapLinesData) {
                IReadOnlyList<BeatmapObjectData> beatmapObjectsData = beatMapLineData.beatmapObjectsData;
                for (int i = beatmapObjectsData.Count - 1; i >= 0; i--) {
                    BeatmapObjectData beatmapObjectData = beatmapObjectsData[i];
                    if (beatmapObjectData.beatmapObjectType == BeatmapObjectType.Note && ((NoteData)beatmapObjectData).colorType != ColorType.None) {
                        if (beatmapObjectData.time > lastTime) {
                            lastTime = beatmapObjectData.time;
                        }
                    }
                }
            }
            return lastTime;
        }
    }
}
