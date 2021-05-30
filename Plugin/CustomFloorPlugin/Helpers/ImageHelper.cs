using System.IO;

using UnityEngine;


namespace CustomFloorPlugin.Helpers
{
    internal static class ImageHelper
    {
        internal static Texture2D ReadTexture2D(this Stream stream)
        {
            byte[] array = new byte[stream.Length];
            stream.Read(array, 0, array.Length);
            return BytesToTexture2D(array);
        }

        internal static Texture2D ReadTexture2D(this BinaryReader binaryReader)
        {
            byte[] bytes = binaryReader.ReadBytes(binaryReader.ReadInt32());
            return BytesToTexture2D(bytes);
        }

        internal static void WriteSprite(this BinaryWriter binaryWriter, Sprite? sprite)
        {
            if (sprite is null)
            {
                binaryWriter.Write(false);
                return;
            }

            byte[] textureBytes = BytesFromTexture2D(sprite.texture);
            binaryWriter.Write(true);
            binaryWriter.Write(textureBytes.Length);
            binaryWriter.Write(textureBytes);
        }

        internal static Sprite ToSprite(this Texture2D texture2D)
        {
            Sprite sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero);
            return sprite;
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
                int width = Mathf.Min(texture.width, 192);
                int height = Mathf.Min(texture.height, 192);
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