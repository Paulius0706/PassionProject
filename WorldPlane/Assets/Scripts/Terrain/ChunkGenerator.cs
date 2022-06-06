using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ChunkGenerator : MonoBehaviour
{

    

    public Transform target;
    public GameObject chunkPrefab;

    public int seed;
    public int chunkSize = 16;
    public int loadChunkDistance = 2;


    System.Random random;
    public NoiseGenerator noise;
    public Vector3Int targetChunkCord;



    private Dictionary<Vector3Int, GameObject> loadedChunks;
    private Dictionary<Vector3Int, Dictionary<Vector3Int, string>> changes;

    // Start is called before the first frame update
    void Start()
    {
        changes = new Dictionary<Vector3Int, Dictionary<Vector3Int, string>>();
        loadedChunks = new Dictionary<Vector3Int, GameObject>();
        noise.setSeed(seed);
    }

    // Update is called once per frame
    void Update()
    {
        targetChunkCord = new Vector3Int(Mathf.FloorToInt(target.position.x / chunkSize),0, Mathf.FloorToInt(target.position.z / chunkSize));

        for (int x =- loadChunkDistance; x <= loadChunkDistance; x++)
        {
            for (int z = -loadChunkDistance; z <= loadChunkDistance; z++)
            {
                int trueX = x + targetChunkCord.x;
                int trueZ = z + targetChunkCord.z;
                if (!loadedChunks.ContainsKey(new Vector3Int(trueX, 0, trueZ)))
                {
                    GameObject chunk = Instantiate(chunkPrefab, new Vector3(trueX * chunkSize, 0, trueZ * chunkSize), Quaternion.identity);
                    chunk.GetComponent<ChunkController>().ChunkSet(new Vector3Int(trueX, 0, trueZ), gameObject.GetComponent<ChunkGenerator>());
                    chunk.transform.parent = gameObject.transform;
                    loadedChunks.Add(new Vector3Int(trueX, 0, trueZ), chunk);
                    loadedChunks[new Vector3Int(trueX, 0, trueZ)].GetComponent<ChunkController>().Generate();
                    StichEdges(new Vector3Int(trueX, 0, trueZ));
                    if(!changes.ContainsKey(new Vector3Int(trueX, 0, trueZ))) changes.Add(new Vector3Int(trueX, 0, trueZ), new Dictionary<Vector3Int, string>());
                }
            }
        }
        int i = 0;
        Vector3Int[] keys = new Vector3Int[loadedChunks.Count];
        foreach (Vector3Int key in loadedChunks.Keys) keys[i++] = key;
        foreach (Vector3Int key in keys)
        {
            if (Mathf.Abs(targetChunkCord.x - loadedChunks[key].GetComponent<ChunkController>().position.x) > loadChunkDistance+1 
                || Mathf.Abs(targetChunkCord.z - loadedChunks[key].GetComponent<ChunkController>().position.z) > loadChunkDistance+1)
            {
                loadedChunks[key].GetComponent<ChunkController>().DestroyChunk();
                loadedChunks.Remove(key);
            }
        }
        Debug.Log(changes[Vector3Int.zero].Count);
    }
    
    public void StichEdges(Vector3Int cord)
    {
        if (loadedChunks.ContainsKey(cord + Vector3Int.forward)) {
            loadedChunks[cord                     ].GetComponent<ChunkController>().StichChunkRoughMeshes(ChunkController.Edges.up);
            loadedChunks[cord + Vector3Int.forward].GetComponent<ChunkController>().StichChunkRoughMeshes(ChunkController.Edges.down);
        }
        if (loadedChunks.ContainsKey(cord + Vector3Int.back))
        {
            loadedChunks[cord                     ].GetComponent<ChunkController>().StichChunkRoughMeshes(ChunkController.Edges.down);
            loadedChunks[cord + Vector3Int.back   ].GetComponent<ChunkController>().StichChunkRoughMeshes(ChunkController.Edges.up);
        }
        if (loadedChunks.ContainsKey(cord + Vector3Int.left))
        {
            loadedChunks[cord                     ].GetComponent<ChunkController>().StichChunkRoughMeshes(ChunkController.Edges.left);
            loadedChunks[cord + Vector3Int.left   ].GetComponent<ChunkController>().StichChunkRoughMeshes(ChunkController.Edges.right);
        }
        if (loadedChunks.ContainsKey(cord + Vector3Int.right))
        {
            loadedChunks[cord                     ].GetComponent<ChunkController>().StichChunkRoughMeshes(ChunkController.Edges.right);
            loadedChunks[cord + Vector3Int.right  ].GetComponent<ChunkController>().StichChunkRoughMeshes(ChunkController.Edges.left);
        }
    }

    public Dictionary<Vector3Int,string> getChunkChanges(Vector3Int position)
    {
        if (changes.ContainsKey(position))
            return changes[position];
        return new Dictionary<Vector3Int, string>();
    }
    


    public void NeigborUpdate(Vector3Int position)
    {
        Vector3Int blockChunkCord = new Vector3Int(Mathf.FloorToInt((float)position.x / chunkSize), 0, Mathf.FloorToInt((float)position.z / chunkSize));

        if (loadedChunks[blockChunkCord].GetComponent<ChunkController>().IsAnyBlockPalceExist(position))
        {
            if (!loadedChunks[blockChunkCord].GetComponent<ChunkController>().BlockExposedToAir(position)) { loadedChunks[blockChunkCord].GetComponent<ChunkController>().UnloadBlock(position); }
            else
            {
                if (!loadedChunks[blockChunkCord].GetComponent<ChunkController>().IsBlockLoaded(position)) loadedChunks[blockChunkCord].GetComponent<ChunkController>().LoadBlock(position);
                //loadedChunks[blockChunkCord].GetComponent<ChunkController>().RoughReMesh(position);
            }
        }

    }
    public void PlayerAddBlock(Vector3Int position, string name)
    {
        Vector3Int blockChunkCord = new Vector3Int(Mathf.FloorToInt((float)position.x / chunkSize), 0, Mathf.FloorToInt((float)position.z / chunkSize));
        if (loadedChunks.ContainsKey(blockChunkCord))
        {
            loadedChunks[blockChunkCord].GetComponent<ChunkController>().PlayerAddBlock(position, name);
        }
    }

    // ChunkMethods
    
    public void LoadBlock(Vector3Int position)
    {
        Vector3Int blockChunkCord = new Vector3Int(Mathf.FloorToInt((float)position.x / chunkSize), 0, Mathf.FloorToInt((float)position.z / chunkSize));
        if (loadedChunks.ContainsKey(blockChunkCord))
        {
            loadedChunks[blockChunkCord].GetComponent<ChunkController>().LoadBlock(position);
        }
    }
    public void UnloadBlock(Vector3Int position)
    {
        Vector3Int blockChunkCord = new Vector3Int(Mathf.FloorToInt((float)position.x / chunkSize), 0, Mathf.FloorToInt((float)position.z / chunkSize));
        if (loadedChunks.ContainsKey(blockChunkCord))
        {
            loadedChunks[blockChunkCord].GetComponent<ChunkController>().UnloadBlock(position);
        }
        
    }
    public void RoughReMesh(Vector3Int position)
    {
        Vector3Int blockChunkCord = new Vector3Int(Mathf.FloorToInt((float)position.x / chunkSize), 0, Mathf.FloorToInt((float)position.z / chunkSize));
        if (loadedChunks.ContainsKey(blockChunkCord))
        {
            loadedChunks[blockChunkCord].GetComponent<ChunkController>().RoughReMesh(position);
        }
    }
    public void DestroyBlock(Vector3Int position)
    {
        Vector3Int blockChunkCord = new Vector3Int(Mathf.FloorToInt((float)position.x / chunkSize), 0, Mathf.FloorToInt((float)position.z / chunkSize));
        if (loadedChunks.ContainsKey(blockChunkCord))
        {
            loadedChunks[blockChunkCord].GetComponent<ChunkController>().DestroyBlock(position);
        }
    }
    public void GiveBlockDamage(Vector3Int position, int damage)
    {
        Vector3Int blockChunkCord = new Vector3Int(Mathf.FloorToInt((float)position.x / chunkSize), 0, Mathf.FloorToInt((float)position.z / chunkSize));
        if (loadedChunks.ContainsKey(blockChunkCord))
        {
            loadedChunks[blockChunkCord].GetComponent<ChunkController>().GiveBlockDamage(position, damage);
        }
    }
    public bool IsBlockPalceExist(Vector3Int position)
    {
        Vector3Int blockChunkCord = new Vector3Int(Mathf.FloorToInt((float)position.x / chunkSize), 0, Mathf.FloorToInt((float)position.z / chunkSize));
        if (!loadedChunks.ContainsKey(blockChunkCord)) { return true; }
        return loadedChunks[blockChunkCord].GetComponent<ChunkController>().IsBlockPalceExist(position);
    }
    public bool IsInteractiveBlockExist(Vector3Int position)
    {
        Vector3Int blockChunkCord = new Vector3Int(Mathf.FloorToInt((float)position.x / chunkSize), 0, Mathf.FloorToInt((float)position.z / chunkSize));
        if (loadedChunks.ContainsKey(blockChunkCord)) { return false; }
        return loadedChunks[blockChunkCord].GetComponent<ChunkController>().IsInteractiveBlockExist(position);
    }
    public bool IsAnyBlockPalceExist(Vector3Int position)
    {
        Vector3Int blockChunkCord = new Vector3Int(Mathf.FloorToInt((float)position.x / chunkSize), 0, Mathf.FloorToInt((float)position.z / chunkSize));
        if (!loadedChunks.ContainsKey(blockChunkCord)) { return false; }
        return loadedChunks[blockChunkCord].GetComponent<ChunkController>().IsAnyBlockPalceExist(position);
    }
    
    public bool IsChanged(Vector3Int blocPosition, Vector3Int chunkPosition)
    {
        if (changes.ContainsKey(chunkPosition))
            if (changes[chunkPosition].ContainsKey(blocPosition)) return true;
        return false;
    }
    public string getChangedBlockString(Vector3Int blockPosition, Vector3Int chunkPosition)
    {
        if(changes.ContainsKey(chunkPosition))
            if(changes[chunkPosition].ContainsKey(blockPosition))
                return changes[chunkPosition][blockPosition];
        return "";
    }
    public void PlayerSaveAddBlock(Vector3Int blockPosition, Vector3Int chunkPosition, string blockPath)
    {
        
        if (changes.ContainsKey(chunkPosition))
        {
            if (changes[chunkPosition].ContainsKey(blockPosition))
            {
                changes[chunkPosition][blockPosition] = blockPath;
            }
            else
            {
                changes[chunkPosition].Add(blockPosition, blockPath);
            }
        }
    }
}
