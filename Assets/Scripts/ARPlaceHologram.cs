using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem.EnhancedTouch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

[RequireComponent(typeof(ARAnchorManager))]
[RequireComponent(typeof(ARRaycastManager))]
[RequireComponent(typeof(ARPlaneManager))]
public class ARPlaceHologram : MonoBehaviour
{
    /// <summary>
    /// The prefab to instantiate on touch.
    /// </summary>
    [SerializeField]
    private GameObject _prefabToPlace;

    // Cache ARRaycastManager GameObject from XROrigin
    private ARRaycastManager _raycastManager;

    // Cache ARAnchorManager GameObject from XROrigin
    private ARAnchorManager _anchorManager;

    // Cache ARAnchorManager GameObject from XROrigin
    //private ARPlaneManager _planeManager;

    // List for raycast hits is re-used by raycast manager
    private static readonly List<ARRaycastHit> Hits = new();

    // Reference to logging UI element in the canvas
    public UnityEngine.UI.Text Log;

    protected void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    protected void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    protected void Awake()
    {
        _raycastManager = GetComponent<ARRaycastManager>();
        _anchorManager = GetComponent<ARAnchorManager>();
        //_planeManager = GetComponent<ARPlaneManager>();
    }
    
    protected void Update()
    {
        // Only consider single-finger touches that are beginning
        var activeTouches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;
        if (activeTouches.Count < 1 || activeTouches[0].phase != TouchPhase.Began)
        {
            return;
        }

        // Raycast against planes and feature points
        const TrackableType trackableTypes =
            TrackableType.FeaturePoint |
            TrackableType.PlaneWithinPolygon;

        // Perform AR raycast to any kind of trackable
        if (_raycastManager.Raycast(activeTouches[0].screenPosition, Hits, trackableTypes))
        {
            // Raycast hits are sorted by distance, so the first one will be the closest hit.
            //var hitPose = Hits[0].pose;

            // Note: simply instantiating the prefab at the given position
            // wouldn't anchor it. Over time, it wouldn't stay in place
            // in the real world.
            //Instantiate(_prefabToPlace, hitPose.position, hitPose.rotation);

            // Therefore: create an anchor so that the object stays
            // in place in the real world.
            CreateAnchor(Hits[0]);

            // Debug output what we actually hit
            //Log.text = $"Instantiated on: {Hits[0].hitType}";
            //Debug.Log($"Instantiated on: {Hits[0].hitType}");
        }
    }

    
    private ARAnchor CreateAnchor(in ARRaycastHit hit)
    {
        ARAnchor anchor;

        // Get the trackable ID in case the raycast hit a trackable
        //var hitTrackableId = hit.trackableId;

        // Attempt to retrieve a plane if the trackable is of type plane
        // and the the raycast hit one
        //var hitPlane = _planeManager.GetPlane(hitTrackableId);

        if (hit.trackable is ARPlane hitPlane)
        {
            // The raycast hit a plane - therefore, attach the anchor to the plane.
            // According to the AR Foundation documentation:
            // Attaching an anchor to a plane affects the anchor update semantics.
            // This type of anchor only changes its position along the normal of
            // the plane to which it is attached,
            // thus maintaining a constant distance from the plane.

            // When the Anchor Manager has a prefab assigned to its property,
            // it will instantiate that and automatically make it a child
            // of an anchor GameObject.
            // The following code temporarily replaces the default prefab
            // with the one we want to instantiate from our script, to ensure
            // it doesn't interfere with potential other logic in your app.
            var oldPrefab = _anchorManager.anchorPrefab;
            _anchorManager.anchorPrefab = _prefabToPlace;
            anchor = _anchorManager.AttachAnchor(hitPlane, hit.pose);
            _anchorManager.anchorPrefab = oldPrefab;

            // Note: the following method seems to produce an offset when placing
            // the prefab instance in AR Foundation 5.0 pre 8
            //anchor = _anchorManager.AttachAnchor(hitPlane, hit.pose);
            // Make our prefab a child of the anchor, so that it's moved
            // with that anchor.
            //Instantiate(_prefabToPlace, anchor.transform);

            Debug.Log($"Created anchor attachment for plane (id: {anchor.nativePtr}).");
        }
        else
        {
            // Otherwise, just create a regular anchor at the hit pose
            // Note: the anchor can be anywhere in the scene hierarchy
            var instantiatedObject = Instantiate(_prefabToPlace, hit.pose.position, hit.pose.rotation);

            // Make sure the new GameObject has an ARAnchor component.
            if (!instantiatedObject.TryGetComponent<ARAnchor>(out anchor))
            {
                // If the prefab doesn't include the ARAnchor component,
                // simply add it.
                // Note: ARAnchorManager.AddAnchor() is obsolete, this
                // is the way to go! ARAnchor will add itself to the
                // anchor manager once it is enabled.
                anchor = instantiatedObject.AddComponent<ARAnchor>();
            }
            Debug.Log($"Created regular anchor (id: {anchor.nativePtr}).");
        }

        return anchor;
    }
}
