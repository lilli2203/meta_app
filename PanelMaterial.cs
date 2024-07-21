using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelMaterial : MonoBehaviour
{
    private Texture2D[] textures;
    public GameObject content;
    public GameObject panel;

    void Start()
    {
        textures = Resources.LoadAll<Texture2D>("panels");
        for (int i = 0; i < textures.Length; i++)
        {
            GameObject newPannel = Instantiate(panel, content.transform);
            Image childImage = newPannel.GetComponent<Image>();
            childImage.sprite = Sprite.Create(textures[i], new Rect(0, 0, textures[i].width, textures[i].height), Vector2.one * 0.5f);
        }
    }

    void Update()
    {
    }

    // New functionality

    private bool isZooming = false;
    private Vector3 initialScale;
    private Vector3 zoomTargetScale = new Vector3(2.0f, 2.0f, 2.0f);
    private float zoomSpeed = 2.0f;
    private Vector3 zoomStartScale;

    void ZoomSelectedPanel(GameObject selectedPanel)
    {
        if (isZooming)
        {
            float step = zoomSpeed * Time.deltaTime;
            selectedPanel.transform.localScale = Vector3.Lerp(selectedPanel.transform.localScale, zoomTargetScale, step);
        }
    }

    void StartZoom(GameObject selectedPanel)
    {
        isZooming = true;
        initialScale = selectedPanel.transform.localScale;
        zoomStartScale = initialScale;
    }

    void StopZoom(GameObject selectedPanel)
    {
        isZooming = false;
    }

    void HandleZoomInput(GameObject selectedPanel)
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            StartZoom(selectedPanel);
        }

        if (Input.GetKeyUp(KeyCode.Z))
        {
            StopZoom(selectedPanel);
        }
    }

    private bool isRotating = false;
    private float rotationSpeed = 50.0f;

    void RotateSelectedPanel(GameObject selectedPanel)
    {
        if (isRotating)
        {
            float rotationStep = rotationSpeed * Time.deltaTime;
            selectedPanel.transform.Rotate(Vector3.up, rotationStep);
        }
    }

    void StartRotation(GameObject selectedPanel)
    {
        isRotating = true;
    }

    void StopRotation(GameObject selectedPanel)
    {
        isRotating = false;
    }

    void HandleRotationInput(GameObject selectedPanel)
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartRotation(selectedPanel);
        }

        if (Input.GetKeyUp(KeyCode.R))
        {
            StopRotation(selectedPanel);
        }
    }

    private bool isDragging = false;
    private Vector3 initialDragPosition;
    private Vector3 dragOffset;

    void StartDrag(GameObject selectedPanel)
    {
        isDragging = true;
        initialDragPosition = selectedPanel.transform.position;
        dragOffset = selectedPanel.transform.position - Input.mousePosition;
    }

    void StopDrag(GameObject selectedPanel)
    {
        isDragging = false;
    }

    void DragSelectedPanel(GameObject selectedPanel)
    {
        if (isDragging)
        {
            selectedPanel.transform.position = Input.mousePosition + dragOffset;
        }
    }

    void HandleDragInput(GameObject selectedPanel)
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartDrag(selectedPanel);
        }

        if (Input.GetMouseButtonUp(0))
        {
            StopDrag(selectedPanel);
        }
    }

    void Update()
    {
        GameObject selectedPanel = content.transform.GetChild(0).gameObject; // Example: Select the first panel

        HandleZoomInput(selectedPanel);
        HandleRotationInput(selectedPanel);
        HandleDragInput(selectedPanel);
        ZoomSelectedPanel(selectedPanel);
        RotateSelectedPanel(selectedPanel);
        DragSelectedPanel(selectedPanel);
    }

    private bool isFlipping = false;
    private Quaternion initialFlipRotation;
    private Quaternion flipTargetRotation = Quaternion.Euler(0, 180, 0);
    private float flipSpeed = 2.0f;

    void FlipSelectedPanel(GameObject selectedPanel)
    {
        if (isFlipping)
        {
            float step = flipSpeed * Time.deltaTime;
            selectedPanel.transform.rotation = Quaternion.Lerp(selectedPanel.transform.rotation, flipTargetRotation, step);
        }
    }

    void StartFlip(GameObject selectedPanel)
    {
        isFlipping = true;
        initialFlipRotation = selectedPanel.transform.rotation;
    }

    void StopFlip(GameObject selectedPanel)
    {
        isFlipping = false;
        selectedPanel.transform.rotation = initialFlipRotation;
    }

    void HandleFlipInput(GameObject selectedPanel)
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            StartFlip(selectedPanel);
        }

        if (Input.GetKeyUp(KeyCode.F))
        {
            StopFlip(selectedPanel);
        }
    }

    void Update()
    {
        GameObject selectedPanel = content.transform.GetChild(0).gameObject; // Example: Select the first panel

        HandleZoomInput(selectedPanel);
        HandleRotationInput(selectedPanel);
        HandleDragInput(selectedPanel);
        HandleFlipInput(selectedPanel);
        ZoomSelectedPanel(selectedPanel);
        RotateSelectedPanel(selectedPanel);
        DragSelectedPanel(selectedPanel);
        FlipSelectedPanel(selectedPanel);
    }
}
