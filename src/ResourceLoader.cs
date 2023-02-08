using RWCustom;
using System.IO;
using UnityEngine;

namespace SlugcatEyebrowRaise
{
    internal static class ResourceLoader
    {
        private const string SPRITES_DIRPATH = "sprites";

        public static void LoadSprites()
        {
            foreach (string filePath in AssetManager.ListDirectory(SPRITES_DIRPATH))
            {
                // https://answers.unity.com/questions/432655/loading-texture-file-from-pngjpg-file-on-disk.html
                byte[] fileData = File.ReadAllBytes(filePath);
                Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, false)
                {
                    anisoLevel = 1,
                    filterMode = FilterMode.Point
                };
                texture.LoadImage(fileData);

                Futile.atlasManager.LoadAtlasFromTexture(SlugcatEyebrowRaise.MOD_ID + "_" + Path.GetFileNameWithoutExtension(filePath), texture, false);
            }
        }
    }
}
