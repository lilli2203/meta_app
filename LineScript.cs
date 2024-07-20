using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static OVRInput;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class LineScript : MonoBehaviour
{
    // [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private TMP_Text text;
    [SerializeField] private Texture2D texture;
    [SerializeField] private Shader shader;
    int width = 1200;
    int height = 13874;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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
                    Debug.Log($"Hit: {hitPlane.transform.parent.name}");

                    Material material = new Material(shader);
                    material.mainTexture = texture;
                    material.SetFloat("_Cull", (float) CullMode.Off);

                    float scaleFactor = 0.0005f;
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
}
