using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class LightProbeDebug : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        LightProbes lightProbes = LightmapSettings.lightProbes;
        SphericalHarmonicsL2[] existingBakedProbes = lightProbes.bakedProbes;

        Debug.Log("Num Probes: " + existingBakedProbes.Length);
    }
}
