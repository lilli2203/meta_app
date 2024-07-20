using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static OVRInput;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class LineScript : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private Texture2D texture;
    [SerializeField] private Shader shader;
    int width = 1200;
    int height = 13874;

    private bool isDrawing = false;
    private Material currentMaterial;
    private Color currentColor = Color.white;
    private float textureScale = 1.0f;
    private List<Vector3> drawingPoints = new List<Vector3>();
    private LineRenderer lineRenderer;
    private float drawingStartTime;
    private List<Color> colorList = new List<Color> { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta };
    private int currentColorIndex = 0;
    private bool isScaling = false;
    private float scaleStartTime;
    private float scaleDuration = 0.5f;

    void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.material = new Material(shader);
    }

    void Update()
    {
        HandleDrawingMode();
        HandleColorChange();
        HandleTextureScaling();
        UpdateTextMessage();
        if (isDrawing)
        {
            DrawLine();
        }
    }

    private void HandleDrawingMode()
    {
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            ToggleDrawingMode();
        }

        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
        {
            Vector3 controllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
            Quaternion controllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
            Vector3 rayDirection = controllerRotation * Vector3.forward;

            if (Physics.Raycast(controllerPosition, rayDirection, out RaycastHit hit))
            {
                GameObject hitPlane = hit.collider.gameObject;

                if (hitPlane.transform.parent.name == "WALL_FACE")
                {
                    ApplyMaterialToPlane(hitPlane);
                }
            }
        }
    }

    private void HandleColorChange()
    {
        if (OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger))
        {
            currentColorIndex = (currentColorIndex + 1) % colorList.Count;
            currentColor = colorList[currentColorIndex];
            currentMaterial.color = currentColor;
            lineRenderer.material.color = currentColor;
        }
    }

    private void HandleTextureScaling()
    {
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
        {
            isScaling = true;
            scaleStartTime = Time.time;
        }

        if (isScaling)
        {
            float elapsed = Time.time - scaleStartTime;
            if (elapsed < scaleDuration)
            {
                float scale = Mathf.Lerp(1.0f, 1.5f, elapsed / scaleDuration);
                currentMaterial.mainTextureScale = new Vector2(scale, scale);
            }
            else
            {
                isScaling = false;
            }
        }
    }

    private void ToggleDrawingMode()
    {
        isDrawing = !isDrawing;
        if (isDrawing)
        {
            lineRenderer.positionCount = 0;
            drawingPoints.Clear();
            drawingStartTime = Time.time;
        }
    }

    private void ApplyMaterialToPlane(GameObject hitPlane)
    {
        Material material = new Material(shader)
        {
            mainTexture = texture
        };
        material.SetFloat("_Cull", (float)CullMode.Off);

        float scaleFactor = 0.0005f;
        float imageWidth = width * scaleFactor;
        float imageHeight = height * scaleFactor;

        float planeWidth = hitPlane.transform.localScale.x;
        float planeHeight = hitPlane.transform.localScale.z;

        float tileX = planeWidth / imageWidth;
        float tileY = planeHeight / imageHeight;
        material.mainTextureScale = new Vector2(tileX, tileY);

        MeshRenderer planeRenderer = hitPlane.GetComponentInParent<MeshRenderer>();
        planeRenderer.material = material;
        currentMaterial = material;
    }

    private void DrawLine()
    {
        Vector3 controllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
        drawingPoints.Add(controllerPosition);
        lineRenderer.positionCount = drawingPoints.Count;
        lineRenderer.SetPositions(drawingPoints.ToArray());
    }

    public void IncreaseTextureScale()
    {
        textureScale += 0.1f;
        currentMaterial.mainTextureScale = new Vector2(textureScale, textureScale);
    }

    public void DecreaseTextureScale()
    {
        textureScale = Mathf.Max(0.1f, textureScale - 0.1f);
        currentMaterial.mainTextureScale = new Vector2(textureScale, textureScale);
    }

    public void SetTextMessage(string message)
    {
        text.text = message;
    }

    private void UpdateTextMessage()
    {
        float elapsedDrawingTime = Time.time - drawingStartTime;
        SetTextMessage($"Drawing Time: {elapsedDrawingTime:F2}s");
    }

    private void OnDisable()
    {
        if (lineRenderer != null)
        {
            Destroy(lineRenderer);
        }
    }
}
