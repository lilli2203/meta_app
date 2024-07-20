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
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            ToggleDrawingMode();
        }

        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
        {
            Vector3 controllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
            Quaternion controllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
            Vector3 rayDirection = controllerRotation * Vector3.forward;

            RaycastHit hit;
            if (Physics.Raycast(controllerPosition, rayDirection, out hit))
            {
                GameObject hitPlane = hit.collider.gameObject;

                if (hitPlane.transform.parent.name == "WALL_FACE")
                {
                    ApplyMaterialToPlane(hitPlane);
                }
            }
        }

        if (OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger))
        {
            ChangeColor();
        }

        if (isDrawing)
        {
            DrawLine();
        }
    }

    private void ToggleDrawingMode()
    {
        isDrawing = !isDrawing;
        if (isDrawing)
        {
            lineRenderer.positionCount = 0;
            drawingPoints.Clear();
        }
    }

    private void ApplyMaterialToPlane(GameObject hitPlane)
    {
        Material material = new Material(shader);
        material.mainTexture = texture;
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

    private void ChangeColor()
    {
        currentColor = new Color(Random.value, Random.value, Random.value);
        currentMaterial.color = currentColor;
        lineRenderer.material.color = currentColor;
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

    private void OnDisable()
    {
        if (lineRenderer != null)
        {
            Destroy(lineRenderer);
        }
    }
}
