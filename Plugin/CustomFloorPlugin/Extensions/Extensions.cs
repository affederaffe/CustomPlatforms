using System.Collections.Generic;
using System.IO;

using UnityEngine;


namespace CustomFloorPlugin.Extensions
{
    /// <summary>
    /// This class holds extensions
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Checks if a <see cref="ScenesTransitionSetupDataSO"/> transitions to a 360° level
        /// </summary>
        internal static bool Is360Level(this ScenesTransitionSetupDataSO setupData)
        {
            bool is360 = (setupData as StandardLevelScenesTransitionSetupDataSO)?
                .difficultyBeatmap
                .parentDifficultyBeatmapSet
                .beatmapCharacteristic
                .requires360Movement
                ?? false;
            return is360;
        }

        /// <summary>
        /// Tries to grab the LevelId from the <see cref="ScenesTransitionSetupDataSO"/>
        /// </summary>
        internal static string GetLevelId(this ScenesTransitionSetupDataSO setupData)
        {
            string levelId = (setupData as StandardLevelScenesTransitionSetupDataSO)?
                .difficultyBeatmap
                .level
                .levelID
                ?? string.Empty;
            return levelId;
        }

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

        /// <summary>
        /// Reads a <see cref="Sprite"/> from a <see cref="Stream"/> of a .png image
        /// </summary>
        internal static Sprite ReadPNGToSprite(this Stream resourceStream)
        {
            byte[] data = new byte[resourceStream.Length];
            resourceStream.Read(data, 0, data.Length);
            Texture2D tex = new(2, 2);
            tex.LoadImage(data);
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        }

        /// <summary>
        /// Reads a <see cref="Texture2D"/> from a <see cref="BinaryReader"/>
        /// (stolen from CustomAvatars)
        /// </summary>
        internal static Texture2D ReadTexture2D(this BinaryReader reader)
        {
            return BytesToTexture2D(reader.ReadBytes(reader.ReadInt32()));
        }

        /// <summary>
        /// Writes a <see cref="Texture2D"/> with a <see cref="BinaryReader"/>
        /// </summary>
        internal static void WriteTexture2D(this BinaryWriter writer, Texture2D texture)
        {
            byte[] textureBytes = BytesFromTexture2D(texture);
            writer.Write(textureBytes.Length);
            writer.Write(textureBytes);
        }

        /// <summary>
        /// Converts a given <see cref="byte"/>[] to a <see cref="Texture2D"/>
        /// </summary>
        private static Texture2D BytesToTexture2D(byte[] bytes)
        {
            Texture2D texture = new(0, 0, TextureFormat.ARGB32, false);
            texture.LoadImage(bytes);
            return texture;
        }

        /// <summary>
        /// Converts a given <see cref="Texture2D"/> to a <see cref="byte"/>[]
        /// </summary>
        private static byte[] BytesFromTexture2D(Texture2D texture)
        {
            // Create readable texture by rendering onto a RenderTexture
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