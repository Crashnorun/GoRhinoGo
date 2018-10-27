using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
class TextureProcessor : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        TextureImporter importer = (TextureImporter)assetImporter;

        Object asset = AssetDatabase.LoadAssetAtPath(importer.assetPath, typeof(Texture2D));

        if (asset)
        {
            Debug.Log("Re-importing old texture at: " + assetPath);
        }
        else
        {
            Debug.Log("Importing new texture at: " + assetPath);
        }

        if (assetPath.Contains("Panorama")) {
            if (assetPath.Contains("LeftEye") || assetPath.Contains("RightEye"))
            {
                importer.textureShape = TextureImporterShape.TextureCube;
                importer.wrapMode = TextureWrapMode.Clamp;
                //importer.compressionQuality = 100;
                importer.filterMode = FilterMode.Bilinear;
                importer.mipmapEnabled = false;
                //importer.npotScale = TextureImporterNPOTScale.ToLarger;

            }
            else 
            {
                Material material = (Material) AssetDatabase.LoadAssetAtPath(Path.GetDirectoryName(assetPath) + "/" + Path.GetFileNameWithoutExtension(assetPath) + ".mat", typeof(Material));

                if (!material)
                    material = new Material(Shader.Find("Skybox/CubemapStereo"));

                Texture texRight = (Texture) AssetDatabase.LoadAssetAtPath(Path.GetDirectoryName(assetPath) + "/" + Path.GetFileNameWithoutExtension(assetPath) + " RightEye.jpg", typeof(Texture));
                Texture texLeft = (Texture) AssetDatabase.LoadAssetAtPath(Path.GetDirectoryName(assetPath) + "/" + Path.GetFileNameWithoutExtension(assetPath) + " LeftEye.jpg", typeof(Texture));

                if(texRight)
                    material.SetTexture("_TexRight", texRight);
                if(texLeft)
                    material.SetTexture("_TexLeft", texLeft);

                AssetDatabase.CreateAsset(material, Path.GetDirectoryName(assetPath) + "/" + Path.GetFileNameWithoutExtension(assetPath) + ".mat");

                // Print the path of the created asset
                Debug.Log(AssetDatabase.GetAssetPath(material));
            }
        }
    }
}
#endif