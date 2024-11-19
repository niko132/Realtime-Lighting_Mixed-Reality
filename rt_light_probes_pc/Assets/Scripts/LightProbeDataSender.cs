using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Net;
using System.Net.Sockets;
using System.IO;

public class LightProbeDataSender : MonoBehaviour
{

    private UdpClient udpClient = new UdpClient();
    public string udpIPAddress = "192.168.178.3"; // The IP address to send the data to
    public int udpPort = 7778; // The port to send the data to

    SphericalHarmonicsL2[] lastCoefficients = new SphericalHarmonicsL2[32];

    // Start is called before the first frame update
    void Start()
    {
        udpClient.EnableBroadcast = true;
    }

    // Update is called once per frame
    void Update()
    {
        SendLightProbeData(udpClient);
    }

    void SendLightProbeData(UdpClient udpClient)
    {
        // Get the light probe group in the scene
        LightProbeGroup lightProbeGroup = FindObjectOfType<LightProbeGroup>();
        if (lightProbeGroup == null)
        {
            Debug.LogError("No LightProbeGroup found in the scene.");
            return;
        }

        Vector3[] probePositions = lightProbeGroup.probePositions;
        int probeCount = probePositions.Length;

        // Create arrays to hold the positions and SH coefficients
        Vector3[] positions = new Vector3[probeCount];
        SphericalHarmonicsL2[] shCoefficients = new SphericalHarmonicsL2[probeCount];

        // Loop through each probe position and get the SH coefficients
        for (int i = 0; i < probeCount; i++)
        {
            positions[i] = probePositions[i];
            LightProbes.GetInterpolatedProbe(probePositions[i], null, out shCoefficients[i]);
        }

        // Serialize the data to a byte array
        for (int i = 0; i < probeCount; i++)
        {
            float norm = SHL2NormSquared(shCoefficients[i], lastCoefficients[i]);
            if (norm < 0.1f)
            {
                // Debug.Log("Skipping #" + i + " " + norm);
                continue;
            }

            byte[] data = SerializeLightProbeData(i, positions[i], shCoefficients[i]);

            // Send the data over UDP
            udpClient.Send(data, data.Length, udpIPAddress, udpPort);

            lastCoefficients[i] = shCoefficients[i];
        }
    }

    float SHL2NormSquared(SphericalHarmonicsL2 sh1, SphericalHarmonicsL2 sh2)
    {
        float sum = 0.0f;

        for (int rgb = 0; rgb < 3; rgb++)
        {
            for (int coefficient = 0; coefficient < 9; coefficient++)
            {
                sum += Mathf.Pow(sh1[rgb, coefficient] - sh2[rgb, coefficient], 2);
            }
        }

        return sum;
    }

    byte[] SerializeLightProbeData(int idx, Vector3 position, SphericalHarmonicsL2 shCoefficients)
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(memoryStream))
            {
                // Write the index
                writer.Write(idx);

                // Write SH coefficients
                for (int rgb = 0; rgb < 3; rgb++)
                {
                    for (int coefficient = 0; coefficient < 9; coefficient++)
                    {
                        writer.Write(shCoefficients[rgb, coefficient]);
                    }
                }
            }

            return memoryStream.ToArray();
        }
    }
}
