using System.IO;

using UnityEngine;


namespace CustomFloorPlugin.Helpers
{
    public static class ImageHelper
    {
        internal static Sprite ReadSprite(this Stream stream)
        {
            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);
            Texture2D tex = BytesToTexture2D(data);
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        }
        
        internal static Sprite ReadSprite(this BinaryReader binaryReader)
        {
            Texture2D tex = BytesToTexture2D(binaryReader.ReadBytes(binaryReader.ReadInt32()));
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        }

        internal static void WriteSprite(this BinaryWriter binaryWriter, Sprite sprite)
        {
            byte[] textureBytes = BytesFromTexture2D(sprite.texture);
            binaryWriter.Write(textureBytes.Length);
            binaryWriter.Write(textureBytes);
        }

        /// <summary>
        /// Converts a given <see cref="byte"/>[] to a <see cref="Texture2D"/>
        /// </summary>
        private static Texture2D BytesToTexture2D(byte[] bytes)
        {
            Texture2D tex = new(0, 0);
            tex.LoadImage(bytes);
            return tex;
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