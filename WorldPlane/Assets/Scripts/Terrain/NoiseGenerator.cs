using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NoiseGenerator : MonoBehaviour
{
    public enum Block
    {
        Rock = 0,
        Grass = 1,
        Sand = 2
    }
    
    System.Random random;
    private int seed = 60;

    private Vector3 seedElevation;
    private float elevationScale = 20f;
    private int elevationOctaves = 2;
    private float elevationLacunarity = 2f;
    private float elevationPersistance = 0.5f;
    public int max_elevation;
    public AnimationCurve elevationCurve;
    private float max_elevation_const;
    
    
    //private Vector3 seedBiome;
    //private float biomeFrequency = ;
    //private float biomeFrequencyIntensity;

    public void setSeed(int seed)
    {
        this.seed = seed;
        random = new System.Random(seed);
        seedElevation = new Vector3((float)random.NextDouble() * 1000000f,0f, (float)random.NextDouble() * 1000000f);
        float amplitude = 1f;
        max_elevation_const = 0f;
        for (int i = 0; i < elevationOctaves; i++) { max_elevation_const += amplitude; amplitude *= elevationPersistance; }
    }

    /// <summary>
    /// fix so it returns float
    /// sand gen is weird if it is not implmented
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public float ElevationHeight(float x, float z)
    {
        //Debug.Log(x + " " + z);
        float noiseHeight = 0f;
        float amplitude = 1f;
        float frequency = 1f;
        for (int i = 0; i < elevationOctaves; i++)
        {
            float xSample = x / elevationScale * frequency + seedElevation.x;
            float zSample = z / elevationScale * frequency + seedElevation.z;
            noiseHeight += Mathf.PerlinNoise(xSample, zSample) * amplitude;

            amplitude *= elevationPersistance;
            frequency *= elevationLacunarity;
        }
        // fix height
        return elevationCurve.Evaluate(noiseHeight/max_elevation_const) * (float)max_elevation;
    }

    
    public Block SpawnBlock(Vector3Int position, float elevation)
    {

        if (position.y < elevation - 2f) return Block.Rock;
        if (elevation > 10f) return Block.Rock;
        if (elevation < 5f) return Block.Sand;
        return Block.Grass;
    }
    public bool isPlains(Vector3Int position, float elevation)
    {
        return false;
    }
    public bool isMountain(Vector3Int position, float elevation)
    {
        return false;
    }
    public bool isLake(Vector3Int position, float elevation)
    {
        return false;
    }
    public bool isValley(Vector3Int position, float elevation)
    {
        return false;
    }
    public Block isUndergruond(Vector3Int position, float elevation)
    {
        return Block.Rock;
    }

}
