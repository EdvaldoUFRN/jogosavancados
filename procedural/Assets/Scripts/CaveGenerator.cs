using UnityEngine;
using System.Collections.Generic;

public class CaveGenerator : MonoBehaviour
{
    [Header("Cave Settings")]
    public int caveWidth = 20;
    public int caveHeight = 10;
    public float noiseScale = 0.1f;
    public float threshold = 0.5f;

    [Header("Item Settings")]
    public GameObject collectiblePrefab;
    public int collectibleCount = 10;

    private List<Vector3> collectiblePositions = new List<Vector3>();

    void Start()
    {
        GenerateCave();
        GenerateCollectibles();
    }

    private void GenerateCave()
    {
        Mesh mesh = new Mesh();
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();

        meshRenderer.material = new Material(Shader.Find("Standard"));

        Vector3[] vertices = new Vector3[caveWidth * caveHeight];
        int[] triangles = new int[(caveWidth - 1) * (caveHeight - 1) * 6];

        int triIndex = 0;
        for (int y = 0; y < caveHeight; y++)
        {
            for (int x = 0; x < caveWidth; x++)
            {
                int index = y * caveWidth + x;
                float noiseValue = Mathf.PerlinNoise(x * noiseScale, y * noiseScale);
                float height = noiseValue > threshold ? 1 : 0;

                vertices[index] = new Vector3(x, height, y);

                if (x < caveWidth - 1 && y < caveHeight - 1)
                {
                    triangles[triIndex++] = index;
                    triangles[triIndex++] = index + caveWidth;
                    triangles[triIndex++] = index + 1;

                    triangles[triIndex++] = index + 1;
                    triangles[triIndex++] = index + caveWidth;
                    triangles[triIndex++] = index + caveWidth + 1;
                }
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    private void GenerateCollectibles()
    {
        System.Random random = new System.Random();

        for (int i = 0; i < collectibleCount; i++)
        {
            int x = random.Next(0, caveWidth);
            int z = random.Next(0, caveHeight);

            float noiseValue = Mathf.PerlinNoise(x * noiseScale, z * noiseScale);
            if (noiseValue > threshold)
            {
                Vector3 position = new Vector3(x, 1, z);
                collectiblePositions.Add(position);
                Instantiate(collectiblePrefab, position, Quaternion.identity, transform);
            }
        }
    }
}
