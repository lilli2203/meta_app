using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using static OVRInput;
using SelfButton = UnityEngine.UI.Button;
using TMPro;

public class PanelsScript2 : MonoBehaviour
{
    private Texture2D[] textures;
    public GameObject content;
    public GameObject buttonPrefab;
    private Texture2D currentTexture;
    [SerializeField] private Shader shader;
    private GameObject[] borderPanels;
    int width = 1200;
    int height = 13874;

    private GameObject pointCircle;
    private bool isPanelSelected = false; // Track if a panel is selected

    void Start()
    {
        textures = Resources.LoadAll<Texture2D>("panels");
        borderPanels = new GameObject[textures.Length];
        for (int i = 0; i < textures.Length; i++)
        {
            GameObject buttonGameObject = Instantiate(buttonPrefab);
            borderPanels[i] = buttonGameObject.transform.GetChild(0).gameObject;
            buttonGameObject.transform.SetParent(content.transform, false);

            SelfButton button = buttonGameObject.GetComponent<SelfButton>();
            int temp = i;
            button.onClick.AddListener(() => OnPanelClick(temp));

            Image imageComponent = buttonGameObject.GetComponent<Image>();
            Sprite sprite = Sprite.Create(textures[i], new Rect(0, 0, textures[i].width, textures[i].height), Vector2.one * 0.5f);
            imageComponent.sprite = sprite;
        }

        currentTexture = textures[0];
        CreatePointCircle();
    }

    void Update()
    {
        if (isPanelSelected)
        {
            CheckForWallPointing();
        }
    }

    private void CheckForWallPointing()
    {
        Vector3 controllerPosition;
        Quaternion controllerRotation;
        if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger))
        {
            controllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
            controllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch);
        }
        else
        {
            controllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
            controllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
        }

        Vector3 rayDirection = controllerRotation * Vector3.forward;
        RaycastHit hit;
        if (Physics.Raycast(controllerPosition, rayDirection, out hit))
        {
            GameObject hitPlane = hit.collider.gameObject;
            if (hitPlane.transform.parent == null) return;
            if (hitPlane.transform.parent.name == "WALL_FACE")
            {
                DisplayPointCircle(hit.point);
                if (OVRInput.Get(OVRInput.Button.PrimaryHandTrigger) || OVRInput.Get(OVRInput.Button.SecondaryHandTrigger))
                {
                    ApplyWallPanel(hitPlane);
                }
            }
        }
    }

    private void CreatePointCircle()
    {
        pointCircle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pointCircle.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        pointCircle.GetComponent<Renderer>().material.color = Color.red;
        pointCircle.SetActive(false);
    }

    private void DisplayPointCircle(Vector3 position)
    {
        pointCircle.SetActive(true);
        pointCircle.transform.position = position;
    }

    private void ApplyWallPanel(GameObject hitPlane)
    {
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

        pointCircle.SetActive(false);
        isPanelSelected = false;
    }

    void OnPanelClick(int t)
    {
        currentTexture = textures[t];
        for (int i = 0; i < textures.Length; i++)
        {
            if (i == t) borderPanels[i].SetActive(true);
            else borderPanels[i].SetActive(false);
        }
        isPanelSelected = true;
    }
}