using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private int mapSizeInChunks = 1;
    [SerializeField] private GameObject chunkPrefab;

    public int numberOfOctaves = 4;
    public float heightMultiplier = 100f;
    public float frequencyScale = 0.005f;
    public float lacunarity = 2f;
    public float persistence = 0.5f;

    private List<GameObject> chunkGOs = new List<GameObject>();

    private void Start()
    {
        GenerateMap();
    }

    public void RegenerateMap()
    {
        DestroyMap();
        GenerateMap();
    }

    private void GenerateMap()
    {
        for (int i = 0; i < mapSizeInChunks; i++)
        {
            for (int j = 0; j < mapSizeInChunks; j++)
            {
                int chunkSize = ChunkGenerator.chunkSize;
                Vector3 chunkPosition = new Vector3(i * chunkSize, 0, j * chunkSize);
                ChunkGenerator chunkGenerator = Instantiate(
                    chunkPrefab,
                    chunkPosition,
                    Quaternion.identity)
                    .GetComponent<ChunkGenerator>();

                chunkGenerator.frequencyScale = this.frequencyScale;
                chunkGenerator.heightMultiplier = this.heightMultiplier;
                chunkGenerator.numberOfOctaves = this.numberOfOctaves;
                chunkGenerator.lacunarity = this.lacunarity;
                chunkGenerator.persistence = this.persistence;
                chunkGenerator.GenerateChunk(new Vector2(chunkPosition.x, chunkPosition.z));
                chunkGOs.Add(chunkGenerator.gameObject);
            }
        }
    }

    private void DestroyMap()
    {
        foreach (GameObject chunk in chunkGOs)
        {
            Destroy(chunk);
        }
    }
}
