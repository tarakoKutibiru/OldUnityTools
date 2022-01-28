﻿using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// A collection of useful shortcuts for manipulating the hierarchy.
/// Some of this was written by me, some of it was collected from random Unity
/// forums and StackOverflow posts.
/// </summary>
public class CustomMenuItems {
    [MenuItem("GameObject/Hierarchy/Select First Child Each", false, 10)]
    private static void SelectChildren() {
        Object[] newSelection = Selection.gameObjects.Select(obj => obj.transform.childCount > 0 ? obj.transform.GetChild(0).gameObject : obj).Cast<Object>().ToArray();
        Debug.Log(newSelection.Length);
        Selection.objects = newSelection;
    }

    [MenuItem("GameObject/Hierarchy/Collapse All [Alt + Q] &q", false, -10)]
    private static void CollapseAll() {
        foreach (GameObject obj in SceneRoots()) {
            SetExpandedRecursive(obj, false);
        }
    }

    [MenuItem("GameObject/Hierarchy/Un-Parent And Collapse All %q", false, 0)]
    private static void UnparentAndCollapse() {
        foreach (GameObject obj in Selection.gameObjects) {
            obj.transform.parent = null;
        }
        CollapseAll();
    }

    [MenuItem("GameObject/Hierarchy/Place Selection In New Container #%q", false, -5)]
    private static void PlaceInNewContainer() {
        if (Selection.gameObjects.Length <= 0) {
            return;
        }

        GameObject container = new GameObject("GameObject");

        foreach(var gameObj in GetRootSelectedGameObjects()) {
            gameObj.transform.parent = container.transform;
        }

        Selection.objects = new Object[] { };
        CollapseAll();
    }

    private static IEnumerable<GameObject> GetRootSelectedGameObjects() {
        foreach (GameObject obj in Selection.gameObjects) {
            // Search up through the objects parents.
            bool isChild = false;
            Transform nextParent = obj.transform.parent;
            while (nextParent != null) {
                if (Selection.gameObjects.Any(o => o.gameObject == nextParent.gameObject)) {
                    isChild = true;
                    break;
                }
                nextParent = nextParent.transform.parent;
            }

            if (!isChild) {
                yield return obj;
            }
        }
    }

    [MenuItem("GameObject/Organize/Decor", false, -5)]
    private static void PlaceInDecor() {
        PlaceInObject("Decor");
    }

    [MenuItem("GameObject/Organize/Background", false, -5)]
    private static void PlaceInBackground() {
        PlaceInObject("Background");
    }

    [MenuItem("GameObject/Organize/Buildings", false, -5)]
    private static void PlaceInBuildings() {
        PlaceInObject("Buildings");
    }

    [MenuItem("GameObject/Organize/Structures", false, -5)]
    private static void PlaceInStructures() {
        PlaceInObject("Structures");
    }

    [MenuItem("GameObject/Organize/LevelObjects", false, -5)]
    private static void PlaceInLevelObjects() {
        PlaceInObject("LevelObjects");
    }

    private static void PlaceInObject(string name) {
        if (Selection.gameObjects.Length <= 0) {
            return;
        }
        var gameObj = GameObject.Find(name);
        if (!gameObj) {
            gameObj = new GameObject(name);
            return;
        }

        foreach (GameObject obj in Selection.gameObjects) {
            obj.transform.parent = gameObj.transform;
        }
    }

    #region CodeFromWeb

    [MenuItem("GameObject/Hierarchy/Sort Children By Name &#%q", false, 1)]
    public static void SortGameObjectsByName(MenuCommand menuCommand) {
        if (menuCommand.context == null || menuCommand.context.GetType() != typeof(GameObject)) {
            EditorUtility.DisplayDialog("Error", "You must select an item to sort in the frame", "Okay");
            return;
        }

        GameObject parentObject = (GameObject)menuCommand.context;

        // Build a list of all the Transforms in this player's hierarchy
        Transform[] objectTransforms = new Transform[parentObject.transform.childCount];
        for (int i = 0; i < objectTransforms.Length; i++)
            objectTransforms[i] = parentObject.transform.GetChild(i);

        int sortTime = System.Environment.TickCount;

        bool sorted = false;
        // Perform a bubble sort on the objects
        while (sorted == false) {
            sorted = true;
            for (int i = 0; i < objectTransforms.Length - 1; i++) {
                // Compare the two strings to see which is sooner
                int comparison = objectTransforms[i].name.CompareTo(objectTransforms[i + 1].name);

                if (comparison > 0) // 1 means that the current value is larger than the last value
                {
                    objectTransforms[i].transform.SetSiblingIndex(objectTransforms[i + 1].GetSiblingIndex());
                    sorted = false;
                }
            }

            // resort the list to get the new layout
            for (int i = 0; i < objectTransforms.Length; i++)
                objectTransforms[i] = parentObject.transform.GetChild(i);
        }

        Debug.Log("Sort took " + (System.Environment.TickCount - sortTime) + " milliseconds");
    }

    public static IEnumerable<GameObject> SceneRoots() {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
    }

    public static void Collapse(GameObject gameObject, bool collapse = true) {
        // Bail out immediately if the go doesn't have children.
        if (gameObject.transform.childCount == 0) return;
        // Get a reference to the hierarchy window.
        var hierarchy = GetFocusedWindow("Hierarchy");
        // Select our GameObject.
        SelectObject(gameObject);
        // Create a new key event (RightArrow for collapsing, LeftArrow for folding)
        var key = new Event { keyCode = collapse ? KeyCode.RightArrow : KeyCode.LeftArrow, type = EventType.KeyDown };
        // Finally, send the window the event.
        hierarchy.SendEvent(key);
    }

    public static void SetExpandedRecursive(GameObject go, bool expand) {
        var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
        var methodInfo = type.GetMethod("SetExpandedRecursive");

        EditorApplication.ExecuteMenuItem("Window/Hierarchy");
        var window = EditorWindow.focusedWindow;

        methodInfo.Invoke(window, new object[] { go.GetInstanceID(), expand });
    }

    public static void SelectObject(Object obj) {
        Selection.activeObject = obj;
    }

    public static EditorWindow GetFocusedWindow(string window) {
        FocusOnWindow(window);
        return EditorWindow.focusedWindow;
    }

    public static void FocusOnWindow(string window) {
        EditorApplication.ExecuteMenuItem("Window/" + window);
    }

    #endregion
}