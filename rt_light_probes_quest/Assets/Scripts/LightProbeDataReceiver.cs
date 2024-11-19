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

public class LightProbeDataReceiver : MonoBehaviour
{
    private UdpClient _ReceiveClient;
    private Thread _ReceiveThread;

    public int udpPort = 7778; // The port to send the data to

    [Range(1.0f, 50000.0f)]
    public float divider = 10000.0f;

    private SphericalHarmonicsL2[] shCoefficients;
    private bool needsUpdate = false;


    void Start()
    {
        int count = LightmapSettings.lightProbes.count;
        shCoefficients = new SphericalHarmonicsL2[count];
        InitializeReceivingThread();
    }

    void Update()
    {
        if (needsUpdate == true)
        {
            // Update SH coefficients of existing light probes
            UpdateLightProbeSHCoefficients(shCoefficients);
            needsUpdate = false;
        }
    }

    /// <summary>
    /// Initialize objects.
    /// </summary>
    public void InitializeReceivingThread()
    {
        // Receive
        _ReceiveThread = new Thread(
            new ThreadStart(ReceiveData));
        _ReceiveThread.IsBackground = true;
        _ReceiveThread.Start();
    }

    /// <summary>
    /// Receive data over UDP.
    /// </summary>
    private void ReceiveData()
    {
        _ReceiveClient = new UdpClient(udpPort);
        _ReceiveClient.EnableBroadcast = true;

        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = _ReceiveClient.Receive(ref anyIP);

                DeserializeLightProbeData(data);
            }
            catch (Exception err)
            {
                Debug.Log("<color=red>" + err.Message + "</color>");
            }
        }
    }

    void DeserializeLightProbeData(byte[] data)
    {
        using (MemoryStream memoryStream = new MemoryStream(data))
        {
            using (BinaryReader reader = new BinaryReader(memoryStream))
            {
                // Read the light probe index
                int idx = reader.ReadInt32();

                // Read SH coefficients
                SphericalHarmonicsL2 sh = new SphericalHarmonicsL2();
                for (int rgb = 0; rgb < 3; rgb++)
                {
                    for (int coefficient = 0; coefficient < 9; coefficient++)
                    {
                        sh[rgb, coefficient] = reader.ReadSingle();
                    }
                }
                shCoefficients[idx] = sh;
                needsUpdate = true;
            }
        }
    }

    void UpdateLightProbeSHCoefficients(SphericalHarmonicsL2[] shCoefficients)
    {
        LightProbes lightProbes = LightmapSettings.lightProbes;
        SphericalHarmonicsL2[] existingBakedProbes = lightProbes.bakedProbes;

        for (int i = 0; i < existingBakedProbes.Length; i++)
        {
            existingBakedProbes[i] = shCoefficients[i] * (1.0f / divider);
        }

        // Update the light probes in LightmapSettings
        LightmapSettings.lightProbes.bakedProbes = existingBakedProbes;
    }

    /// <summary>
    /// Deinitialize everything on quiting the application.Or you might get error in restart.
    /// </summary>
    private void OnApplicationQuit()
    {
        try
        {
            _ReceiveThread.Abort();
            _ReceiveThread = null;
            _ReceiveClient.Close();
        }
        catch (Exception err)
        {
            Debug.Log("<color=red>" + err.Message + "</color>");
        }
    }
}
