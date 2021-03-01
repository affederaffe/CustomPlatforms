using System.Collections.Generic;
using System.IO;

using UnityEngine;


namespace CustomFloorPlugin.Extensions
{
    /// <summary>
    /// This class holds extensions
    /// </summary>
    internal static class Extentions
    {
        /// <summary>
        /// Gets the time of the song the last note is spawned<br></br>
        /// (stolen from SaberFactory)
        /// </summary>
        /// <param name="beatmapData"></param>
        /// <returns></returns>
        internal static float GetLastNoteTime(this BeatmapData beatmapData)
        {
            float lastTime = 0f;
            IReadOnlyList<IReadonlyBeatmapLineData> beatmapLinesData = beatmapData.beatmapLinesData;
            foreach (BeatmapLineData beatMapLineData in beatmapLinesData)
            {
                IReadOnlyList<BeatmapObjectData> beatmapObjectsData = beatMapLineData.beatmapObjectsData;
                for (int i = beatmapObjectsData.Count - 1; i >= 0; i--)
                {
                    BeatmapObjectData beatmapObjectData = beatmapObjectsData[i];
                    if (beatmapObjectData.beatmapObjectType == BeatmapObjectType.Note && ((NoteData)beatmapObjectData).colorType != ColorType.None)
                    {
                        if (beatmapObjectData.time > lastTime)
                        {
                            lastTime = beatmapObjectData.time;
                        }
                    }
                }
            }
            return lastTime;
        }

        /// <summary>
        /// Reads a <see cref="Texture2D"/> from a <see cref="BinaryReader"/>
        /// (stolen from CustomAvatars)
        /// </summary>
        /// <returns></returns>
        internal static Texture2D ReadTexture2D(this BinaryReader reader)
        {
            return BytesToTexture2D(reader.ReadBytes(reader.ReadInt32()));
        }

        /// <summary>
        /// Writes a <see cref="Texture2D"/> with a <see cref="BinaryReader"/>
        /// </summary>
        internal static void Write(this BinaryWriter writer, Texture2D texture, bool forceReadable)
        {
            byte[] textureBytes = BytesFromTexture2D(texture, forceReadable);

            writer.Write(textureBytes.Length);
            writer.Write(textureBytes);
        }

        /// <summary>
        /// Converts a given <see cref="byte"/>[] to a <see cref="Texture2D"/>
        /// </summary>
        private static Texture2D BytesToTexture2D(byte[] bytes)
        {
            if (bytes.Length == 0) return null;

            Texture2D texture = new Texture2D(0, 0, TextureFormat.ARGB32, false);

            texture.LoadImage(bytes);

            return texture;
        }

        /// <summary>
        /// Converts a given <see cref="Texture2D"/> to a <see cref="byte"/>[]
        /// </summary>
        private static byte[] BytesFromTexture2D(Texture2D texture, bool forceReadable)
        {
            if (texture == null || (!texture.isReadable && !forceReadable)) return new byte[0];

            // create readable texture by rendering onto a RenderTexture
            if (!texture.isReadable)
            {
                float maxSize = 256;
                float scale = Mathf.Min(1, maxSize / texture.width, maxSize / texture.height);
                int width = Mathf.RoundToInt(texture.width * scale);
                int height = Mathf.RoundToInt(texture.height * scale);

                RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
                RenderTexture.active = renderTexture;
                Graphics.Blit(texture, renderTexture);
                texture = renderTexture.GetTexture2D();
                RenderTexture.active = null;
                RenderTexture.ReleaseTemporary(renderTexture);
            }
            return texture.EncodeToPNG();
        }
    }
}
