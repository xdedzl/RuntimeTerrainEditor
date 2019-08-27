using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float meshHeightMultiplier, AnimationCurve _heightCurver, int levelOfDetail)
    {
        // 多线程同时调用Evaluate时会返回一个错误的值
        AnimationCurve heightCurver = new AnimationCurve(_heightCurver.keys);

        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        int meshSimplificationIncrement = levelOfDetail == 0 ? 1 : levelOfDetail * 2;
        int verticesPerline = (width - 1) / meshSimplificationIncrement + 1;

        MeshData meshData = new MeshData(verticesPerline, verticesPerline);
        int vertexIndex = 0;

        for (int y = 0; y < height; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < width; x += meshSimplificationIncrement)
            {
                
                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, heightCurver.Evaluate(heightMap[x, y]) * meshHeightMultiplier, topLeftZ - y);
                meshData.uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);

                if(x < width - 1 && y < height - 1)
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerline + 1, vertexIndex + verticesPerline);
                    meshData.AddTriangle(vertexIndex + verticesPerline + 1, vertexIndex, vertexIndex + 1);
                }

                vertexIndex++;
            }
        }

        return meshData;
    }
}

public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    private int triangleIndex;

    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex++] = a;
        triangles[triangleIndex++] = b;
        triangles[triangleIndex++] = c;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        mesh.name = "terrain";
        return mesh;
    }
}