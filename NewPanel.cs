
//Size increase 


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
    public GameObject selectedPanelDisplay; // The GameObject to display the currently selected wall panel

    private Texture2D currentTexture;
    [SerializeField] private Shader shader;

    private GameObject[] borderPanels;
    int width = 1200;
    int height = 13874;

    public OVRHand hand;

    private Vector3 originalScale;

    // Start is called before the first frame update
    void Start()
    {
        textures = Resources.LoadAll<Texture2D>("panels");
        borderPanels = new GameObject[textures.Length];

        // Iterate through the textures array and create panels
        for (int i = 0; i < textures.Length; i++)
        {
            // Create a new panel
            GameObject newPanel = Instantiate(panel, content.transform);
            Image childImage = newPanel.GetComponent<Image>();
            childImage.sprite = Sprite.Create(textures[i], new Rect(0, 0, textures[i].width, textures[i].height), Vector2.one * 0.5f);

            // Assign the click function
            SelfButton button = newPanel.GetComponent<SelfButton>();
            int temp = i;
            button.onClick.AddListener(() => OnPanelClick(temp));

            // Ensure the border panel reference is set correctly
            borderPanels[i] = newPanel;

            // Adjust the panel's position to prevent overlap (if needed)
            newPanel.transform.localPosition = new Vector3(0, -i * (textures[i].height + 10), 0); // Adjust spacing as needed
        }

        currentTexture = textures[0];
        originalScale = borderPanels[0].transform.localScale; // Assuming all panels have the same original scale
        UpdateSelectedPanelDisplay();
    }

    // Update is called once per frame
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
                return; // No valid input detected
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
                    // Constrain the dimensions
                    float imageWidth = width * scaleFactor;
                    float imageHeight = height * scaleFactor;

                    // Obtain the plane's dimensions
                    float planeWidth = hitPlane.transform.localScale.x;
                    float planeHeight = hitPlane.transform.localScale.z;

                    // Set the tiling values
                    float tileX = planeWidth / imageWidth;
                    float tileY = planeHeight / imageHeight;
                    material.mainTextureScale = new Vector2(tileX, tileY);

                    // Apply the material to the plane
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

        // Update the selected panel display
        UpdateSelectedPanelDisplay();

        // Resize the selected panel and reset others
        for (int i = 0; i < textures.Length; i++)
        {
            if (i == t)
            {
                // Enlarge the selected panel by scaling both width and height
                borderPanels[i].transform.localScale = new Vector3(originalScale.x * 1.2f, originalScale.y * 1.2f, originalScale.z);
            }

            else
            {
                borderPanels[i].transform.localScale = originalScale; // Reset to original size
            }
        }
    }

    void UpdateSelectedPanelDisplay()
    {
        Image displayImage = selectedPanelDisplay.GetComponent<Image>();
        displayImage.sprite = Sprite.Create(currentTexture, new Rect(0, 0, currentTexture.width, currentTexture.height), Vector2.one * 0.5f);
    }
}
