using UnityEngine;
using System.IO;
using System;
#if USE_DEVIL
using DevIL;
#endif

namespace TriLib
{
    /// <summary>
    /// Represents a texture compression parameter.
    /// </summary>
    public enum TextureCompression
    {
        /// <summary>
        /// No texture compression will be applied.
        /// </summary>
        None,

        /// <summary>
        /// Normal-quality texture compression will be applied.
        /// </summary>
        NormalQuality,

        /// <summary>
        /// High-quality texture compression will be applied.
        /// </summary>
        HighQuality
    }

    /// <summary>
    /// Represents a <see cref="UnityEngine.Texture2D"/> loading event handle.
    /// </summary>
    public delegate void TextureLoadHandle(string sourcePath, Material material, string propertyName, Texture2D texture);

    /// <summary>
    /// Represents a class to load external textures.
    /// </summary>
    public static class Texture2DUtils
    {
        /// <summary>
        /// Loads a <see cref="UnityEngine.Texture2D"/> from an external source.
        /// </summary>
        /// <param name="scene">Scene where the texture belongs.</param>
        /// <param name="path">Path to load the texture data.</param>
        /// <param name="name">Name of the <see cref="UnityEngine.Texture2D"/> to be created.</param>
        /// <param name="material"><see cref="UnityEngine.Material"/> to assign the <see cref="UnityEngine.Texture2D"/>.</param>
        /// <param name="propertyName"><see cref="UnityEngine.Material"/> property name to assign to the <see cref="UnityEngine.Texture2D"/>.</param>
        /// <param name="textureWrapMode">Wrap mode of the <see cref="UnityEngine.Texture2D"/> to be created.</param>
        /// <param name="basePath">Base path to lookup for the <see cref="UnityEngine.Texture2D"/>.</param>
        /// <param name="onTextureLoaded">Event to trigger when the <see cref="UnityEngine.Texture2D"/> finishes loading.</param>
        /// <param name="textureCompression">Texture loading compression level.</param>
        /// <param name="textureFileNameWithoutExtension">Texture filename without the extension.</param>
        public static void LoadTextureFromFile(IntPtr scene, string path, string name, Material material, string propertyName, TextureWrapMode textureWrapMode = TextureWrapMode.Repeat, string basePath = null, TextureLoadHandle onTextureLoaded = null, TextureCompression textureCompression = TextureCompression.None, string textureFileNameWithoutExtension = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            bool assimpUncompressed;
            string finalPath;
            byte[] data;
            if (path[0] == '*')
            {
                UInt32 slotIndex;
                if (UInt32.TryParse(path.Substring(1), out slotIndex))
                {
                    assimpUncompressed = !AssimpInterop.aiMaterial_IsEmbeddedTextureCompressed(scene, slotIndex);
                    var dataLength = AssimpInterop.aiMaterial_GetEmbeddedTextureDataSize(scene, slotIndex, !assimpUncompressed);
                    data = AssimpInterop.aiMaterial_GetEmbeddedTextureData(scene, slotIndex, dataLength);
                }
                else
                {
#if ASSIMP_OUTPUT_MESSAGES
                    Debug.LogWarningFormat("Unable to process embedded texture '{0}'", path);
#endif
                    return;
                }
                finalPath = StringUtils.GenerateUniqueName(path);
            }
            else
            {
                finalPath = path;
                if (!File.Exists(finalPath))
                {
                    if (basePath != null)
                    {
                        finalPath = Path.Combine(basePath, path);
                    }
                }
                if (!File.Exists(finalPath))
                {
                    var filename = Path.GetFileName(path);
                    if (basePath != null)
                    {
                        finalPath = Path.Combine(basePath, filename);
                    }
                }
                if (!File.Exists(finalPath))
                {
#if ASSIMP_OUTPUT_MESSAGES
                    Debug.LogWarningFormat("Texture '{0}' not found", path);
#endif
                    return;
                }
                data = File.ReadAllBytes(finalPath);
                assimpUncompressed = false;
            }
            bool loaded;
            Texture2D texture2D;
            if (assimpUncompressed)
            {
                //TODO: additional DLL methods to load actual resolution
                var textureResolution = Mathf.FloorToInt(Mathf.Sqrt(data.Length / 4));
                texture2D = new Texture2D(textureResolution, textureResolution, TextureFormat.ARGB32, false);
                texture2D.LoadRawTextureData(data);
                texture2D.Apply();
                loaded = true;
            }
            else
            {
#if USE_DEVIL && (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
                loaded = IlLoader.LoadTexture2DFromByteArray(data, data.Length, out texture2D);  
#else
                texture2D = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                loaded = texture2D.LoadImage(data);
#endif
            }
            texture2D.name = name;
            texture2D.wrapMode = textureWrapMode;
            if (loaded)
            {
                if (textureCompression != TextureCompression.None)
                {
                    texture2D.Compress(textureCompression == TextureCompression.HighQuality);
                }
                material.SetTexture(propertyName, texture2D);
                if (onTextureLoaded != null)
                {
                    onTextureLoaded(finalPath, material, propertyName, texture2D);
                }
            }
            else
            {
#if ASSIMP_OUTPUT_MESSAGES
                Debug.LogErrorFormat("Unable to load texture '{0}'", path);
#endif
            }
        }
    }
}

