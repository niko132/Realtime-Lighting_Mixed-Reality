using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ProjectionUV : MonoBehaviour
{
    public GameObject TextureRenderer;
    public List<GameObject> Objects;
    public List<Plane> TextureMaps;
    public bool MapUvButton = false;

    [Range(0.0f, 1.0f)]
    public float x;
    [Range(0.0f, 1.0f)]
    public float y;

    public Vector3 RotationOffset;

    // Tolerance for considering vertices as identical
    private float dupMergeTolerance = 0.0001f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // Gets called when the mapping button was pressed
    void OnValidate()
    {
        if (MapUvButton)
        {
            MapUVs();
            MapUvButton = false;
        }
    }

    // Maps the UVs of all the objects in the list
    void MapUVs()
    {
        foreach (GameObject gameObject in Objects)
        {
            MapUV(gameObject);

        }
    }

    // maps single object and all its children
    void MapUV(GameObject gameObject)
    {
        CreateUvMap(gameObject);
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            MapUV(gameObject.transform.GetChild(i).gameObject);
        }
    }

    void CreateUvMap(GameObject gameObject)
    {
        if (gameObject.GetComponent<MeshFilter>() == null) return;

        Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;

        // merge duplicate vertices from previous mappings
        // NormalizeMesh(mesh);

        // calculate the UVs without post-correction
        CalculateUVs(mesh, gameObject);

        // correct the edges that span over the edge of the UV map
        // using a vertex duplication strategy
        FixUVSeam(mesh, gameObject);
    }

    // Normalize mesh by merging vertices within the specified tolerance
    public void NormalizeMesh(Mesh mesh)
    {
        Vector3[] originalVertices = mesh.vertices;
        int[] originalTriangles = mesh.triangles;

        Dictionary<Vector3, int> uniqueVertices = new Dictionary<Vector3, int>(new Vector3Comparer(dupMergeTolerance));
        List<Vector3> newVertices = new List<Vector3>();
        List<int> newTriangles = new List<int>();

        for (int i = 0; i < originalTriangles.Length; i++)
        {
            Vector3 vertex = originalVertices[originalTriangles[i]];

            if (!uniqueVertices.ContainsKey(vertex))
            {
                uniqueVertices[vertex] = newVertices.Count;
                newVertices.Add(vertex);
            }

            newTriangles.Add(uniqueVertices[vertex]);
        }

        mesh.Clear();
        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    // Custom comparer for Vector3 that uses a tolerance for equality
    private class Vector3Comparer : IEqualityComparer<Vector3>
    {
        private float tolerance;

        public Vector3Comparer(float tolerance)
        {
            this.tolerance = tolerance;
        }

        public bool Equals(Vector3 v1, Vector3 v2)
        {
            return (v1 - v2).sqrMagnitude < tolerance * tolerance;
        }

        public int GetHashCode(Vector3 obj)
        {
            return obj.GetHashCode();
        }
    }

    // Calculate the UV coordinates of every vertex depending on its world position
    // and the center of projection
    public void CalculateUVs(Mesh mesh, GameObject gameObject)
    {
        Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = new Vector2[vertices.Length];

        for (int i = 0; i < uvs.Length; i++)
        {
            Vector3 worldPosition = gameObject.transform.TransformPoint(vertices[i]);
            Vector3 sphereCenter = TextureRenderer.transform.position;

            Vector2 uv = GetSphereUV(worldPosition, sphereCenter);
            uv = new Vector2(uv.x, 1.0f - uv.y);
            uvs[i] = uv;
        }

        mesh.uv = uvs;
    }

    // Fix edges overlapping the edge of the UV map by duplicating the
    // problematic vertices
    public void FixUVSeam(Mesh mesh, GameObject gameObject)
    {
        HashSet<int> shouldDuplicate = new HashSet<int>();

        Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = mesh.uv;
        int[] triangles = mesh.triangles;

        HashSet<int> trianglePositionsToUpdate = new HashSet<int>();

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int idx1 = triangles[i];
            int idx2 = triangles[i + 1];
            int idx3 = triangles[i + 2];

            float dist12 = -(uvs[idx1].x - uvs[idx2].x);
            float dist23 = -(uvs[idx2].x - uvs[idx3].x);
            float dist31 = -(uvs[idx3].x - uvs[idx1].x);

            float angle12 = GetAngleBetweenPoints(gameObject.transform.TransformPoint(vertices[idx1]), gameObject.transform.TransformPoint(vertices[idx2]), TextureRenderer.transform.position);
            float angle23 = GetAngleBetweenPoints(gameObject.transform.TransformPoint(vertices[idx2]), gameObject.transform.TransformPoint(vertices[idx3]), TextureRenderer.transform.position);
            float angle31 = GetAngleBetweenPoints(gameObject.transform.TransformPoint(vertices[idx3]), gameObject.transform.TransformPoint(vertices[idx1]), TextureRenderer.transform.position);

            // If we move clockwise from an angle POV but we go to the left in the UV space,
            // we found a vertex to copy
            // The Mathf.Abs() solution is not clean but without it we cut too many edges
            if ((angle12 >= 0.0f) ^ (dist12 >= 0.0f))
            {
                shouldDuplicate.Add(dist12 >= 0.0f ? idx1 : idx2);
                if (Mathf.Abs(dist12) > 0.5f) trianglePositionsToUpdate.Add(dist12 >= 0.0f ? i : (i + 1));
            }

            if ((angle23 >= 0.0f) ^ (dist23 >= 0.0f))
            {
                shouldDuplicate.Add(dist23 >= 0.0f ? idx2 : idx3);
                if (Mathf.Abs(dist23) > 0.5f) trianglePositionsToUpdate.Add(dist23 >= 0.0f ? (i + 1) : (i + 2));
            }

            if ((angle31 >= 0.0f) ^ (dist31 >= 0.0f))
            {
                shouldDuplicate.Add(dist31 >= 0.0f ? idx3 : idx1);
                if (Mathf.Abs(dist31) > 0.5f) trianglePositionsToUpdate.Add(dist31 >= 0.0f ? (i + 2) : i);
            }
        }

        List<int> duplicates = shouldDuplicate.ToList();

        // Create new arrays and populate them with the previous data
        Vector3[] newVertices = new Vector3[vertices.Length + duplicates.Count];
        Vector2[] newUvs = new Vector2[newVertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            newVertices[i] = vertices[i];
            newUvs[i] = uvs[i];
        }

        int[] newTriangles = new int[triangles.Length];
        for (int i = 0; i < triangles.Length; i++)
        {
            newTriangles[i] = triangles[i];
        }

        for (int i = 0; i < duplicates.Count; i++)
        {
            newVertices[vertices.Length + i] = vertices[duplicates[i]];
            newUvs[vertices.Length + i] = new Vector2(uvs[duplicates[i]].x + 1.0f, uvs[duplicates[i]].y);
        }

        foreach (var idx in trianglePositionsToUpdate)
        {
            newTriangles[idx] = vertices.Length + duplicates.IndexOf(triangles[idx]);
        }

        mesh.vertices = newVertices;
        mesh.triangles = newTriangles;
        mesh.uv = newUvs;
    }

    public float GetAngleBetweenPoints(Vector3 p1, Vector3 p2, Vector3 center)
    {
        Vector3 normalizedP1 = (p1 - center).normalized;
        normalizedP1 = Quaternion.Euler(RotationOffset) * normalizedP1;
        Vector3 projectedP1 = new Vector3(normalizedP1.x, 0.0f, normalizedP1.z);

        Vector3 normalizedP2 = (p2 - center).normalized;
        normalizedP2 = Quaternion.Euler(RotationOffset) * normalizedP2;
        Vector3 projectedP2 = new Vector3(normalizedP2.x, 0.0f, normalizedP2.z);

        float angle = Vector3.SignedAngle(projectedP1, projectedP2, new Vector3(0.0f, 1.0f, 0.0f));
        return angle;
    }

    public Vector2 GetSphereUV(Vector3 point, Vector3 sphereCenter)
    {
        Vector3 normalizedPoint = (point - sphereCenter).normalized;
        normalizedPoint = Quaternion.Euler(RotationOffset) * normalizedPoint;

        float u = 1.0f - (0.5f + Mathf.Atan2(normalizedPoint.z, normalizedPoint.x) / (2.0f * Mathf.PI) + x);
        float v = 0.5f - Mathf.Asin(normalizedPoint.y) / Mathf.PI + y;
        return new Vector2(u, v);
    }
}
