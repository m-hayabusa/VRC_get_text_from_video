using UnityEngine;
using UnityEditor;

public class CreateVideoParserCore
{
    [MenuItem("GameObject/nSVideoParser", false, 10)]
    public static void Create(MenuCommand menu)
    {
        Object[] prefab = AssetDatabase.LoadAllAssetsAtPath("Packages/studio.nekomimi.parsevideo/Runtime/nSVideoParser.prefab");
        if (prefab.Length < 1) return;

        GameObject res = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab[0]);
        GameObjectUtility.SetParentAndAlign(res, (GameObject) menu.context);
        Undo.RegisterCreatedObjectUndo(res, "VideoParserCore");
        Selection.activeObject = res;
    }
}