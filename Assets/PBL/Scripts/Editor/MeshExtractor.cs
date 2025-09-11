using UnityEngine;
using UnityEditor;
using System.IO;

public class MeshExtractor
{
    [MenuItem("Assets/Extract Mesh", true)]
    private static bool Validate()
    {
        return Selection.activeObject is Mesh;
    }

    [MenuItem("Assets/Extract Mesh")]
    private static void Extract()
    {
        Mesh mesh = Selection.activeObject as Mesh;
        if (mesh == null) return;

        // Fixed folder
        string folder = "Assets/PBL/Meshes";
        if (!AssetDatabase.IsValidFolder(folder))
        {
            AssetDatabase.CreateFolder("Assets/PBL", "Meshes");
        }

        // Prompt only for file name
        string suggestedName = mesh.name;
        string fakePath = EditorUtility.SaveFilePanelInProject(
            "Choose Mesh File Name",
            suggestedName,
            "asset",
            "Enter a file name (folder will be ignored)");

        if (string.IsNullOrEmpty(fakePath))
            return;

        // Extract just the filename (ignore user’s folder)
        string fileName = Path.GetFileName(fakePath);

        // Force into fixed folder
        string path = Path.Combine(folder, fileName);

        // Ensure unique path
        path = AssetDatabase.GenerateUniqueAssetPath(path);

        // Create asset
        Mesh meshCopy = Object.Instantiate(mesh);
        meshCopy.name = Path.GetFileNameWithoutExtension(fileName);
        AssetDatabase.CreateAsset(meshCopy, path);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Extracted mesh saved to: {path}");
    }
}