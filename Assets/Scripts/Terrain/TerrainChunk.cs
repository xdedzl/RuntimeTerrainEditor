// ==========================================
// 描述： 
// 作者： LYG
// 时间： 2018-11-23 16:52:06
// 版本： V 1.0
// ==========================================
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class TerrainChunk : MonoBehaviour
{
    private TerrainData terrainData;

    public int X;
    public int Z;

    private Terrain terrain;

    public TerrainChunkSettings settings = new TerrainChunkSettings();

    public Texture2D FlatTexture;
    public Texture2D SteepTexture;

    private void Start()
    {
        settings.HeightmapResolution = 129;
        settings.AlphamapResolution = 256;
        settings.Length = 100230.6f;
        settings.width = 23700.3f;
        settings.Height = 1000.5f;
        settings.FlatTexture = FlatTexture;
        settings.SteepTexture = SteepTexture;

        CreateTerrain();
    }

    public void CreateTerrain()
    {
        terrainData = new TerrainData();
        terrainData.heightmapResolution = settings.HeightmapResolution;
        terrainData.alphamapResolution = settings.AlphamapResolution;

        terrainData.size = new Vector3(settings.width, settings.Height, settings.Length);
        Debug.Log(terrainData.heightmapHeight);
        
        Debug.Log(terrainData.heightmapWidth);
        var heightmap = GetHeightmap();
        terrainData.SetHeightsDelayLOD(0, 0, heightmap);
        ApplyTextures(terrainData);

        var newTerrainGameObject = Terrain.CreateTerrainGameObject(terrainData);
        newTerrainGameObject.transform.position = new Vector3(X * settings.Length, 0, Z * settings.Length);
        terrain = newTerrainGameObject.GetComponent<Terrain>();
        terrain.Flush();
    }

    private float[,] GetHeightmap()
    {
        var heightmap = new float[settings.HeightmapResolution, settings.HeightmapResolution];

        for (var zRes = 0; zRes < settings.HeightmapResolution; zRes++)
        {
            for (var xRes = 0; xRes < settings.HeightmapResolution; xRes++)
            {
                heightmap[zRes, xRes] = Random.Range(0, 1f);
            }
        }
        return heightmap;
    }

    private void ApplyTextures(TerrainData terrainData)
    {
        var flatSplat = new SplatPrototype();
        var steepSplat = new SplatPrototype();

        flatSplat.texture = settings.FlatTexture;
        steepSplat.texture = settings.SteepTexture;

        terrainData.splatPrototypes = new SplatPrototype[]
        {
            flatSplat,
            steepSplat
        };

        terrainData.RefreshPrototypes();

        var splatMap = new float[terrainData.alphamapResolution, terrainData.alphamapResolution, 2];

        for (var zRes = 0; zRes < terrainData.alphamapHeight; zRes++)
        {
            for (var xRes = 0; xRes < terrainData.alphamapWidth; xRes++)
            {
                var normalizedX = (float)xRes / (terrainData.alphamapWidth - 1);
                var normalizedZ = (float)zRes / (terrainData.alphamapHeight - 1);

                var steepness = terrainData.GetSteepness(normalizedX, normalizedZ);
                var steepnessNormalized = Mathf.Clamp(steepness / 1.5f, 0, 1f);

                splatMap[zRes, xRes, 0] = 1f - steepnessNormalized;
                splatMap[zRes, xRes, 1] = steepnessNormalized;
            }
        }
        terrainData.SetAlphamaps(0, 0, splatMap);
    }

    public class TerrainChunkSettings
    {
        public int HeightmapResolution;
        public int AlphamapResolution;

        public float Length;
        public float width;
        public float Height;

        public Texture2D FlatTexture;
        public Texture2D SteepTexture;

    }
}