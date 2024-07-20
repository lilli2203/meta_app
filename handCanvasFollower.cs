using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class handCanvasFollower : MonoBehaviour
{
    public OVRHand leftHand;
    public Vector3 positionOffset = new Vector3(0, 0, 0.1f); // Default position offset
    public Vector3 rotationOffset = new Vector3(0, 0, 0);    // Default rotation offset
    private Canvas canvas;
    private RectTransform scrollViewRectTransform;
    private bool isCanvasVisible = false; // Track canvas visibility state
    private bool wasIndexPinching = false; // Track previous state of index pinch

    private Transform targetHandTransform;

    void Start()
    {
        targetHandTransform = leftHand.transform;
        canvas = GetComponent<Canvas>();

        // Find the Scroll View RectTransform
        ScrollRect scrollRect = GetComponentInChildren<ScrollRect>();
        if (scrollRect != null)
        {
            scrollViewRectTransform = scrollRect.GetComponent<RectTransform>();
        }

        // Set initial canvas visibility
        canvas.enabled = isCanvasVisible;
    }

    void Update()
    {
        if (targetHandTransform != null)
        {
            // Update the position and rotation of the canvas to match the hand
            transform.position = targetHandTransform.position + targetHandTransform.TransformDirection(positionOffset);
            transform.rotation = targetHandTransform.rotation * Quaternion.Euler(rotationOffset);

            if (canvas != null && scrollViewRectTransform != null)
            {
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(scrollViewRectTransform);
            }

            // Check the pinch state of the index finger
            bool isIndexPinching = leftHand.GetFingerIsPinching(OVRHand.HandFinger.Index);

            // Toggle canvas visibility on pinch
            if (isIndexPinching && !wasIndexPinching)
            {
                isCanvasVisible = !isCanvasVisible;
                canvas.enabled = isCanvasVisible;
            }

            // Update the previous pinch state
            wasIndexPinching = isIndexPinching;
        }
    }
}
