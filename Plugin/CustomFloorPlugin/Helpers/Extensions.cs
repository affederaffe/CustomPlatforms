using System.Collections.Generic;


namespace CustomFloorPlugin.Helpers
{
    /// <summary>
    /// This class holds extensions
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Returns the time of the song the last note is spawned<br/>
        /// (stolen from SaberFactory)
        /// </summary>
        internal static float GetLastNoteTime(this BeatmapData beatmapData)
        {
            float lastTime = 0f;
            IReadOnlyList<IReadonlyBeatmapLineData> beatmapLinesData = beatmapData.beatmapLinesData;
            foreach (IReadonlyBeatmapLineData beatMapLineData in beatmapLinesData)
            {
                IReadOnlyList<BeatmapObjectData> beatmapObjectsData = beatMapLineData.beatmapObjectsData;
                for (int i = beatmapObjectsData.Count - 1; i >= 0; i--)
                {
                    BeatmapObjectData beatmapObjectData = beatmapObjectsData[i];
                    if (beatmapObjectData.beatmapObjectType == BeatmapObjectType.Note && ((NoteData)beatmapObjectData).colorType != ColorType.None && beatmapObjectData.time > lastTime)
                    {
                        lastTime = beatmapObjectData.time;
                    }
                }
            }
            return lastTime;
        }
    }
}