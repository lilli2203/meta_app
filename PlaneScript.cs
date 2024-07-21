using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static OVRInput;
using TMPro;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.Rendering;
using Meta.XR.MRUtilityKit;

public class PlaneScript : MonoBehaviour
{
    // [SerializeField] private LineRenderer lineRenderer;
    // [SerializeField] private TMP_Text text;
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
        if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger))
        {
            Debug.Log("press");
            Vector3 controllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
            Quaternion controllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch);
            Vector3 rayDirection = controllerRotation * Vector3.forward;
            
            RaycastHit hit;
            if (Physics.Raycast(controllerPosition, rayDirection, out hit))
            {
                GameObject hitPlane = hit.collider.gameObject;

                OVRSemanticClassification anchor = hitPlane.GetComponentInParent<OVRSemanticClassification>();
                List<string> labelsList = (List<string>) anchor.Labels;
                Debug.Log(labelsList.ToString());
                if (labelsList.Contains("WALL_FACE"))
                {
                    // text.text = $"Hit: {string.Join(", ", anchor.Labels)}";
                    Debug.Log($"Hit: {string.Join(", ", anchor.Labels)}");

                    // lineRenderer.SetPosition(0, controllerPosition);
                    // lineRenderer.SetPosition(1, hit.point);

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
                // else
                // {
                //     // text.text = $"{string.Join(", ", anchor.Labels)} is not a wall";
                // }
            }
        }
    }
}
