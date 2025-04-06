using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class ScreenshotToolWindow : EditorWindow
{
    private DeviceInfoCollection deviceInfoCollection;
    private List<bool> toggles;
    private Vector2 scrollPos;

    [MenuItem("Window/Screenshot Tool")]
    public static void ShowWindow()
    {
        GetWindow<ScreenshotToolWindow>("Screenshot Tool");
    }

    private void OnEnable()
    {
        deviceInfoCollection = FindDeviceInfoCollection();
        if (deviceInfoCollection == null)
        {
            Debug.LogError("DeviceInfoCollection not found in the project.");
            return;
        }

		UpdateTogglesList();	
    }

	public void UpdateTogglesList()
	{
    	toggles = new List<bool>(new bool[deviceInfoCollection.DeviceInfos.Count]);
	}

    private DeviceInfoCollection FindDeviceInfoCollection()
    {
        string[] guids = AssetDatabase.FindAssets("t:DeviceInfoCollection");
        if (guids.Length == 0)
        {
            return null;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<DeviceInfoCollection>(path);
    }

    private void OnGUI()
    {
        GUILayout.Label("Screenshot Tool", EditorStyles.boldLabel);

        if (GUILayout.Button("Select All"))
        {
            SetAllToggles(true);
        }

        if (GUILayout.Button("Remove All"))
        {
            SetAllToggles(false);
        }

        if (GUILayout.Button("Add New Screen"))
        {
            AddDeviceInfoWindow.ShowWindow(deviceInfoCollection, this);
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        for (int i = 0; i < deviceInfoCollection.DeviceInfos.Count; i++)
        {
            toggles[i] = EditorGUILayout.ToggleLeft(deviceInfoCollection.DeviceInfos[i].DeviceName, toggles[i]);
        }
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Take Screenshots"))
        {
            TakeScreenshots();
        }
    }
    private void SetAllToggles(bool value)
    {
        for (int i = 0; i < toggles.Count; i++)
        {
            toggles[i] = value;
        }
    }

private void TakeScreenshots()
{
    string prefabPath = "Packages/com.muratkacmaz.screenshottool/Runtime/ScreenshotToolPanel.prefab";
    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

    if (prefab != null)
    {
        GameObject panelObject = Instantiate(prefab);
        ScreenshotToolPanel screenshotToolPanel = panelObject.GetComponent<ScreenshotToolPanel>();

        if (screenshotToolPanel != null)
        {
			var deviceInfos = new List<DeviceInfo>();
			for(int i = 0; i < toggles.Count; i++)
			{
				if(toggles[i])
                {
                    deviceInfos.Add(deviceInfoCollection.DeviceInfos[i]);
                }
			}
			
            screenshotToolPanel.OnTakeScreenshotsClick(deviceInfos);
        }
        else
        {
            Debug.LogError("ScreenshotToolPanel component not found on the instantiated prefab.");
        }
    }
    else
    {
        Debug.LogError("ScreenshotToolPanel prefab not found in Resources.");
    }
}
}