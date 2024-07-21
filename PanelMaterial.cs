using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class PanelMaterial : MonoBehaviour
{
    private Texture2D[] textures;

    public GameObject content;

    public GameObject panel;
    // Start is called before the first frame update
    void Start()
    {
        textures = Resources.LoadAll<Texture2D>("panels");
        // Iterate through the children array and set their Image component's material
        for (int i = 0; i < textures.Length; i++)
        {
            // if(children.Length<textures.Length){
                GameObject newPannel = Instantiate(panel, content.transform);

            // }
            Image childImage = newPannel.GetComponent<Image>();
            childImage.sprite = Sprite.Create(textures[i], new Rect(0, 0, textures[i].width, textures[i].height), Vector2.one*0.5f);
        }
     }

    // Update is called once per fram
    void Update()
    {
    
    }
}





// using UnityEngine;
// using UnityEngine.UI;

// public class ScrollViewController : MonoBehaviour
// {
//     public GameObject content; // The Content GameObject in the Scroll View
//     public GameObject imagePrefab; // A prefab with an Image component to use as a template

//     void Start()
//     {
//         // Load all textures from the Resources/panels folder
//         Texture2D[] textures = Resources.LoadAll<Texture2D>("panels");

//         // Iterate over each texture and create an Image UI element for each
//         foreach (Texture2D texture in textures)
//         {
//             // Create a new instance of the image prefab
//             GameObject newImage = Instantiate(imagePrefab, content.transform);

//             // Set the texture to the Image component
//             Image imageComponent = newImage.GetComponent<Image>();
//             imageComponent.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
//         }

//         // Optionally, adjust the size of the Content to fit all images horizontally
//         RectTransform contentRect = content.GetComponent<RectTransform>();
//         GridLayoutGroup gridLayout = content.GetComponent<GridLayoutGroup>();
//         if (gridLayout != null)
//         {
//             int totalImages = textures.Length;
//             float totalWidth = totalImages * (gridLayout.cellSize.x + gridLayout.spacing.x);
//             contentRect.sizeDelta = new Vector2(totalWidth, contentRect.sizeDelta.y);
//         }
//     }
// }
