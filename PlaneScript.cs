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
    [SerializeField] private Texture2D texture;
    [SerializeField] private Shader shader;
    [SerializeField] private TMP_Text debugText;  
    [SerializeField] private GameObject hitIndicatorPrefab; 
    private GameObject hitIndicator;

    private int width = 1200;
    private int height = 13874;
    private bool isLeftHanded = true; 

    void Start()
    {
        InitializeHitIndicator();
    }

    void Update()
    {
        HandleControllerInput();
    }

    private void InitializeHitIndicator()
    {
        if (hitIndicatorPrefab != null)
        {
            hitIndicator = Instantiate(hitIndicatorPrefab);
            hitIndicator.SetActive(false);
        }
    }

    private void HandleControllerInput()
    {
        if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger))
        {
            Vector3 controllerPosition = OVRInput.GetLocalControllerPosition(isLeftHanded ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch);
            Quaternion controllerRotation = OVRInput.GetLocalControllerRotation(isLeftHanded ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch);
            Vector3 rayDirection = controllerRotation * Vector3.forward;

            RaycastHit hit;
            if (Physics.Raycast(controllerPosition, rayDirection, out hit))
            {
                ProcessRaycastHit(hit);
            }
            else
            {
                if (hitIndicator != null)
                {
                    hitIndicator.SetActive(false);
                }
            }
        }
    }

    private void ProcessRaycastHit(RaycastHit hit)
    {
        GameObject hitPlane = hit.collider.gameObject;
        OVRSemanticClassification anchor = hitPlane.GetComponentInParent<OVRSemanticClassification>();

        if (anchor == null)
        {
            Debug.LogWarning("No OVRSemanticClassification found on the hit object.");
            UpdateDebugText("No semantic classification found.");
            return;
        }

        List<string> labelsList = new List<string>(anchor.Labels);
        Debug.Log($"Hit: {string.Join(", ", labelsList)}");

        if (labelsList.Contains("WALL_FACE"))
        {
            UpdateDebugText($"Hit: {string.Join(", ", labelsList)}");
            DisplayHitIndicator(hit.point);
            ApplyTextureToPlane(hitPlane);
        }
        else
        {
            UpdateDebugText($"Hit: {string.Join(", ", labelsList)} is not a wall");
        }
    }

    private void DisplayHitIndicator(Vector3 position)
    {
        if (hitIndicator != null)
        {
            hitIndicator.SetActive(true);
            hitIndicator.transform.position = position;
        }
    }

    private void ApplyTextureToPlane(GameObject hitPlane)
    {
        MeshRenderer planeRenderer = hitPlane.GetComponentInParent<MeshRenderer>();
        if (planeRenderer == null)
        {
            Debug.LogWarning("No MeshRenderer found on the hit plane.");
            UpdateDebugText("No MeshRenderer found.");
            return;
        }

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

        planeRenderer.material = material;
    }

    private void UpdateDebugText(string message)
    {
        if (debugText != null)
        {
            debugText.text = message;
        }
    }

    public void SwitchHand()
    {
        isLeftHanded = !isLeftHanded;
    }
}
