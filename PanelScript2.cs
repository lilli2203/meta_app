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
    private bool isPanelSelected = false;
    private Dictionary<string, Texture2D> savedPanels = new Dictionary<string, Texture2D>();
    private Stack<string> undoStack = new Stack<string>();
    private Stack<string> redoStack = new Stack<string>();

    public TMP_Text statusText;

    void Start()
    {
        LoadTextures();
        CreateButtons();
        SetInitialTexture();
        CreatePointCircle();
        LoadPanelStates();
        UpdateStatusText("Initialization complete.");
    }

    void Update()
    {
        if (isPanelSelected)
        {
            CheckForWallPointing();
        }
        HandleUndoRedo();
    }

    private void LoadTextures()
    {
        textures = Resources.LoadAll<Texture2D>("panels");
        borderPanels = new GameObject[textures.Length];
    }

    private void CreateButtons()
    {
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
    }

    private void SetInitialTexture()
    {
        if (textures.Length > 0)
        {
            currentTexture = textures[0];
        }
        else
        {
            Debug.LogError("No textures found!");
        }
    }

    private void CreatePointCircle()
    {
        pointCircle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pointCircle.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        pointCircle.GetComponent<Renderer>().material.color = Color.red;
        pointCircle.SetActive(false);
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
        if (Physics.Raycast(controllerPosition, rayDirection, out RaycastHit hit))
        {
            GameObject hitPlane = hit.collider.gameObject;
            if (hitPlane.transform.parent != null && hitPlane.transform.parent.name == "WALL_FACE")
            {
                DisplayPointCircle(hit.point);
                if (OVRInput.Get(OVRInput.Button.PrimaryHandTrigger) || OVRInput.Get(OVRInput.Button.SecondaryHandTrigger))
                {
                    ApplyWallPanel(hitPlane);
                }
            }
        }
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
        string panelName = hitPlane.transform.parent.name;
        if (savedPanels.ContainsKey(panelName))
        {
            undoStack.Push(panelName);
        }
        savedPanels[panelName] = currentTexture;
        SavePanelStates();

        pointCircle.SetActive(false);
        isPanelSelected = false;

        UpdateStatusText("Texture applied to " + panelName);
    }

    private void OnPanelClick(int t)
    {
        currentTexture = textures[t];
        for (int i = 0; i < textures.Length; i++)
        {
            borderPanels[i].SetActive(i == t);
        }
        isPanelSelected = true;
    }

    private void SavePanelStates()
    {
        foreach (var panel in savedPanels)
        {
            PlayerPrefs.SetString(panel.Key, panel.Value.name);
        }
        PlayerPrefs.Save();
    }

    private void LoadPanelStates()
    {
        foreach (GameObject hitPlane in GameObject.FindGameObjectsWithTag("WALL_FACE"))
        {
            string panelName = hitPlane.transform.parent.name;
            if (PlayerPrefs.HasKey(panelName))
            {
                string textureName = PlayerPrefs.GetString(panelName);
                Texture2D texture = Resources.Load<Texture2D>("panels/" + textureName);
                if (texture != null)
                {
                    savedPanels[panelName] = texture;
                    ApplySavedTexture(hitPlane, texture);
                }
            }
        }
    }

    private void ApplySavedTexture(GameObject hitPlane, Texture2D texture)
    {
        Material material = new Material(shader);
        material.mainTexture = texture;
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

    private void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    // New functionality for undo/redo operations
    private void HandleUndoRedo()
    {
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            UndoLastAction();
        }

        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            RedoLastAction();
        }
    }

    private void UndoLastAction()
    {
        if (undoStack.Count > 0)
        {
            string panelName = undoStack.Pop();
            redoStack.Push(panelName);

            if (PlayerPrefs.HasKey(panelName))
            {
                PlayerPrefs.DeleteKey(panelName);
            }

            foreach (GameObject hitPlane in GameObject.FindGameObjectsWithTag("WALL_FACE"))
            {
                if (hitPlane.transform.parent.name == panelName)
                {
                    hitPlane.GetComponentInParent<MeshRenderer>().material = null;
                    savedPanels.Remove(panelName);
                    UpdateStatusText("Undid texture application for " + panelName);
                    break;
                }
            }
        }
        else
        {
            UpdateStatusText("No actions to undo.");
        }
    }

    private void RedoLastAction()
    {
        if (redoStack.Count > 0)
        {
            string panelName = redoStack.Pop();
            if (savedPanels.ContainsKey(panelName))
            {
                Texture2D texture = savedPanels[panelName];
                foreach (GameObject hitPlane in GameObject.FindGameObjectsWithTag("WALL_FACE"))
                {
                    if (hitPlane.transform.parent.name == panelName)
                    {
                        ApplySavedTexture(hitPlane, texture);
                        undoStack.Push(panelName);
                        UpdateStatusText("Redid texture application for " + panelName);
                        break;
                    }
                }
            }
        }
        else
        {
            UpdateStatusText("No actions to redo.");
        }
    }
}
