using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FGUFW.Editor
{
    public static class RemoveMissingComp
    {
        private const string MenuPath = "GameObject/Remove Missing Scripts";

        [MenuItem(MenuPath)]
        private static void Execute()
        {
            var roots = GetTargetRoots();
            var removedCount = 0;

            foreach (var root in roots)
            {
                Undo.RegisterFullObjectHierarchyUndo(root, "Remove Missing Scripts");
                removedCount += RemoveMissingScripts(root);
            }

            if (removedCount > 0)
            {
                AssetDatabase.SaveAssets();
            }

            Debug.Log($"Removed {removedCount} missing script component(s).");
        }

        private static List<GameObject> GetTargetRoots()
        {
            var selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length == 0)
            {
                var sceneRoots = new List<GameObject>();
                foreach (var gameObject in Object.FindObjectsByType<GameObject>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None))
                {
                    if (gameObject.transform.parent == null && gameObject.scene.IsValid())
                    {
                        sceneRoots.Add(gameObject);
                    }
                }

                return sceneRoots;
            }

            var selectedSet = new HashSet<GameObject>(selectedObjects);
            var roots = new List<GameObject>();
            foreach (var selectedObject in selectedObjects)
            {
                var parent = selectedObject.transform.parent;
                var hasSelectedAncestor = false;
                while (parent != null)
                {
                    if (selectedSet.Contains(parent.gameObject))
                    {
                        hasSelectedAncestor = true;
                        break;
                    }

                    parent = parent.parent;
                }

                if (!hasSelectedAncestor)
                {
                    roots.Add(selectedObject);
                }
            }

            return roots;
        }

        private static int RemoveMissingScripts(GameObject gameObject)
        {
            var removedCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
            if (removedCount > 0)
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
                EditorUtility.SetDirty(gameObject);
            }

            foreach (Transform child in gameObject.transform)
            {
                removedCount += RemoveMissingScripts(child.gameObject);
            }

            return removedCount;
        }
    }
}
