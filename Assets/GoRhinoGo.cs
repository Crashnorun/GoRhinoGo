using UnityEngine;
using UnityEditor;

public class MenuItems
{
    [MenuItem("GoRhinoGo/Launch Grasshopper")]
    private static void NewMenuOption()
    {
        PlayerPrefs.DeleteAll();
    }
}