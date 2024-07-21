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
    public GameObject interactableObject;
    public Text interactionStatusText;
    private bool isGrabbingObject = false;
    private Transform originalParent;
    private bool isIndexGrabbing = false;
    private Vector3 lastHandPosition;

    public GameObject settingsMenu;
    public Button showSettingsButton;
    public Slider positionOffsetSlider;
    public Slider rotationOffsetSlider;
    public Slider doubleTapTimeSlider;
    public Button closeSettingsButton;
    public Slider canvasSizeSlider;
    public Slider canvasRotationSlider;
    private float canvasSize = 1f;
    private float canvasRotation = 0f;

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
        if (showSettingsButton != null)
        {
            showSettingsButton.onClick.AddListener(ToggleSettingsMenu);
        }
        if (closeSettingsButton != null)
        {
            closeSettingsButton.onClick.AddListener(ToggleSettingsMenu);
        }
        if (positionOffsetSlider != null)
        {
            positionOffsetSlider.onValueChanged.AddListener(UpdatePositionOffset);
        }
        if (rotationOffsetSlider != null)
        {
            rotationOffsetSlider.onValueChanged.AddListener(UpdateRotationOffset);
        }
        if (doubleTapTimeSlider != null)
        {
            doubleTapTimeSlider.onValueChanged.AddListener(UpdateDoubleTapTime);
        }
        if (canvasSizeSlider != null)
        {
            canvasSizeSlider.onValueChanged.AddListener(UpdateCanvasSize);
        }
        if (canvasRotationSlider != null)
        {
            canvasRotationSlider.onValueChanged.AddListener(UpdateCanvasRotation);
        }

        UpdateHandFollowStatusText();
    }

    void Update()
    {
        HandleHandFollowToggle();

        if (targetHandTransform != null)
        {
            FollowHand();
            HandleCanvasVisibility();
            DetectGestures();
            HandleObjectInteraction();
            MoveCanvasWithHand();
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

    private void FollowHand()
    {
        Transform activeHandTransform = followRightHand ? rightHandTransform : targetHandTransform;
        Vector3 activePositionOffset = followRightHand ? rightHandPositionOffset : positionOffset;
        Vector3 activeRotationOffset = followRightHand ? rightHandRotationOffset : rotationOffset;

        transform.position = activeHandTransform.position + activeHandTransform.TransformDirection(activePositionOffset);
        transform.rotation = activeHandTransform.rotation * Quaternion.Euler(activeRotationOffset);

        if (canvas != null && scrollViewRectTransform != null)
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollViewRectTransform);
        }

        // Apply size and rotation
        transform.localScale = Vector3.one * canvasSize;
        transform.rotation = Quaternion.Euler(0, canvasRotation, 0);
    }

    private void HandleCanvasVisibility()
    {
        bool isIndexPinching = leftHand.GetFingerIsPinching(OVRHand.HandFinger.Index);
        if (isIndexPinching && !wasIndexPinching)
        {
            isCanvasVisible = !isCanvasVisible;
            canvas.enabled = isCanvasVisible;
        }
        wasIndexPinching = isIndexPinching;
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

    private void HandleObjectInteraction()
    {
        if (isFist && !isGrabbingObject)
        {
            GrabObject();
        }
        else if (!isFist && isGrabbingObject)
        {
            ReleaseObject();
        }
    }

    private void GrabObject()
    {
        if (interactableObject != null)
        {
            originalParent = interactableObject.transform.parent;
            interactableObject.transform.SetParent(targetHandTransform);
            isGrabbingObject = true;
            UpdateInteractionStatusText("Object Grabbed");
        }
    }

    private void ReleaseObject()
    {
        if (interactableObject != null)
        {
            interactableObject.transform.SetParent(originalParent);
            isGrabbingObject = false;
            UpdateInteractionStatusText("Object Released");
        }
    }

    private void UpdateInteractionStatusText(string status)
    {
        if (interactionStatusText != null)
        {
            interactionStatusText.text = status;
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

    private void ToggleSettingsMenu()
    {
        if (settingsMenu != null)
        {
            settingsMenu.SetActive(!settingsMenu.activeSelf);
        }
    }

    private void UpdatePositionOffset(float value)
    {
        positionOffset = new Vector3(0, 0, value);
    }

    private void UpdateRotationOffset(float value)
    {
        rotationOffset = new Vector3(0, 0, value);
    }

    private void UpdateDoubleTapTime(float value)
    {
        doubleTapTime = value;
    }

    private void UpdateCanvasSize(float value)
    {
        canvasSize = value;
    }

    private void UpdateCanvasRotation(float value)
    {
        canvasRotation = value;
    }

    private void MoveCanvasWithHand()
    {
        if (isIndexPinching && !isIndexGrabbing)
        {
            isIndexGrabbing = true;
            lastHandPosition = leftHand.transform.position;
        }

        if (isIndexGrabbing)
        {
            Vector3 handMovement = leftHand.transform.position - lastHandPosition;
            transform.position += handMovement;
            lastHandPosition = leftHand.transform.position;

            if (!leftHand.GetFingerIsPinching(OVRHand.HandFinger.Index))
            {
                isIndexGrabbing = false;
            }
        }
    }

    private void DetectThumbGesture()
    {
        bool thumbPinched = leftHand.GetFingerIsPinching(OVRHand.HandFinger.Thumb) && !leftHand.GetFingerIsPinching(OVRHand.HandFinger.Index);
        if (thumbPinched)
        {
            ChangeBackgroundColor(Color.yellow);
        }
    }

    private void DetectPalmGesture()
    {
        bool palmOpen = !leftHand.GetFingerIsPinching(OVRHand.HandFinger.Thumb) && !leftHand.GetFingerIsPinching(OVRHand.HandFinger.Index) &&
                         !leftHand.GetFingerIsPinching(OVRHand.HandFinger.Middle) && !leftHand.GetFingerIsPinching(OVRHand.HandFinger.Ring) &&
                         !leftHand.GetFingerIsPinching(OVRHand.HandFinger.Pinky);

        if (palmOpen)
        {
            PlayAudioClip(thumbsUpClip);
            ChangeBackgroundColor(Color.blue);
        }
    }
}
