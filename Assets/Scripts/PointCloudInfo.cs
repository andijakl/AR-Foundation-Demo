using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PointCloudInfo : MonoBehaviour
{
    // The AR Foundation PointCloud script
    private ARPointCloud _pointCloud;

    // Reference to logging UI element in the canvas
    public UnityEngine.UI.Text Log;

    public static int InstanceCount = 0;

    public PointCloudInfo()
    {
        InstanceCount++;
        Debug.Log("Instance created");
    }
    
    void OnEnable()
    {
        // Subscribe to event when point cloud changed
        _pointCloud = GetComponent<ARPointCloud>();
        _pointCloud.updated += OnPointCloudChanged;
    }

    void OnDisable()
    {
        // Unsubscribe event when this element is disabled
        _pointCloud.updated -= OnPointCloudChanged;
    }

    private void OnPointCloudChanged(ARPointCloudUpdatedEventArgs eventArgs)
    {
        if (!_pointCloud.positions.HasValue ||
            !_pointCloud.identifiers.HasValue ||
            !_pointCloud.confidenceValues.HasValue)
            return;

        var positions = _pointCloud.positions.Value;
        var identifiers = _pointCloud.identifiers.Value;
        var confidence = _pointCloud.confidenceValues.Value;

        if (positions.Length == 0) return;

        var logText = "Number of points: " + positions.Length + "\nPoint info: x = " 
                   + positions[0].x + ", y = " + positions[0].y + ", z = " + positions[0].z
                   + ",\n Identifier = " + identifiers[0] + ", Confidence = " + confidence[0];
        
        //Log.text = logText;
        Debug.Log(logText);
    }
}
