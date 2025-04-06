using UnityEditor;
using UnityEngine;

public class AddDeviceInfoWindow : EditorWindow
{
    private string deviceName;
    private int width;
    private int height;
    private float safeAreaWidth;
    private float safeAreaHeight;
    private float safeAreaYMax;
    private DeviceInfoCollection deviceInfoCollection;
    private ScreenshotToolWindow screenshotToolWindow;

    public static void ShowWindow(DeviceInfoCollection collection, ScreenshotToolWindow window)
    {
        var addWindow = GetWindow<AddDeviceInfoWindow>("Add New Screen");
        addWindow.deviceInfoCollection = collection;
        addWindow.screenshotToolWindow = window;
    }

    private void OnGUI()
    {
        GUILayout.Label("Add New Screen", EditorStyles.boldLabel);

        deviceName = EditorGUILayout.TextField("Device Name", deviceName);
        width = EditorGUILayout.IntField("Width", width);
        height = EditorGUILayout.IntField("Height", height);
        safeAreaWidth = EditorGUILayout.FloatField("Safe Area Width", safeAreaWidth);
        safeAreaHeight = EditorGUILayout.FloatField("Safe Area Height", safeAreaHeight);
        safeAreaYMax = EditorGUILayout.FloatField("Safe Area YMax", safeAreaYMax);

        if (GUILayout.Button("Add Device Info"))
        {
            AddDeviceInfo();
        }
    }

    private void AddDeviceInfo()
    {
        var newDeviceInfo = new DeviceInfo(deviceName, width, height, safeAreaWidth, safeAreaHeight, safeAreaYMax);
        deviceInfoCollection.DeviceInfos.Add(newDeviceInfo);
        EditorUtility.SetDirty(deviceInfoCollection);
        screenshotToolWindow.UpdateTogglesList();
        Close();
    }
}