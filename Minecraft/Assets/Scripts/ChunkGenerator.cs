using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour
{
    public enum BLOCKTYPE
    {
        AIR = 0, //ezzel lesz tele az üres tömb
        DIRT = 1
    }

    private Mesh mesh;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private int triangleIndex = 0;

    public int numberOfOctaves = 4;
    public float heightMultiplier = 100f;
    public float frequencyScale = 0.005f;
    public float lacunarity = 2f;
    public float persistence = 0.5f;

    public const int chunkSize = 16;
    public const int heightLimit = 255;

    //perlin noise gives the same value for whole numbers,
    //this is why I need this
    private const float correction = 0.001f;

    private BLOCKTYPE[,,] blocks = new BLOCKTYPE[chunkSize, 255, chunkSize];//[x, y, z]

    public void GenerateChunk(Vector2 position)
    {
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = this.mesh;
        CreateMesh(position);
        UpdateMesh();
    }

    private void CreateMesh(Vector2 position)
    {
        triangleIndex = 0;
        GenerateBlocks(position);
        AddBlocksToMesh();
    }

    private void GenerateBlocks(Vector2 position)
    {
        for (int i = 0; i < chunkSize; i++)
        {
            for (int j = 0; j < chunkSize; j++)
            {
                int cubeXPos = i + (int)position.x;
                int cubeZPos = j + (int)position.y;
                Vector3 blockPosition = new Vector3(
                        i,
                        GenerateHeightViaPerlinNoise(
                            i + (int)transform.position.x,
                            j + (int)transform.position.z,
                            numberOfOctaves) * heightMultiplier,
                        j);
                Vector3Int finalizedBlockPosition = GenerateBlock(blockPosition);
                GenerateBlocksUnderThisBlock(finalizedBlockPosition);
            }
        }
    }

    private void GenerateBlocksUnderThisBlock(Vector3Int blockPosition)
    {
        int from = blockPosition.y - 1;
        int to = 0;
        for (int i = from; i >= to; i--)
        {
            GenerateBlock(new Vector3(blockPosition.x, i, blockPosition.z));
        }
    }

    private void AddBlocksToMesh()
    {
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < 255; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    if(blocks[x, y, z] == BLOCKTYPE.DIRT)
                    {
                        Vector3Int blockPosition = new Vector3Int(x, y, z);
                        List<VoxelData.Face> facesToDraw = GetWhichFacesToDrawForThisCube(blockPosition);
                        AddBlockToMeshAtPosition(blockPosition, facesToDraw);
                    }
                }
            }
        }
    }

    private List<VoxelData.Face> GetWhichFacesToDrawForThisCube(Vector3Int blockPosition)
    {
        List<VoxelData.Face> facesToDraw = new List<VoxelData.Face>();
        int maxX = chunkSize - 1;
        int minX = 0;
        int maxY = heightLimit - 1;
        int minY = 0;
        int maxZ = chunkSize - 1;
        int minZ = 0;

        if (blockPosition.y + 1 > maxY)
        {
            facesToDraw.Add(VoxelData.Face.TOP);
        }
        else if(blocks[blockPosition.x, blockPosition.y + 1, blockPosition.z] == BLOCKTYPE.AIR)
        {
            facesToDraw.Add(VoxelData.Face.TOP);
        }

        if (blockPosition.y - 1 < minY)
        {
            facesToDraw.Add(VoxelData.Face.BOTTOM);
        }
        else if (blocks[blockPosition.x, blockPosition.y - 1, blockPosition.z] == BLOCKTYPE.AIR)
        {
            facesToDraw.Add(VoxelData.Face.BOTTOM);
        }

        if (blockPosition.x - 1 < minX)
        {
            facesToDraw.Add(VoxelData.Face.LEFT);
        }
        else if (blocks[blockPosition.x - 1, blockPosition.y, blockPosition.z] == BLOCKTYPE.AIR)
        {
            facesToDraw.Add(VoxelData.Face.LEFT);
        }

        if (blockPosition.x + 1 > maxX)
        {
            facesToDraw.Add(VoxelData.Face.RIGHT);
        }
        else if (blocks[blockPosition.x + 1, blockPosition.y, blockPosition.z] == BLOCKTYPE.AIR)
        {
            facesToDraw.Add(VoxelData.Face.RIGHT);
        }

        if (blockPosition.z - 1 < minZ)
        {
            facesToDraw.Add(VoxelData.Face.FRONT);
        }
        else if (blocks[blockPosition.x, blockPosition.y, blockPosition.z - 1] == BLOCKTYPE.AIR)
        {
            facesToDraw.Add(VoxelData.Face.FRONT);
        }

        if (blockPosition.z + 1 > maxZ)
        {
            facesToDraw.Add(VoxelData.Face.BACK);
        }
        else if (blocks[blockPosition.x, blockPosition.y, blockPosition.z + 1] == BLOCKTYPE.AIR)
        {
            facesToDraw.Add(VoxelData.Face.BACK);
        }

        return facesToDraw;
    }

    private void AddBlockToMeshAtPosition(Vector3 position, List<VoxelData.Face> facesToDraw)
    {
        foreach (VoxelData.Face face in facesToDraw)
        {
            int[] faceInfo = VoxelData.Faces[(int)face];
            for (int i = 0; i < faceInfo.Length; i++)
            {
                int vertexIndex = faceInfo[i];
                Vector3 vertex = TranslateVertex(position, VoxelData.Vertices[vertexIndex]);
                vertices.Add(vertex);
                triangles.Add(triangleIndex);
                triangleIndex++;
            }
        }
    }

    private float GenerateHeightViaPerlinNoise(int x, int y, int numberOfOctavesToUse)
    {
        float result = 0f;
        for (int i = 0; i < numberOfOctavesToUse; i++)
        {
            result += Mathf.PerlinNoise(
                (x * getFrequencyForOctave(i)) + correction,
                (y * getFrequencyForOctave(i)) + correction)
                * getAmplitudeForOctave(i);
        }

        return result;
    }

    private float getFrequencyForOctave(int numberOfOctave)
    {
        return Mathf.Pow(lacunarity, numberOfOctave) * frequencyScale;
    }

    private float getAmplitudeForOctave(int numberOfOctave)
    {
        return Mathf.Pow(persistence, numberOfOctave);
    }

    private Vector3 TranslateVertex(Vector3 translate, Vector3 vertex)
    {
        vertex.x += translate.x;
        vertex.y += translate.y;
        vertex.z += translate.z;
        return vertex;
    }

    private void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = this.vertices.ToArray();
        mesh.triangles = this.triangles.ToArray();
        mesh.RecalculateNormals();
    }

    private Vector3Int GenerateBlock(Vector3 blockPosition)
    {
        Vector3Int blockPositionInt = new Vector3Int(
                Mathf.RoundToInt(blockPosition.x),
                Mathf.Clamp(Mathf.RoundToInt(blockPosition.y), 0, heightLimit - 1),
                Mathf.RoundToInt(blockPosition.z)
            );
        blocks[blockPositionInt.x, blockPositionInt.y, blockPositionInt.z] = BLOCKTYPE.DIRT;
        return blockPositionInt;
    }
}
