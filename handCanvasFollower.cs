using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class handCanvasFollower : MonoBehaviour
{
    public OVRHand leftHand;
    public Vector3 positionOffset = new Vector3(0, 0, 0.1f); 
    public Vector3 rotationOffset = new Vector3(0, 0, 0);   
    private Canvas canvas;
    private RectTransform scrollViewRectTransform;
    private bool isCanvasVisible = false; 
    private bool wasIndexPinching = false; 

    private Transform targetHandTransform;

    // New Variables
    public OVRHand rightHand;
    public Vector3 rightHandPositionOffset = new Vector3(0, 0, 0.1f); 
    public Vector3 rightHandRotationOffset = new Vector3(0, 0, 0);    
    private Transform rightHandTransform;

    private bool followRightHand = false;
    private float doubleTapTime = 0.2f; 
    private float lastTapTime = 0;
    private int tapCount = 0;
    public Button toggleHandButton;
    public Text handFollowStatusText;

    void Start()
    {
        targetHandTransform = leftHand.transform;
        rightHandTransform = rightHand.transform;
        canvas = GetComponent<Canvas>();
        ScrollRect scrollRect = GetComponentInChildren<ScrollRect>();
        if (scrollRect != null)
        {
            scrollViewRectTransform = scrollRect.GetComponent<RectTransform>();
        }
        canvas.enabled = isCanvasVisible;

        if (toggleHandButton != null)
        {
            toggleHandButton.onClick.AddListener(ToggleHandFollow);
        }
        UpdateHandFollowStatusText();
    }

    void Update()
    {
        HandleHandFollowToggle();

        if (targetHandTransform != null)
        {
            if (followRightHand)
            {
                transform.position = rightHandTransform.position + rightHandTransform.TransformDirection(rightHandPositionOffset);
                transform.rotation = rightHandTransform.rotation * Quaternion.Euler(rightHandRotationOffset);
            }
            else
            {
                transform.position = targetHandTransform.position + targetHandTransform.TransformDirection(positionOffset);
                transform.rotation = targetHandTransform.rotation * Quaternion.Euler(rotationOffset);
            }

            if (canvas != null && scrollViewRectTransform != null)
            {
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(scrollViewRectTransform);
            }
            bool isIndexPinching = leftHand.GetFingerIsPinching(OVRHand.HandFinger.Index);
            if (isIndexPinching && !wasIndexPinching)
            {
                isCanvasVisible = !isCanvasVisible;
                canvas.enabled = isCanvasVisible;
            }
            wasIndexPinching = isIndexPinching;
        }
    }

    private void HandleHandFollowToggle()
    {
        if (Input.GetMouseButtonDown(0))
        {
            tapCount++;
            if (tapCount == 1)
            {
                lastTapTime = Time.time;
            }
            else if (tapCount == 2 && Time.time - lastTapTime < doubleTapTime)
            {
                ToggleHandFollow();
                tapCount = 0;
            }
        }

        if (Time.time - lastTapTime > doubleTapTime)
        {
            tapCount = 0;
        }
    }

    private void ToggleHandFollow()
    {
        followRightHand = !followRightHand;
        UpdateHandFollowStatusText();
    }

    private void UpdateHandFollowStatusText()
    {
        if (handFollowStatusText != null)
        {
            handFollowStatusText.text = followRightHand ? "Following Right Hand" : "Following Left Hand";
        }
    }
}
