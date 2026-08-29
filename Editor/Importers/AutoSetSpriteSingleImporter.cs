using UnityEditor;
using UnityEngine;
using System.IO;

public class AutoSetSpriteSingleImporter : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        var importer = (TextureImporter)assetImporter;
        if (importer.textureType == TextureImporterType.Sprite)
        {
            string metaPath = assetPath + ".meta";
            // Chỉ xử lý nếu meta file vừa được tạo (ảnh mới import)
            if (!File.Exists(metaPath) || File.GetLastWriteTime(metaPath) == File.GetCreationTime(metaPath))
            {
                Texture2D tempTex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                if (tempTex != null)
                {
                    int width = tempTex.width;
                    int height = tempTex.height;

                    int newWidth = ((width + 3) / 4) * 4;
                    int newHeight = ((height + 3) / 4) * 4;

                    if (importer.maxTextureSize < Mathf.Max(newWidth, newHeight))
                        importer.maxTextureSize = Mathf.Max(newWidth, newHeight);
                }

                importer.spriteImportMode = SpriteImportMode.Single;
            }
        }
    }
}