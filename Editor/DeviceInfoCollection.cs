using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

[Serializable,
 CreateAssetMenu(fileName = nameof(DeviceInfoCollection),
     menuName = "Scriptable Objects/" + nameof(DeviceInfoCollection), order = 1)]
public class DeviceInfoCollection : ScriptableObject
{
    public List<DeviceInfo> DeviceInfos = new List<DeviceInfo>();

    private static Lazy<Assembly> DeviceSimulatorAssembly => new(() =>
        AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(assembly => assembly.GetName().Name == "UnityEditor.DeviceSimulatorModule"));

    private static Lazy<Type> SimulatorWindowType => new(() =>
        DeviceSimulatorAssembly.Value.GetType("UnityEditor.DeviceSimulation.SimulatorWindow"));

    private static Lazy<FieldInfo> SimulatorWindowMainField => new(() =>
        SimulatorWindowType.Value.GetField("m_Main", BindingFlags.Instance | BindingFlags.NonPublic));

    private static Lazy<Type> DeviceSimulatorMainType => new(() =>
        DeviceSimulatorAssembly.Value.GetType("UnityEditor.DeviceSimulation.DeviceSimulatorMain"));

    private static Lazy<Type> DeviceInfoAssetType => new(() =>
        DeviceSimulatorAssembly.Value.GetType("UnityEditor.DeviceSimulation.DeviceInfoAsset"));

    private static Lazy<Type> DeviceInfoType =>
        new(() => DeviceSimulatorAssembly.Value.GetType("UnityEditor.DeviceSimulation.DeviceInfo"));

    private static Lazy<Type> ScreenDataType =>
        new(() => DeviceSimulatorAssembly.Value.GetType("UnityEditor.DeviceSimulation.ScreenData"));

    private static Lazy<Type> OrientationDataType => new(() =>
        DeviceSimulatorAssembly.Value.GetType("UnityEditor.DeviceSimulation.OrientationData"));

    private static Lazy<FieldInfo> DeviceSimulatorMainDevicesField => new(() =>
        DeviceSimulatorMainType.Value.GetField("m_Devices", BindingFlags.Instance | BindingFlags.NonPublic));

    private static Lazy<FieldInfo> DeviceInfoAssetDeviceInfoField => new(() =>
        DeviceInfoAssetType.Value.GetField("deviceInfo", BindingFlags.Instance | BindingFlags.Public));

    private static Lazy<FieldInfo> DeviceInfoFriendlyNameField => new(() =>
        DeviceInfoType.Value.GetField("friendlyName", BindingFlags.Instance | BindingFlags.Public));

    private static Lazy<FieldInfo> DeviceScreenDataField => new(() =>
        DeviceInfoType.Value.GetField("screens", BindingFlags.Instance | BindingFlags.Public));

    private static Lazy<FieldInfo> DeviceOrientationDataField => new(() =>
        ScreenDataType.Value.GetField("orientations", BindingFlags.Instance | BindingFlags.Public));

    private static Lazy<FieldInfo> ScreenWidthField => new(() =>
        ScreenDataType.Value.GetField("width", BindingFlags.Instance | BindingFlags.Public));

    private static Lazy<FieldInfo> ScreenHeightField => new(() =>
        ScreenDataType.Value.GetField("height", BindingFlags.Instance | BindingFlags.Public));

    private static Lazy<FieldInfo> SafeAreaField => new(() =>
        OrientationDataType.Value.GetField("safeArea", BindingFlags.Instance | BindingFlags.Public));


    // Only works when simulator is on screen
    private void SetDeviceInfos()
    {
        DeviceInfos.Clear();
        ForActiveDeviceSimulatorMain(deviceSimulatorMain =>
        {
            var devices = DeviceSimulatorMainDevicesField.Value.GetValue(deviceSimulatorMain) as Array;

            foreach (var device in devices)
            {
                var deviceInfo = DeviceInfoAssetDeviceInfoField.Value.GetValue(device);
                var deviceName = DeviceInfoFriendlyNameField.Value.GetValue(deviceInfo).ToString();
                var screenDataArray = DeviceScreenDataField.Value.GetValue(deviceInfo) as Array;
                foreach (var screenData in screenDataArray)
                {
                    var width = ScreenWidthField.Value.GetValue(screenData);
                    var height = ScreenHeightField.Value.GetValue(screenData);

                    var orientationDataArray = DeviceOrientationDataField.Value.GetValue(screenData) as Array;
                    foreach (var orientationData in orientationDataArray)
                    {
                        var safeArea = SafeAreaField.Value.GetValue(orientationData);

                        Rect safeAreaRect = (Rect)safeArea;

                        DeviceInfos.Add(new DeviceInfo(deviceName, (int)width, (int)height, safeAreaRect.width,
                            safeAreaRect.height, safeAreaRect.yMax));
                        break;
                    }
                }

                Debug.Log(deviceInfo);
            }
        });
    }

    private void ForActiveDeviceSimulatorMain(Action<object> callback)
    {
        var simulatorWindow = Resources.FindObjectsOfTypeAll(SimulatorWindowType.Value).FirstOrDefault();
        if (simulatorWindow == null)
        {
            return;
        }

        var deviceSimulatorMain = SimulatorWindowMainField.Value.GetValue(simulatorWindow);
        if (deviceSimulatorMain != null)
        {
            callback(deviceSimulatorMain);
        }
    }
}


[Serializable]
public class DeviceInfo
{
    public string DeviceName;
    public int Width;
    public int Height;
    public float SafeAreaWidth;
    public float SafeAreaHeight;
    public float SafeAreaYMax;

    public DeviceInfo(string deviceName, int width, int height, float safeAreaWidth, float safeAreaHeight, float ymax)
    {
        DeviceName = deviceName;
        Width = width;
        Height = height;
        SafeAreaWidth = safeAreaWidth;
        SafeAreaHeight = safeAreaHeight;
        SafeAreaYMax = ymax;
    }
}