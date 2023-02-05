using RWCustom;
using System.IO;
using UnityEngine;

namespace SlugcatEyebrowRaise
{
    internal static class ResourceLoader
    {
        private const string SPRITE_DIRECTORY_ASSET_PATH = "\\mods\\slugcateyebrowraise\\sprites";

        public static void LoadSprites()
        {
            string fullPath = Custom.RootFolderDirectory() + SPRITE_DIRECTORY_ASSET_PATH;

            if (!Directory.Exists(fullPath)) return;

            // https://answers.unity.com/questions/432655/loading-texture-file-from-pngjpg-file-on-disk.html
            foreach (string filePath in Directory.GetFiles(fullPath))
            {
                byte[] fileData = File.ReadAllBytes(filePath);
                Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, false)
                {
                    anisoLevel = 1,
                    filterMode = FilterMode.Point
                };
                texture.LoadImage(fileData);
                Futile.atlasManager.LoadAtlasFromTexture(Path.GetFileNameWithoutExtension(filePath), texture, false);
            }
        }
    }
}
