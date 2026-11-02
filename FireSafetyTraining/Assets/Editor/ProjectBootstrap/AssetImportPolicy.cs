using UnityEditor;

namespace FireSafety.EditorTools
{
    // Keeps imported source art within the laptop texture budget. Originals stay intact.
    public sealed class AssetImportPolicy : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/ThirdParty/")) return;
            var importer = (TextureImporter)assetImporter;
            string lower = assetPath.ToLowerInvariant();
            bool normal = lower.Contains("_normal") || lower.Contains("_nor_gl");
            importer.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = !normal && (lower.Contains("basecolor") || lower.Contains("basemap") || lower.Contains("_diff"));
            importer.maxTextureSize = 2048;
            importer.mipmapEnabled = true;
            importer.isReadable = false;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.anisoLevel = 4;
        }

        void OnPreprocessModel()
        {
            if (!assetPath.StartsWith("Assets/ThirdParty/")) return;
            var importer = (ModelImporter)assetImporter;
            importer.importCameras = false;
            importer.importLights = false;
            bool schoolCharacter=assetPath.StartsWith("Assets/ThirdParty/SchoolExpansion/Characters/");
            importer.importAnimation = schoolCharacter;
            if(schoolCharacter) importer.animationType=ModelImporterAnimationType.Legacy;
            importer.isReadable = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
        }
    }
}
