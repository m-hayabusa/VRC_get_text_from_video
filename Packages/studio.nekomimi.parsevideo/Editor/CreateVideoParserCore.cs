using UnityEngine;
using UnityEditor;

namespace nekomimiStudio.video2String
{
    public class CreateVideoParserCore
    {
        [MenuItem("GameObject/nekomimiStudio/VideoParser", false, 10)]
        public static void Create(MenuCommand menu)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Packages/studio.nekomimi.parsevideo/Runtime/nSVideoParser.prefab");
            GameObject res = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
            GameObjectUtility.SetParentAndAlign(res, (GameObject)menu.context);
            Undo.RegisterCreatedObjectUndo(res, "VideoParserCore");
            Selection.activeObject = res;
        }
    }
}