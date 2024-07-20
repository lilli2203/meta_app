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
    private bool isThumbsUp = false;
    private bool isFist = false;
    public AudioSource audioSource;
    public AudioClip thumbsUpClip;
    public AudioClip fistClip;
    public Image backgroundPanel;

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

        DetectGestures();
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

    private void DetectGestures()
    {
        isThumbsUp = leftHand.GetFingerIsPinching(OVRHand.HandFinger.Thumb) && !leftHand.GetFingerIsPinching(OVRHand.HandFinger.Index) &&
                     !leftHand.GetFingerIsPinching(OVRHand.HandFinger.Middle) && !leftHand.GetFingerIsPinching(OVRHand.HandFinger.Ring) &&
                     !leftHand.GetFingerIsPinching(OVRHand.HandFinger.Pinky);

        isFist = leftHand.GetFingerIsPinching(OVRHand.HandFinger.Thumb) && leftHand.GetFingerIsPinching(OVRHand.HandFinger.Index) &&
                 leftHand.GetFingerIsPinching(OVRHand.HandFinger.Middle) && leftHand.GetFingerIsPinching(OVRHand.HandFinger.Ring) &&
                 leftHand.GetFingerIsPinching(OVRHand.HandFinger.Pinky);

        if (isThumbsUp)
        {
            PlayAudioClip(thumbsUpClip);
            ChangeBackgroundColor(Color.green);
        }

        if (isFist)
        {
            PlayAudioClip(fistClip);
            ChangeBackgroundColor(Color.red);
        }
    }

    private void PlayAudioClip(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void ChangeBackgroundColor(Color color)
    {
        if (backgroundPanel != null)
        {
            backgroundPanel.color = color;
        }
    }
}
