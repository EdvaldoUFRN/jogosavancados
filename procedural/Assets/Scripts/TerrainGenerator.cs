using UnityEngine;
using System.Collections.Generic;

public class TerrainGenerator : MonoBehaviour
{
    [Header("Terrain Settings")]
    public int chunkSize = 16; // Tamanho de cada bloco (chunk)
    public int terrainHeight = 10; // Altura máxima do terreno
    public float noiseScale = 0.1f;
    public int octaves = 4;
    public float persistence = 0.5f;
    public float lacunarity = 2.0f;

    [Header("Dynamic Loading")]
    public int viewDistance = 1; // Raio em chunks (1 = matriz 3x3, com 9 blocos ao redor)
    public Transform player;

    [Header("Prefabs")]
    public GameObject treePrefab;
    public GameObject caveEntrancePrefab;

    private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();

    void Update()
    {
        UpdateChunks();
    }

    private void UpdateChunks()
    {
        Vector2Int playerChunk = GetChunkPosition(player.position);

        // Gerar os 9 blocos ao redor do jogador (matriz 3x3)
        for (int x = -viewDistance; x <= viewDistance; x++)
        {
            for (int z = -viewDistance; z <= viewDistance; z++)
            {
                Vector2Int chunkCoord = new Vector2Int(playerChunk.x + x, playerChunk.y + z);

                // Verifica se o chunk já foi gerado
                if (!activeChunks.ContainsKey(chunkCoord))
                {
                    CreateChunk(chunkCoord); // Cria o novo chunk
                    Debug.Log($"Chunk gerado em: {chunkCoord.x}, {chunkCoord.y}"); // Imprime as coordenadas dos chunks
                }
            }
        }

        // Remover blocos que estão fora da distância de visão
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        foreach (var chunk in activeChunks)
        {
            if (Vector2Int.Distance(chunk.Key, playerChunk) > viewDistance)
            {
                chunksToRemove.Add(chunk.Key);
            }
        }

        foreach (var chunkCoord in chunksToRemove)
        {
            Destroy(activeChunks[chunkCoord]);
            activeChunks.Remove(chunkCoord);
        }

        // Debug: Verifique se a quantidade de blocos gerados é 9
        Debug.Log($"Total de blocos gerados: {activeChunks.Count}");
    }

    private void CreateChunk(Vector2Int chunkCoord)
    {
        GameObject chunk = new GameObject($"Chunk_{chunkCoord.x}_{chunkCoord.y}");
        chunk.transform.position = new Vector3(chunkCoord.x * chunkSize, 0, chunkCoord.y * chunkSize);

        MeshFilter meshFilter = chunk.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = chunk.AddComponent<MeshRenderer>();

        // Cria um novo material e define a cor marrom
        Material material = new Material(Shader.Find("Standard"));
        material.color = new Color(0.6f, 0.3f, 0.1f); // Cor marrom
        meshRenderer.material = material;

        MeshCollider meshCollider = chunk.AddComponent<MeshCollider>();

        meshFilter.mesh = GenerateTerrainMesh(chunkCoord);
        meshCollider.sharedMesh = meshFilter.mesh;

        GenerateElements(chunk);

        activeChunks.Add(chunkCoord, chunk);
    }

    private Mesh GenerateTerrainMesh(Vector2Int chunkCoord)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[(chunkSize + 1) * (chunkSize + 1)];
        int[] triangles = new int[chunkSize * chunkSize * 6];

        for (int z = 0; z <= chunkSize; z++)
        {
            for (int x = 0; x <= chunkSize; x++)
            {
                int index = z * (chunkSize + 1) + x;

                float height = GetHeight(chunkCoord.x * chunkSize + x, chunkCoord.y * chunkSize + z);
                vertices[index] = new Vector3(x, height, z);
            }
        }

        int triIndex = 0;
        for (int z = 0; z < chunkSize; z++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                int start = z * (chunkSize + 1) + x;

                triangles[triIndex++] = start;
                triangles[triIndex++] = start + chunkSize + 1;
                triangles[triIndex++] = start + 1;

                triangles[triIndex++] = start + 1;
                triangles[triIndex++] = start + chunkSize + 1;
                triangles[triIndex++] = start + chunkSize + 2;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    private float GetHeight(int x, int z)
    {
        float height = 0;
        float frequency = noiseScale;
        float amplitude = 1;

        for (int i = 0; i < octaves; i++)
        {
            float perlinValue = Mathf.PerlinNoise(x * frequency, z * frequency);
            height += perlinValue * amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return height * terrainHeight;
    }

    private Vector2Int GetChunkPosition(Vector3 position)
    {
        return new Vector2Int(Mathf.FloorToInt(position.x / chunkSize), Mathf.FloorToInt(position.z / chunkSize));
    }

    private void GenerateElements(GameObject chunk)
    {
        // Lista para armazenar as posições ocupadas no chunk
        List<Vector3> occupiedPositions = new List<Vector3>();
        float minimumDistance = 9.0f; // Distância mínima entre objetos

        int treeCount = 0; // Contador de árvores

        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                // Calcula o valor de ruído para árvore e caverna
                float noiseValueForTrees = Mathf.PerlinNoise((chunk.transform.position.x + x) * 1f, (chunk.transform.position.z + z) * 0.1f);
                float noiseValueForCaves = Mathf.PerlinNoise((chunk.transform.position.x + x) * 0.1f, (chunk.transform.position.z + z) * 0.1f);

                // Condição para árvores
                if (noiseValueForTrees < 0.2f) // Árvores
                {
                    float height = GetHeight((int)chunk.transform.position.x + x, (int)chunk.transform.position.z + z);
                    Vector3 position = new Vector3(x, height, z);

                    // Adiciona altura dinâmica para as árvores
                    float treeHeightOffset = Random.Range(3f, 6f); // Altura adicional aleatória
                    position.y += treeHeightOffset;

                    // Checa se a posição é longe o suficiente de outras
                    bool isFarEnough = true;
                    foreach (Vector3 occupied in occupiedPositions)
                    {
                        if (Vector3.Distance(occupied, position) < minimumDistance)
                        {
                            isFarEnough = false;
                            break;
                        }
                    }

                    // Se a posição for válida, cria a árvore
                    if (isFarEnough)
                    {
                        GameObject tree = Instantiate(treePrefab, chunk.transform.position + position, Quaternion.identity, chunk.transform);
                        occupiedPositions.Add(position);

                        treeCount++;

                        // Verifica se é a vez de gerar uma caverna a cada 8 árvores
                        if (treeCount % 8 == 0)
                        {
                            GenerateCaveEntrance(chunk, occupiedPositions);
                        }
                    }
                }

                // Condição para cavernas
                if (noiseValueForCaves < 0.1f) // Caverna
                {
                    float height = GetHeight((int)chunk.transform.position.x + x, (int)chunk.transform.position.z + z);
                    Vector3 position = new Vector3(x, height, z);

                    // Verifica se a posição está suficientemente distante de outras
                    bool isFarEnough = true;
                    foreach (Vector3 occupied in occupiedPositions)
                    {
                        if (Vector3.Distance(occupied, position) < minimumDistance)
                        {
                            isFarEnough = false;
                            break;
                        }
                    }

                    // Se a posição for válida, cria a entrada da caverna
                    if (isFarEnough)
                    {
                        Instantiate(caveEntrancePrefab, chunk.transform.position + position, Quaternion.identity, chunk.transform);
                        occupiedPositions.Add(position);
                    }
                }
            }
        }
    }

    private void GenerateCaveEntrance(GameObject chunk, List<Vector3> occupiedPositions)
    {
        // Geração de caverna
        Vector3 cavePosition = new Vector3(Random.Range(0, chunkSize), GetHeight((int)chunk.transform.position.x, (int)chunk.transform.position.z), Random.Range(0, chunkSize));
        Instantiate(caveEntrancePrefab, chunk.transform.position + cavePosition, Quaternion.identity, chunk.transform);
        occupiedPositions.Add(cavePosition);
    }

    
}
