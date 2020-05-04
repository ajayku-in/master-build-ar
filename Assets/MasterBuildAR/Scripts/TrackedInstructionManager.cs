using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
using LDraw;

[RequireComponent(typeof(ARTrackedImageManager))]
[RequireComponent(typeof(ARRaycastManager))]
public class TrackedInstructionManager : MonoBehaviour
{
    // Constants

    const float MODEL_MOVEMENT_SPEED = 10f;
    const float MODEL_ROTATION_SPEED = 15f;

    static readonly Vector3 MODEL_BASE_SCALE = new Vector3(-1f, 1f, 1f);

    static readonly Dictionary<int, string> stepMapping = new Dictionary<int, string>()
    {
        { 7, "1.4" },
        { 9, "1.8" },
        { 11, "2.1" },
        { 13, "2.6" },
        { 15, "2.9.1" },
        { 17, "2.10.3" },
        { 19, "2.13" },
        { 21, "2.16" },
        { 23, "2.18" },
        { 25, "2.20" },
        { 27, "2.22" },
        { 29, "2.24" },
        { 31, "2.25.5" },
        { 33, "2.26.5" },
        { 35, "2.27" },
        { 37, "2.29" },
        { 39, "2.31" },
        { 41, "2.33" },
        { 43, "2.35" },
        { 45, "2.40.3" },
        { 47, "2.43" },
        { 49, "2.48.3" },
        { 51, "2.51" },
        { 53, "2.53" },
        { 55, "2.55.1" },
        { 57, "2.56" },
        { 59, "2.58" },
        { 61, "2.60" },
        { 63, "2.64.1" },
        { 65, "2.66" }
    };

    // Inspector fields

    [SerializeField]
    [Tooltip("The camera to set on the world space UI canvas for each instantiated image info.")]
    Camera worldSpaceCanvasCamera;

    [SerializeField]
    [Tooltip("If an image is detected but no source texture can be found, this texture is used instead.")]
    Texture2D defaultTexture;

    [SerializeField]
    [Tooltip("Model to instantiate when the first page of the instruction is detected.")]
    GameObject modelPrefab;

    [SerializeField]
    [Tooltip("Position offset of the model from the detected image.")]
    Vector3 modelOffset = new Vector3(0f, 0.01f, 0f);

    [SerializeField]
    [Tooltip("Rotation offset of the model from the detected image.")]
    Vector3 rotationOffset = new Vector3(0f, 0f, 180f);

    [SerializeField]
    [Tooltip("Scale of the model.")]
    float modelScale = 0.0005f;

    // Internal state

    ARTrackedImageManager trackedImageManager;

    ARTrackedImage currentInsruction = null;

    ARRaycastManager raycastManager;

    List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();

    GameObject model = null;

    Stepper stepper = null;

    // Unity lifecycle methods

    void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
        raycastManager = GetComponent<ARRaycastManager>();
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
            Quaternion targetRotation = currentInsruction.transform.rotation * Quaternion.Euler(rotationOffset);
            Vector3 targetPosition = currentInsruction.transform.position + modelOffset;
            model.transform.rotation = Quaternion.RotateTowards(model.transform.rotation, targetRotation, MODEL_ROTATION_SPEED * Time.deltaTime);
            model.transform.position = Vector3.MoveTowards(model.transform.position, targetPosition, MODEL_MOVEMENT_SPEED * Time.deltaTime);
        }

        if (stepper != null &&
            TryGetTouchPosition(out Vector2 touchPosition) &&
            raycastManager.Raycast(touchPosition, raycastHits, TrackableType.Image))
        {
            // Raycast hits are sorted by distance, so the first one
            // will be the closest hit.
            var hitPose = raycastHits[0].pose;

            // Advance to the next step
            stepper.NextStep();
        }
    }

    // Helper methods

    bool TryGetTouchPosition(out Vector2 touchPosition)
    {
#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            var mousePosition = Input.mousePosition;
            touchPosition = new Vector2(mousePosition.x, mousePosition.y);
            return true;
        }
#else
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began) {
                touchPosition = touch.position;
                return true;
            }
        }
#endif

        touchPosition = default;
        return false;
    }

    void SetupInstruction(ARTrackedImage trackedImage)
    {
        // Set the world space camera on the canvas
        var canvas = trackedImage.GetComponentInChildren<Canvas>();
        canvas.worldCamera = worldSpaceCanvasCamera;

        // Give the initial image a reasonable default scale
        trackedImage.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
    }

    void UpdateInstruction(ARTrackedImage trackedImage)
    {
        var planeParentGo = trackedImage.transform.GetChild(0).gameObject;
        var planeGo = planeParentGo.transform.GetChild(0).gameObject;
        var canvasGroup = trackedImage.GetComponentInChildren<CanvasGroup>();
        var text = trackedImage.GetComponentInChildren<Text>();

        // Disable/Enable the visuals based on if this is the current instruction and it is tracked
        if (trackedImage == currentInsruction && trackedImage.trackingState != TrackingState.None)
        {
            planeGo.SetActive(true);
            canvasGroup.alpha = 1f;

            // The image extents is only valid when the image is being tracked
            trackedImage.transform.localScale = new Vector3(trackedImage.size.x, trackedImage.size.y, trackedImage.size.y);

            // Set the texture
            var material = planeGo.GetComponentInChildren<MeshRenderer>().material;
            material.mainTexture = (trackedImage.referenceImage.texture == null) ? defaultTexture : trackedImage.referenceImage.texture;

            // Update the step number
            if (stepper != null)
            {
                text.text = "Step: " + stepper.GetCurrentStep().Number;
            }
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
            model.transform.localScale = MODEL_BASE_SCALE * modelScale;

            stepper = model.GetComponent<Stepper>();
        }

        else if (imageId == 5 && stepper != null)
        {
            stepper.ClearSteps();
        }

        else if (stepper != null && stepMapping.ContainsKey(imageId))
        {
            stepper.GoToStep(stepMapping[imageId]);
        }
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            currentInsruction = trackedImage;

            SetupInstruction(trackedImage);

            UpdateInstruction(trackedImage);

            UpdateModel(trackedImage);

            break;
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            UpdateInstruction(trackedImage);
        }

        foreach (var trackedImage in eventArgs.removed)
        {
            // Does not get invoked when the image goes out of view
        }
    }
}
