using UnityEngine;
using UnityEditor;

public class CreateVideoParserCore
{
    [MenuItem("GameObject/nSVideoParser", false, 10)]
    public static void Create(MenuCommand menu)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Packages/studio.nekomimi.parsevideo/Runtime/nSVideoParser.prefab");
        GameObject res = (GameObject) UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
        GameObjectUtility.SetParentAndAlign(res, (GameObject) menu.context);
        Undo.RegisterCreatedObjectUndo(res, "VideoParserCore");
        Selection.activeObject = res;
    }
}