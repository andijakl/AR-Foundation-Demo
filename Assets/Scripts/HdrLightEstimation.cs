using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(Light))]
public class HdrLightEstimation : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The ARCameraManager which will produce frame events containing light estimation information.")]
    private ARCameraManager _cameraManager;

    /// <summary>
    /// Reference to the main directional light of the scene.
    /// It is assumed that this script is placed on the main directional light GameObject.
    /// </summary>
    private Light _light;


    void Awake()
    {
        _light = GetComponent<Light>();
    }

    void OnEnable()
    {
        if (_cameraManager != null)
            _cameraManager.frameReceived += CameraFrameChanged;
    }

    void OnDisable()
    {
        if (_cameraManager != null)
            _cameraManager.frameReceived -= CameraFrameChanged;
    }

    private void CameraFrameChanged(ARCameraFrameEventArgs args)
    {
        if (args.lightEstimation.averageBrightness.HasValue)
        {
            _light.intensity = args.lightEstimation.averageBrightness.Value;
        }

        if (args.lightEstimation.averageColorTemperature.HasValue)
        {
            _light.colorTemperature = args.lightEstimation.averageColorTemperature.Value;
        }

        if (args.lightEstimation.colorCorrection.HasValue)
        {
            _light.color = args.lightEstimation.colorCorrection.Value;
        }

        if (args.lightEstimation.mainLightDirection.HasValue)
        {
            var mainLightDirection = args.lightEstimation.mainLightDirection;
            _light.transform.rotation = Quaternion.LookRotation(mainLightDirection.Value);
        }

        if (args.lightEstimation.mainLightColor.HasValue)
        {
            // Could overwrite colorCorrection if that was available
            // (this value is usually the better choice)
            _light.color = (Color) args.lightEstimation.mainLightColor;
        }

        if (args.lightEstimation.mainLightIntensityLumens.HasValue)
        {
            // Could overwrite averageBrightness if that was available
            // (this value is usually the better choice)
            _light.intensity = (float) args.lightEstimation.mainLightIntensityLumens;
        }

        if (args.lightEstimation.ambientSphericalHarmonics.HasValue)
        {
            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientProbe = args.lightEstimation.ambientSphericalHarmonics.Value;
        }
    }
}
