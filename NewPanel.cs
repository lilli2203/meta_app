using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using static OVRInput;
using SelfButton = UnityEngine.UI.Button;

public class NewPanel : MonoBehaviour
{
    private Texture2D[] textures;

    public GameObject content;
    public GameObject panel;
    public GameObject selectedPanelDisplay; 

    private Texture2D currentTexture;
    [SerializeField] private Shader shader;

    private GameObject[] borderPanels;
    int width = 1200;
    int height = 13874;

    public OVRHand hand;

    private Vector3 originalScale;

    void Start()
    {
        textures = Resources.LoadAll<Texture2D>("panels");
        borderPanels = new GameObject[textures.Length];

        for (int i = 0; i < textures.Length; i++)
        {
            GameObject newPanel = Instantiate(panel, content.transform);
            Image childImage = newPanel.GetComponent<Image>();
            childImage.sprite = Sprite.Create(textures[i], new Rect(0, 0, textures[i].width, textures[i].height), Vector2.one * 0.5f);

            SelfButton button = newPanel.GetComponent<SelfButton>();
            int temp = i;
            button.onClick.AddListener(() => OnPanelClick(temp));

            borderPanels[i] = newPanel;

            newPanel.transform.localPosition = new Vector3(0, -i * (textures[i].height + 10), 0); 
        }

        currentTexture = textures[0];
        originalScale = borderPanels[0].transform.localScale;
        UpdateSelectedPanelDisplay();
    }

    void Update()
    {
        if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) || 
            OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger) ||
            hand.GetFingerIsPinching(OVRHand.HandFinger.Index))
        {
            Vector3 controllerPosition;
            Quaternion controllerRotation;
            bool isHandPinching = hand.GetFingerIsPinching(OVRHand.HandFinger.Index);

            if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger))
            {
                controllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
                controllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch);
            }
            else if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
            {
                controllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
                controllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
            }
            else if (isHandPinching)
            {
                controllerPosition = hand.PointerPose.position;
                controllerRotation = hand.PointerPose.rotation;
                Debug.Log("Index finger is pinching");
            }
            else
            {
                return;
            }

            Vector3 rayDirection = controllerRotation * Vector3.forward;

            RaycastHit hit;
            if (Physics.Raycast(controllerPosition, rayDirection, out hit))
            {
                GameObject hitPlane = hit.collider.gameObject;
                if (hitPlane.transform.parent == null) return;
                if (hitPlane.transform.parent.name == "WALL_FACE")
                {
                    Debug.Log($"Hit: {hitPlane.transform.parent.name}");

                    Material material = new Material(shader);
                    material.mainTexture = currentTexture;
                    material.SetFloat("_Cull", (float)CullMode.Off);
                    material.SetFloat("_EnvironmentDepthBias", 0.06f);

                    float scaleFactor = 0.0001f;
                    float imageWidth = width * scaleFactor;
                    float imageHeight = height * scaleFactor;

                    float planeWidth = hitPlane.transform.localScale.x;
                    float planeHeight = hitPlane.transform.localScale.z;

                    float tileX = planeWidth / imageWidth;
                    float tileY = planeHeight / imageHeight;
                    material.mainTextureScale = new Vector2(tileX, tileY);

                    MeshRenderer planeRenderer = hitPlane.GetComponentInParent<MeshRenderer>();
                    planeRenderer.material = material;
                }
                else
                {
                    Debug.Log($"{hitPlane.transform.parent.name} is not a wall");
                }
            }
        }
    }

    void OnPanelClick(int t)
    {
        Debug.Log(t);
        currentTexture = textures[t];

        UpdateSelectedPanelDisplay();

        for (int i = 0; i < textures.Length; i++)
        {
            if (i == t)
            {
                borderPanels[i].transform.localScale = new Vector3(originalScale.x * 1.2f, originalScale.y * 1.2f, originalScale.z);
            }
            else
            {
                borderPanels[i].transform.localScale = originalScale; 
            }
        }
    }

    void UpdateSelectedPanelDisplay()
    {
        Image displayImage = selectedPanelDisplay.GetComponent<Image>();
        displayImage.sprite = Sprite.Create(currentTexture, new Rect(0, 0, currentTexture.width, currentTexture.height), Vector2.one * 0.5f);
    }

    // New functionality

    private bool isZooming = false;
    private Vector3 initialScale;
    private Vector3 zoomTargetScale = new Vector3(2.0f, 2.0f, 2.0f);
    private float zoomSpeed = 2.0f;
    private Vector3 zoomStartScale;

    void ZoomSelectedPanel()
    {
        if (isZooming)
        {
            float step = zoomSpeed * Time.deltaTime;
            selectedPanelDisplay.transform.localScale = Vector3.Lerp(selectedPanelDisplay.transform.localScale, zoomTargetScale, step);
        }
    }

    void StartZoom()
    {
        isZooming = true;
        initialScale = selectedPanelDisplay.transform.localScale;
        zoomStartScale = initialScale;
    }

    void StopZoom()
    {
        isZooming = false;
    }

    void HandleZoomInput()
    {
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            StartZoom();
        }

        if (OVRInput.GetUp(OVRInput.Button.One))
        {
            StopZoom();
        }
    }

    void ResetZoom()
    {
        selectedPanelDisplay.transform.localScale = initialScale;
    }

    private bool isRotating = false;
    private float rotationSpeed = 50.0f;

    void RotateSelectedPanel()
    {
        if (isRotating)
        {
            float rotationStep = rotationSpeed * Time.deltaTime;
            selectedPanelDisplay.transform.Rotate(Vector3.up, rotationStep);
        }
    }

    void StartRotation()
    {
        isRotating = true;
    }

    void StopRotation()
    {
        isRotating = false;
    }

    void HandleRotationInput()
    {
        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            StartRotation();
        }

        if (OVRInput.GetUp(OVRInput.Button.Two))
        {
            StopRotation();
        }
    }

    void Update()
    {
        HandleZoomInput();
        HandleRotationInput();
        ZoomSelectedPanel();
        RotateSelectedPanel();
    }

    // Additional new functionality

    private bool isDragging = false;
    private Vector3 initialDragPosition;
    private Vector3 dragOffset;

    void StartDrag()
    {
        isDragging = true;
        initialDragPosition = selectedPanelDisplay.transform.position;
        dragOffset = selectedPanelDisplay.transform.position - hand.PointerPose.position;
    }

    void StopDrag()
    {
        isDragging = false;
    }

    void DragSelectedPanel()
    {
        if (isDragging)
        {
            selectedPanelDisplay.transform.position = hand.PointerPose.position + dragOffset;
        }
    }

    void HandleDragInput()
    {
        if (OVRInput.GetDown(OVRInput.Button.Three))
        {
            StartDrag();
        }

        if (OVRInput.GetUp(OVRInput.Button.Three))
        {
            StopDrag();
        }
    }

    private bool isFlipping = false;
    private Quaternion initialFlipRotation;
    private Quaternion flipTargetRotation = Quaternion.Euler(0, 180, 0);
    private float flipSpeed = 2.0f;

    void FlipSelectedPanel()
    {
        if (isFlipping)
        {
            float step = flipSpeed * Time.deltaTime;
            selectedPanelDisplay.transform.rotation = Quaternion.Lerp(selectedPanelDisplay.transform.rotation, flipTargetRotation, step);
        }
    }

    void StartFlip()
    {
        isFlipping = true;
        initialFlipRotation = selectedPanelDisplay.transform.rotation;
    }

    void StopFlip()
    {
        isFlipping = false;
        selectedPanelDisplay.transform.rotation = initialFlipRotation;
    }

    void HandleFlipInput()
    {
        if (OVRInput.GetDown(OVRInput.Button.Four))
        {
            StartFlip();
        }

        if (OVRInput.GetUp(OVRInput.Button.Four))
        {
            StopFlip();
        }
    }

    void Update()
    {
        HandleZoomInput();
        HandleRotationInput();
        HandleDragInput();
        HandleFlipInput();
        ZoomSelectedPanel();
        RotateSelectedPanel();
        DragSelectedPanel();
        FlipSelectedPanel();
    }
}
