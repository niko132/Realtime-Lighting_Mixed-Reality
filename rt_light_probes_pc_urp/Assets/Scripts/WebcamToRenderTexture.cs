using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebcamToRenderTexture : MonoBehaviour
{
    public RenderTexture outputTexture;
    public List<GameObject> emissiveObjects;
    public string cameraDeviceName = "HP";
    public Texture image;

    public bool enableWebcam = false;
    public bool enableImage = false;

    private WebCamTexture webCamTexture;

    // Start is called before the first frame update
    void Start()
    {
        if (!enableWebcam) return;

        WebCamDevice[] devices = WebCamTexture.devices;
        WebCamDevice device = devices[0];

        // for debugging purposes, prints available devices to the console
        for (int i = 0; i < devices.Length; i++)
        {
            print("Webcam available: " + devices[i].name);
            if (devices[i].name.Contains(cameraDeviceName))
            {
                device = devices[i];
            }
        }

        webCamTexture = new WebCamTexture(device.name);
        webCamTexture.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (enableWebcam)
        {
            Graphics.Blit(webCamTexture, outputTexture);
        }
        if (enableImage)
        {
            Graphics.Blit(image, outputTexture);
        }
        foreach (GameObject emissiveObject in emissiveObjects)
        {
            UpdateEmissiveMaterial(emissiveObject);

            for (int i = 0; i < emissiveObject.transform.childCount; i++)
            {
                UpdateEmissiveMaterial(emissiveObject.transform.GetChild(i).gameObject);
            }
        }

        DynamicGI.UpdateEnvironment();
    }

    void UpdateEmissiveMaterial(GameObject go)
    {
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer == null) return;

        RendererExtensions.UpdateGIMaterials(renderer);
    }
}
