using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
using LDraw;

/// This component listens for images detected by the <c>XRImageTrackingSubsystem</c>
/// and overlays some information as well as the source Texture2D on top of the
/// detected image.
/// </summary>
[RequireComponent(typeof(ARTrackedImageManager))]
public class TrackedInstructionManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The camera to set on the world space UI canvas for each instantiated image info.")]
    Camera worldSpaceCanvasCamera;

    /// <summary>
    /// The prefab has a world space UI canvas,
    /// which requires a camera to function properly.
    /// </summary>
    public Camera WorldSpaceCanvasCamera
    {
        get { return worldSpaceCanvasCamera; }
        set { worldSpaceCanvasCamera = value; }
    }

    [SerializeField]
    [Tooltip("If an image is detected but no source texture can be found, this texture is used instead.")]
    Texture2D defaultTexture;

    /// <summary>
    /// If an image is detected but no source texture can be found,
    /// this texture is used instead.
    /// </summary>
    public Texture2D DefaultTexture
    {
        get { return defaultTexture; }
        set { defaultTexture = value; }
    }

    ARTrackedImageManager trackedImageManager;

    ARTrackedImage currentInsruction = null;

    // TODO tooltops etc...

    public GameObject modelPrefab;

    public Vector3 modelOffset = new Vector3(0f, 0.01f, 0f);

    public Vector3 rotationOffset = new Vector3(0f, 0f, 180f);

    public float modelScale = 0.0005f;

    GameObject model = null;

    Stepper stepper = null;

    Dictionary<int, string[]> stepMapping = new Dictionary<int, string[]>()
    {
        { 7, new string[] { "1.4", "1.5" } },
        { 9, new string[] { "1.8", "1.9" } },
        { 11, new string[] { "2.1", "2.2", "2.3" } },
        { 13, new string[] { "2.6", "2.7" } },
        { 15, new string[] { "2.9.1" } },
        { 17, new string[] { "2.10.3", "2.10.4", "2.10.5", "2.10.6" } },
        { 19, new string[] { "2.13" } },
        { 21, new string[] { "2.16" } },
        { 23, new string[] { "2.18" } },
        { 25, new string[] { "2.20" } },
        { 27, new string[] { "2.22" } },
        { 29, new string[] { "2.24" } },
        { 31, new string[] { "2.25.5" } },
        { 33, new string[] { "2.26.5" } },
        { 35, new string[] { "2.27" } },
        { 37, new string[] { "2.29" } },
        { 39, new string[] { "2.31" } },
        { 41, new string[] { "2.33" } },
        { 43, new string[] { "2.35", "2.36", "2.37" } },
        { 45, new string[] { "2.40.3", "2.40.4", "2.40.5", "2.41.1", "2.41.3" } },
        { 47, new string[] { "2.43", "2.44", "2.45", "2.46" } },
        { 49, new string[] { "2.48.3", "2.48.4", "2.49.1", "2.49.3" } },
        { 51, new string[] { "2.51" } },
        { 53, new string[] { "2.53" } },
        { 55, new string[] { "2.55.1", "2.55.2", "2.55.3", "2.55.4", "2.55.5" } },
        { 57, new string[] { "2.56" } },
        { 59, new string[] { "2.58" } },
        { 61, new string[] { "2.60", "2.61", "2.62" } },
        { 63, new string[] { "2.64.1", "2.64.3" } },
        { 65, new string[] { "2.66" } }
    };

    void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void Update()
    {
        if (model != null && 
            currentInsruction != null && 
            currentInsruction.trackingState == TrackingState.Tracking)
        {
            model.transform.rotation = currentInsruction.transform.rotation * Quaternion.Euler(rotationOffset);
            model.transform.position = currentInsruction.transform.position + modelOffset;
        }
    }

    void UpdateInstruction(ARTrackedImage trackedImage)
    {
        var planeParentGo = trackedImage.transform.GetChild(0).gameObject;
        var planeGo = planeParentGo.transform.GetChild(0).gameObject;
        var canvasGroup = trackedImage.GetComponentInChildren<CanvasGroup>();

        // Disable/Enable the visuals based on if this is the current instruction and it is tracked
        if (trackedImage == currentInsruction && trackedImage.trackingState == TrackingState.Tracking)
        {
            planeGo.SetActive(true);
            canvasGroup.alpha = 1f;

            // The image extents is only valid when the image is being tracked
            trackedImage.transform.localScale = new Vector3(trackedImage.size.x, 1f, trackedImage.size.y);

            // Set the texture
            var material = planeGo.GetComponentInChildren<MeshRenderer>().material;
            material.mainTexture = (trackedImage.referenceImage.texture == null) ? DefaultTexture : trackedImage.referenceImage.texture;
        }
        else
        {
            planeGo.SetActive(false);
            canvasGroup.alpha = 0f;
        }
    }

    void UpdateModel(ARTrackedImage trackedImage)
    {
        int imageId = int.Parse(trackedImage.referenceImage.name);
        if (imageId == 1 && model == null)
        {
            model = Instantiate(modelPrefab);
            model.transform.localScale = Vector3.one * modelScale;

            stepper = model.GetComponent<Stepper>();
        }

        else if (imageId == 5 && model != null)
        {
            stepper.ClearSteps();
        }

        else if (stepper != null && stepMapping.ContainsKey(imageId))
        {
            stepper.GoToStep(stepMapping[imageId][0]);
        }
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            currentInsruction = trackedImage;

            // Set the world space camera on the canvas
            var canvas = trackedImage.GetComponentInChildren<Canvas>();
            canvas.worldCamera = WorldSpaceCanvasCamera;

            // Give the initial image a reasonable default scale
            trackedImage.transform.localScale = new Vector3(0.01f, 1f, 0.01f);

            UpdateModel(trackedImage);

            UpdateInstruction(trackedImage);

            Debug.Log("Added: " + trackedImage.name);

            break;
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            UpdateInstruction(trackedImage);
        }

        foreach (var trackedImage in eventArgs.removed)
        {
            Debug.Log("Removed: " + trackedImage.name);
        }
    }
}
