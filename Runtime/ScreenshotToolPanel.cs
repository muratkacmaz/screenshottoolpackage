using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using UnityEngine;
using UnityEngine.UI;


public class ScreenshotToolPanel : MonoBehaviour
{
    [SerializeField] private Image Border;

    private string _folderPath => Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/Screenshots/";
    private ContentSizeFitter[] _fitters;
    private List<DeviceInfo> _deviceInfos = new List<DeviceInfo>();

    private int _initWidth;
    private int _initHeight;
    private float _safeAreaYMax;

#if UNITY_EDITOR
    private RecorderController _recorderController;
    private RecorderControllerSettings _controllerSettings;


    private void Awake()
    {
        _controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        _recorderController = new RecorderController(_controllerSettings);
    }

    public void OnTakeScreenshotsClick(List<DeviceInfo> deviceInfos)
    {
        _deviceInfos = deviceInfos;
        SetInitialValues();
    }

    private void SetInitialValues()
    {
        _initWidth = Screen.width;
        _initHeight = Screen.height;
        _safeAreaYMax = Screen.safeArea.yMax;
        _fitters = FindObjectsOfType<ContentSizeFitter>();
        StartCoroutine(CaptureScreenshots());
    }


    private IEnumerator CaptureScreenshots()
    {
        for (int i = 0; i < _deviceInfos.Count; i++)
        {
            var deviceInfo = _deviceInfos[i];
            SetScreenshotSettings(deviceInfo.Width, deviceInfo.Height, out var imageRecorder);
            _controllerSettings.AddRecorderSettings(imageRecorder);
            _controllerSettings.SetRecordModeToSingleFrame(0);
            SetScreenResolution(imageRecorder);
            ShowSafeAreaOutline(deviceInfo);

            yield return new WaitForSeconds(2f);

            yield return new WaitForSeconds(1f);

            UpdateAllFitters();
            yield return new WaitForSeconds(1f);

            var screenshotName =
                $"{deviceInfo.DeviceName}_{deviceInfo.Width}_{deviceInfo.Height}_{DateTime.Now:dd-MM-yyyy-HH-mm-ss}.png";
            CompleteScreenshot(imageRecorder, screenshotName);
            Debug.Log($"Screenshot taken for : {deviceInfo.DeviceName}_{deviceInfo.Width}_{deviceInfo.Height}");

            yield return new WaitForSeconds(2f);


            _controllerSettings.RemoveRecorder(imageRecorder);
        }

        BackToInitialValue();
    }

    private void BackToInitialValue()
    {
        Debug.Log($"Screenshots Completed");
        SetScreenshotSettings(_initWidth, _initHeight, out var imageRecorder);
        SetScreenResolution(imageRecorder);
        Destroy(gameObject);
    }

    private void ShowSafeAreaOutline(DeviceInfo deviceInfo)
    {
        var topDiff = (deviceInfo.Height - deviceInfo.SafeAreaYMax);
        ShowSafeArea(topDiff);
    }

    private void SetScreenshotSettings(int deviceWidth, int deviceHeight, out ImageRecorderSettings imageRecorder)
    {
        imageRecorder = ScriptableObject.CreateInstance<ImageRecorderSettings>();
        imageRecorder.name = Guid.NewGuid().ToString();
        imageRecorder.Enabled = true;
        imageRecorder.OutputFormat = ImageRecorderSettings.ImageRecorderOutputFormat.PNG;
        imageRecorder.CaptureAlpha = false;

        imageRecorder.imageInputSettings = new GameViewInputSettings
        {
            OutputWidth = deviceWidth,
            OutputHeight = deviceHeight,
        };
    }

    private void SetScreenResolution(ImageRecorderSettings imageRecorder)
    {
        var screenshotName = $"Dummy_{DateTime.Now:dd-MM-yyyy-HH-mm-ss}.png";
        CompleteScreenshot(imageRecorder, screenshotName);
    }

    private void CompleteScreenshot(ImageRecorderSettings imageRecorder, string screenshotName)
    {
        imageRecorder.OutputFile = Path.Combine(_folderPath, screenshotName);
        _recorderController.PrepareRecording();
        _recorderController.StartRecording();
    }

    private void UpdateAllFitters()
    {
        foreach (ContentSizeFitter fitter in _fitters)
        {
            if (fitter.enabled && fitter.gameObject.activeInHierarchy)
            {
                fitter.gameObject.SetActive(false);
                StartCoroutine(ActivateFitter(fitter));
            }
        }
    }

    private IEnumerator ActivateFitter(ContentSizeFitter fitter)
    {
        yield return new WaitForSeconds(0.1f);

        fitter.gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(fitter.GetComponent<RectTransform>());
    }

    public void ShowSafeArea(float topDiff)
    {
        var image = Border;
        image.enabled = true;
        var rectTransform = Border.GetComponent<RectTransform>();
        rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, -topDiff);
    }

#endif
}