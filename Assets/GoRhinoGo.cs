using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
public class MenuItems
{
    [MenuItem("GoRhinoGo/Launch Grasshopper")]
    private static void NewMenuOption()
    {
        PlayerPrefs.DeleteAll();
    }
}
#endif