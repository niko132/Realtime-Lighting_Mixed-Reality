using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Globalization;
using UnityEngine.Rendering;
using System.IO;

public class LightProbeDebug : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {
        LightProbes lightProbes = LightmapSettings.lightProbes;
        SphericalHarmonicsL2[] existingBakedProbes = lightProbes.bakedProbes;

        Debug.Log("Num Probes: " + existingBakedProbes.Length);
    }
}
