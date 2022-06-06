using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ChunkController : MonoBehaviour
{
    public Vector3Int position;
    private ChunkGenerator chunkGenerator;
    public Dictionary<Vector3Int, GameObject> terrainObjects;
    public Dictionary<Vector3Int, GameObject> interactiveObjects;
    public Dictionary<Vector3Int, float> elevations;
    private List<Vector3Int>[] edgeBlocks;
    
    private List<Vector3Int> updateLoadList;
    private List<Vector3Int> updateMeshList;

    private int[] xLim;
    private int[] zLim;
    public Mesh roughMeshPrefab;
    
    public enum ObjectType
    {
        terrain = 1,
        interactive = 2
    }
    public enum Edges
    {
        up = 0,
        down = 1,
        right = 2,
        left = 3
    }
    
    public void ChunkSet(Vector3Int position, ChunkGenerator chunkGenerator)
    {
        this.position = position;
        this.chunkGenerator = chunkGenerator;
        this.terrainObjects = new Dictionary<Vector3Int, GameObject>();
        this.elevations = new Dictionary<Vector3Int, float>();
        this.interactiveObjects = new Dictionary<Vector3Int, GameObject>();
        this.updateLoadList = new List<Vector3Int>();
        this.updateMeshList = new List<Vector3Int>();
        xLim = new int[2];
        xLim[0] = this.position.x * chunkGenerator.chunkSize;
        xLim[1] = this.position.x * chunkGenerator.chunkSize + chunkGenerator.chunkSize - 1;
        zLim = new int[2];
        zLim[0] = this.position.z * chunkGenerator.chunkSize;
        zLim[1] = this.position.z * chunkGenerator.chunkSize + chunkGenerator.chunkSize - 1;
        edgeBlocks = new List<Vector3Int>[4];
        for (int i = 0; i < 4; i++) edgeBlocks[i] = new List<Vector3Int>();
    }

    public void Generate()
    {
        generateElevation();
    }

    /// <summary>
    /// Generate register blocks, gets elevation and loads blocks exposed to air
    /// </summary>
    private void generateElevation()
    {
        // sets objects keys that are destined to not be air
        for (int x = 0; x < chunkGenerator.chunkSize; x++)
        {
            for (int z = 0; z < chunkGenerator.chunkSize; z++)
            {
                Vector3Int worldPosition = localPosToWorldPos(new Vector3Int(x, 0, z));
                float elevation = chunkGenerator.noise.ElevationHeight(worldPosition.x, worldPosition.z);
                elevations.Add(worldPosition, elevation);
                worldPosition.y = Mathf.RoundToInt(elevation);
                while (worldPosition.y > -1)
                {

                    if (xLim[0] == worldPosition.x) edgeBlocks[(int)Edges.left].Add(worldPosition);
                    if (xLim[1] == worldPosition.x) edgeBlocks[(int)Edges.right].Add(worldPosition);
                    if (zLim[0] == worldPosition.z) edgeBlocks[(int)Edges.down].Add(worldPosition);
                    if (zLim[1] == worldPosition.z) edgeBlocks[(int)Edges.up].Add(worldPosition);
                    terrainObjects.Add(worldPosition, null);
                    worldPosition.y--;
                }
            }
        }
        // Do add remove changed blocks
        int i = 0;
        Dictionary<Vector3Int, string> changes = chunkGenerator.getChunkChanges(this.position);
        Vector3Int[] keys = new Vector3Int[changes.Count];
        foreach (Vector3Int key in changes.Keys) keys[i++] = key;
        foreach (Vector3Int key in keys)
        {
            if (changes[key] == "" && terrainObjects.ContainsKey(key)) terrainObjects.Remove(key);
            else if(!terrainObjects.ContainsKey(key))
            {
                terrainObjects.Add(key, null);
            }
        }
        //

        // adds objects that are exposed to air
        i = 0;
        keys = new Vector3Int[terrainObjects.Count];
        foreach (Vector3Int key in terrainObjects.Keys) keys[i++] = key;
        foreach (Vector3Int key in keys) {
            if (BlockExposedToAir(key))
            {
                LoadBlock(key);
                RoughReMesh(key);
            }
        }
    }

    

    /// <summary>
    /// find if position neigbors are exposed to no blocks(register block included)
    /// (need rework becouse interactive, nonfull, transparent blocks are included to blocade)
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool BlockExposedToAir(Vector3Int position)
    {
        if (!IsBlockPalceExist(position + Vector3Int.up)) { return true; }
        if (!IsBlockPalceExist(position + Vector3Int.down) && position.y > 0) { return true; }
        if (!IsBlockPalceExist(position + Vector3Int.left)) { return true; }
        if (!IsBlockPalceExist(position + Vector3Int.right)) { return true; }
        if (!IsBlockPalceExist(position + Vector3Int.forward)) { return true; }
        if (!IsBlockPalceExist(position + Vector3Int.back)) { return true; }
        return false;
    }
    



    /// <summary>
    /// (Useless)Updates this and neighbor blocks states
    /// </summary>
    /// <param name="position"></param>
    private void TargetUpdate(Vector3Int position)
    {
        if (IsAnyBlockPalceExist(position))
        {
            if (!BlockExposedToAir(position)) { UnloadBlock(position); }
            else
            {
                if (!IsBlockLoaded(position) && !IsInteractiveBlockExist(position)) LoadBlock(position);
                //RoughReMesh(position);
            }
        }
        chunkGenerator.NeigborUpdate(position + Vector3Int.up);
        chunkGenerator.NeigborUpdate(position + Vector3Int.down);
        chunkGenerator.NeigborUpdate(position + Vector3Int.left);
        chunkGenerator.NeigborUpdate(position + Vector3Int.right);
        chunkGenerator.NeigborUpdate(position + Vector3Int.forward);
        chunkGenerator.NeigborUpdate(position + Vector3Int.back);
    }

    void LateUpdate()
    {
        while(updateLoadList.Count > 0)
        {
            Vector3Int vector = updateLoadList[0];
            updateLoadList.RemoveAt(0);
            LoadUpdate(vector);
        }

        while (updateMeshList.Count > 0)
        {
            Vector3Int vector = updateMeshList[0];
            updateMeshList.RemoveAt(0);
            MeshUpdate(vector);
        }
    }


    public void LoadUpdate(Vector3Int position)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    Vector3Int temp = position + Vector3Int.right * x + Vector3Int.forward * z + Vector3Int.up * y;
                    if (IsBlockPalceExist(temp))
                    {
                        if      (!BlockExposedToAir(temp)) { UnloadBlock(temp); }
                        else if (!IsBlockLoaded(temp))     { LoadBlock(temp); }
                    }
                }   
            }
        }
    }
    /// <summary>
    /// Updates position and border block meshes
    /// (fix chunk border is not correct)
    /// </summary>
    /// <param name="position"></param>
    public void MeshUpdate(Vector3Int position)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    Vector3Int temp = position + Vector3Int.right * x + Vector3Int.forward * z + Vector3Int.up * y;
                    RoughReMesh(temp);
                }
            }
        }
    }

    /// <summary>
    /// Remesh position block and neigbor blocks
    /// </summary>
    /// <param name="position"></param>
    private void ExtensiveRoughReMesh(Vector3Int position)
    {
        RoughReMesh(position);
        if (IsInChunk(position + Vector3Int.forward)) { chunkGenerator.RoughReMesh(position + Vector3Int.forward); }
        else { RoughReMesh(position + Vector3Int.forward); }
        if (IsInChunk(position + Vector3Int.back)) { chunkGenerator.RoughReMesh(position + Vector3Int.back); }
        else { RoughReMesh(position + Vector3Int.back); }
        if (IsInChunk(position + Vector3Int.left)) { chunkGenerator.RoughReMesh(position + Vector3Int.left); }
        else { RoughReMesh(position + Vector3Int.left); }
        if (IsInChunk(position + Vector3Int.right)) { chunkGenerator.RoughReMesh(position + Vector3Int.right); }
        else { RoughReMesh(position + Vector3Int.right); }
        RoughReMesh(position + Vector3Int.up);
        RoughReMesh(position + Vector3Int.down);
    }

    /// <summary>
    /// remesh specified edges
    /// </summary>
    /// <param name="edge"></param>
    public void StichChunkRoughMeshes(Edges edge)
    {
        foreach (Vector3Int position in edgeBlocks[(int)edge])
        {
            RoughReMesh(position);
        }

    }


    /// <summary>
    /// Modify mesh to smother state (needs critical fix)
    /// </summary>
    /// <param name="position"></param>
    public void RoughReMesh(Vector3Int position)
    {
        // Rough Cube
        //30 50 62    48 63   44 66   5 46 67
        //28 57                       7 65
        //32 54                       9 60
        //34 38 53    36 52   40 56   11 42 59

        //Rough Cube1 vright ^left >foward <back
        //18 29 59 ,19 58 ,21 55 ,8 22 54
        //28 57    ,      ,      ,7 48
        //31 52    ,      ,      ,5 47
        //32 42 51 ,41 50 ,39 44 ,4 38 46

        float chipHeight = 0.3125f;
        Vector3[] vertices = roughMeshPrefab.vertices;

        if (!IsBlockPalceExist(position + Vector3Int.up) && IsBlockLoaded(position))
        {

            // Rough Cube1 vright ^left >foward <back
            //18 29 59 ,19 58 ,21 55 ,8 22 54
            //28 57    ,      ,      ,7 48
            //31 52    ,      ,      ,5 47
            //32 42 51 ,41 50 ,39 44 ,4 38 46
            if (!IsBlockPalceExist(position + Vector3Int.right) && !IsBlockPalceExist(position + Vector3Int.right + Vector3Int.up))
            {
                vertices[32].y = chipHeight;
                vertices[42].y = chipHeight;
                vertices[51].y = chipHeight;

                vertices[41].y = chipHeight;
                vertices[50].y = chipHeight;

                vertices[39].y = chipHeight;
                vertices[44].y = chipHeight;

                vertices[4].y = chipHeight;
                vertices[38].y = chipHeight;
                vertices[46].y = chipHeight;
            }
            if (!IsBlockPalceExist(position + Vector3Int.left)    && !IsBlockPalceExist(position + Vector3Int.left + Vector3Int.up))
            {
                vertices[18].y = chipHeight;
                vertices[29].y = chipHeight;
                vertices[59].y = chipHeight;

                vertices[19].y = chipHeight;
                vertices[58].y = chipHeight;

                vertices[21].y = chipHeight;
                vertices[55].y = chipHeight;

                vertices[8].y = chipHeight;
                vertices[22].y = chipHeight;
                vertices[54].y = chipHeight;
            }
            if (!IsBlockPalceExist(position + Vector3Int.forward) && !IsBlockPalceExist(position + Vector3Int.forward + Vector3Int.up))
            {
                vertices[8].y = chipHeight;
                vertices[22].y = chipHeight;
                vertices[54].y = chipHeight;

                vertices[7].y = chipHeight;
                vertices[48].y = chipHeight;

                vertices[5].y = chipHeight;
                vertices[47].y = chipHeight;

                vertices[4].y = chipHeight;
                vertices[38].y = chipHeight;
                vertices[46].y = chipHeight;
            }
            if (!IsBlockPalceExist(position + Vector3Int.back)    && !IsBlockPalceExist(position + Vector3Int.back + Vector3Int.up))
            {
                vertices[18].y = 0.3125f;
                vertices[29].y = 0.3125f;
                vertices[59].y = 0.3125f;

                vertices[28].y = 0.3125f;
                vertices[57].y = 0.3125f;

                vertices[31].y = 0.3125f;
                vertices[52].y = 0.3125f;

                vertices[32].y = 0.3125f;
                vertices[42].y = 0.3125f;
                vertices[51].y = 0.3125f;
            }

            if (!IsBlockPalceExist(position + Vector3Int.back + Vector3Int.left)     && !IsBlockPalceExist(position + Vector3Int.back + Vector3Int.up + Vector3Int.left))
            {
                vertices[18].y = chipHeight;
                vertices[29].y = chipHeight;
                vertices[59].y = chipHeight;
            }
            if (!IsBlockPalceExist(position + Vector3Int.back + Vector3Int.right)    && !IsBlockPalceExist(position + Vector3Int.back + Vector3Int.up + Vector3Int.right))
            {
                vertices[32].y = chipHeight;
                vertices[42].y = chipHeight;
                vertices[51].y = chipHeight;
            }
            if (!IsBlockPalceExist(position + Vector3Int.forward + Vector3Int.left)  && !IsBlockPalceExist(position + Vector3Int.forward + Vector3Int.up + Vector3Int.left))
            {
                vertices[8].y = chipHeight;
                vertices[22].y = chipHeight;
                vertices[54].y = chipHeight;
            }
            if (!IsBlockPalceExist(position + Vector3Int.forward + Vector3Int.right) && !IsBlockPalceExist(position + Vector3Int.forward + Vector3Int.up + Vector3Int.right))
            {
                vertices[4].y = chipHeight;
                vertices[38].y = chipHeight;
                vertices[46].y = chipHeight;
            }


            
        }
        if (IsBlockLoaded(position))
        {
            terrainObjects[position].GetComponent<MeshFilter>().mesh.SetVertices(vertices);
            terrainObjects[position].GetComponent<MeshFilter>().mesh.RecalculateNormals();
        }
    }

    /// <summary>
    /// Converts bloc local chunk cordinates to world ones
    /// </summary>
    /// <param name="position">Local cordinates</param>
    /// <returns>World cordinates</returns>
    private Vector3Int localPosToWorldPos(Vector3Int position)
    {
        return new Vector3Int(this.position.x * chunkGenerator.chunkSize + position.x,
            position.y,
            this.position.z * chunkGenerator.chunkSize + position.z
            );
    }
    
    
    
    /// <summary>
    /// finds if there are registered or loaded terrain block in position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool IsBlockPalceExist(Vector3Int position)
    {
        if (!IsInChunk(position)) return chunkGenerator.IsBlockPalceExist(position);
        return terrainObjects.ContainsKey(position);
    }
    /// <summary>
    /// finds if there are registered or loaded terrain or interactive block in position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool IsAnyBlockPalceExist(Vector3Int position)
    {
        if (!IsInChunk(position)) return chunkGenerator.IsAnyBlockPalceExist(position);
        return terrainObjects.ContainsKey(position) || interactiveObjects.ContainsKey(position);
    }
    /// <summary>
    /// finds if block is in register state
    /// </summary>
    /// <param name="position"></param>
    /// <returns>is block in register state</returns>
    public bool IsBlockRegister(Vector3Int position)
    {
        return terrainObjects.ContainsKey(position) && terrainObjects[position] == null;
    }
    /// <summary>
    /// finds if block is in loaded state
    /// </summary>
    /// <param name="position"></param>
    /// <returns>is block in loaded state</returns>
    public bool IsBlockLoaded(Vector3Int position)
    {
        return terrainObjects.ContainsKey(position) && terrainObjects[position] != null;
    }
    /// <summary>
    /// finds if interactive block exist in specific position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool IsInteractiveBlockExist(Vector3Int position)
    {
        return interactiveObjects.ContainsKey(position);
    }
    /// <summary>
    /// Checks if position is in chunk
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool IsInChunk(Vector3Int position)
    {
        return !(position.x < xLim[0]
              || position.z < zLim[0]
              || position.x > xLim[1]
              || position.z > zLim[1]);
    }

    /// <summary>
    /// finds if position in chunk boundaries
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool IsOnChunkBorder(Vector3Int position)
    {
        return (position.x == xLim[0]
            || position.z == zLim[0]
            || position.x == xLim[1]
            || position.z == zLim[1]);
    }
    
    /// <summary>
    /// Gives blockDamage or sets block to interactive and gives damage
    /// </summary>
    /// <param name="position"></param>
    /// <param name="damage"></param>
    public void GiveBlockDamage(Vector3Int position, int damage)
    {
        if (terrainObjects.ContainsKey(position))
        {
            string name = terrainObjects[position].name.Split('(')[0];
            DestroyBlock(position);
            SetInteractiveBlock("InteractiveTerrain/" + name, position);
            if (!updateLoadList.Contains(position)) updateLoadList.Add(position);
            if (!updateMeshList.Contains(position)) updateMeshList.Add(position);
            //if (!addInteractiveQueue.ContainsKey(position)) addInteractiveQueue.Add(position, "InteractiveTerrain/" + name);
        }
        if(interactiveObjects.ContainsKey(position))
        {
            interactiveObjects[position].GetComponent<BlockDestructor>().doDamage(damage);
        }
    }

    /// <summary>
    /// Restores block from interactive to inactive state
    /// Saves computer resources
    /// </summary>
    /// <param name="position"></param>
    public void RestoreBlockToNoninteractive(Vector3Int position, string name)
    {
        if (IsInteractiveBlockExist(position))
        {
            DestroyInteractiveBlock(position);
            SetBlock("Terrain/" + name, position);
            if (!updateLoadList.Contains(position)) updateLoadList.Add(position);
            if (!updateMeshList.Contains(position)) updateMeshList.Add(position);
            //if (!addTerrainQueue.ContainsKey(position)) addTerrainQueue.Add(position, "Terrain/" + name);
        }
    }

    // ADDs AND REMOVES OF BLOCKS
    //
    /// <summary>
    /// Adds block without update
    /// </summary>
    /// <param name="prefab">block type</param>
    /// <param name="position">block position</param>
    private void SetBlock(GameObject prefab, Vector3Int position)
    {
        GameObject block = Instantiate(prefab, position + Vector3.up * prefab.transform.position.y, Quaternion.identity);
        block.transform.parent = gameObject.transform;
        if (terrainObjects.ContainsKey(position)) terrainObjects[position] = block;
        else terrainObjects.Add(position, block);
    }
    /// <summary>
    /// Adds block without update
    /// </summary>
    /// <param name="path"></param>
    /// <param name="position"></param>
    private void SetBlock(string path, Vector3Int position)
    {
        GameObject prefab = Resources.Load<GameObject>(path);
        GameObject block = Instantiate(prefab, position + Vector3.up * prefab.transform.position.y, Quaternion.identity);
        block.transform.parent = gameObject.transform;
        if (terrainObjects.ContainsKey(position)) terrainObjects[position] = block;
        else terrainObjects.Add(position, block);
    }
    /// <summary>
    /// executes when player places block
    /// </summary>
    /// <param name="position"></param>
    /// <param name="name"></param>
    public void PlayerAddBlock(Vector3Int position, string name)
    {
        GameObject prefab = Resources.Load<GameObject>("InteractiveTerrain/" + name);
        GameObject block = Instantiate(prefab, position + Vector3.up * prefab.transform.position.y, Quaternion.identity);
        block.GetComponent<BlockDestructor>().lastHitTime = 10000f;
        block.transform.parent = gameObject.transform;
        block.transform.position = position;
        if (!interactiveObjects.ContainsKey(position)) interactiveObjects.Add(position, block);
        chunkGenerator.PlayerSaveAddBlock(position, this.position, "InteractiveTerrain/" + name);
    }
    /// <summary>
    /// Adds interactive block without update
    /// </summary>
    /// <param name="path"></param>
    /// <param name="position"></param>
    private void SetInteractiveBlock(string path, Vector3Int position)
    {
        GameObject prefab = Resources.Load<GameObject>(path);
        GameObject block = Instantiate(prefab, position + Vector3.up * prefab.transform.position.y, Quaternion.identity);
        block.transform.parent = gameObject.transform;
        if (interactiveObjects.ContainsKey(position)) interactiveObjects[position] = block;
        else interactiveObjects.Add(position, block);
    }
    /// <summary>
    /// Adds interactive block without update
    /// </summary>
    /// <param name="path"></param>
    /// <param name="position"></param>
    private void SetInteractiveBlock(GameObject prefab, Vector3Int position)
    {
        GameObject block = Instantiate(prefab, position + Vector3.up * prefab.transform.position.y, Quaternion.identity);
        block.transform.parent = gameObject.transform;
        if (interactiveObjects.ContainsKey(position)) interactiveObjects[position] = block;
        else interactiveObjects.Add(position, block);
    }
    /// <summary>
    /// Adds block Form Registery to GameObject 
    /// (dont trigger update)
    /// </summary>
    /// <param name="position"></param>
    public void LoadBlock(Vector3Int position)
    {
        if (!IsInChunk(position)) { chunkGenerator.LoadBlock(position); }
        else if (chunkGenerator.IsChanged(position,this.position))
        {
            string blockPath = chunkGenerator.getChangedBlockString(position, this.position);
            SetBlock(blockPath, position);
            Debug.Log("Loading Saved block: " + blockPath);
        }
        else if(terrainObjects.ContainsKey(position))
        {
            Vector3Int elevationID = position + Vector3Int.down * position.y;
            NoiseGenerator.Block block = chunkGenerator.noise.SpawnBlock(position, elevations[elevationID]);
            switch (block)
            {
                case NoiseGenerator.Block.Grass:
                    SetBlock("Terrain/Grass", position);
                    break;
                case NoiseGenerator.Block.Rock:
                    SetBlock("Terrain/Rock", position);
                    break;
                case NoiseGenerator.Block.Sand:
                    SetBlock("Terrain/Sand", position);
                    break;
            }
        }
        
    }
    /// <summary>
    /// Completly destroy block (register included)
    /// </summary>
    /// <param name="position"></param>
    public void UnloadBlock(Vector3Int position)
    {
        if (!IsInChunk(position)) chunkGenerator.UnloadBlock(position);
        else if (terrainObjects.ContainsKey(position))
        {
            Destroy(terrainObjects[position]);
            terrainObjects[position] = null;
        }
    }
    /// <summary>
    /// Completly destroy block (register included)
    /// </summary>
    /// <param name="position"></param>
    public void DestroyBlock(Vector3Int position)
    {
        if (!IsInChunk(position)) chunkGenerator.DestroyBlock(position);
        else if (terrainObjects.ContainsKey(position))
        {
            Destroy(terrainObjects[position].gameObject);
            terrainObjects.Remove(position);
        }
    }
    /// <summary>
    /// Completly destroys interactive block (register included)
    /// </summary>
    /// <param name="position"></param>
    public void DestroyInteractiveBlock(Vector3Int position)
    {
        if (interactiveObjects.ContainsKey(position))
        {
            Destroy(interactiveObjects[position]);
            interactiveObjects.Remove(position);
        }
    }
    //
    // ADDs AND REMOVES OF BLOCKS

    /// <summary>
    /// Unloads chunk form memory
    /// </summary>
    public void DestroyChunk()
    {
        int i = 0;
        Vector3Int[] keys = new Vector3Int[terrainObjects.Count];
        foreach (Vector3Int key in terrainObjects.Keys) keys[i++] = key;
        foreach (Vector3Int key in keys)
        {
            Destroy(terrainObjects[key]);
            terrainObjects.Remove(key);
        }
        i = 0;
        keys = new Vector3Int[interactiveObjects.Count];
        foreach (Vector3Int key in interactiveObjects.Keys) keys[i++] = key;
        foreach (Vector3Int key in keys)
        {
            Destroy(interactiveObjects[key]);
            interactiveObjects.Remove(key);
        }
        Destroy(gameObject);
    }


    //////////////MY OLD CODE//////////////////

    ///// <summary>
    ///// (Useless) Old elevation and block generator
    ///// </summary>
    //private void generateElevation1()
    //{
    //    for (int x = 0; x < chunkGenerator.chunkSize; x++)
    //    {
    //        for (int z = 0; z < chunkGenerator.chunkSize; z++)
    //        {
    //            Vector3Int worldPosition = localPosToWorldPos(new Vector3Int(x, 0, z));
    //            float elevation = chunkGenerator.noise.ElevationHeight(worldPosition.x, worldPosition.z);
    //            worldPosition.y = Mathf.RoundToInt(elevation);
    //            while (worldPosition.y >= 0f)
    //            {
    //                NoiseGenerator.Block block = chunkGenerator.noise.SpawnBlock(worldPosition, elevation);
    //                switch (block)
    //                {
    //                    case NoiseGenerator.Block.Grass:
    //                        addBlock("Terrain/Grass", worldPosition);
    //                        break;
    //                    case NoiseGenerator.Block.Rock:
    //                        addBlock("Terrain/Rock", worldPosition);
    //                        break;
    //                }
    //                worldPosition.y -= 1;
    //            }
    //        }
    //    }
    //}
    ///// <summary>
    ///// (Useless) adds block by given value
    ///// </summary>
    ///// <param name="path"></param>
    ///// <param name="position"></param>
    //private void addBlock(string path, Vector3Int position)
    //{
    //    GameObject prefab = Resources.Load<GameObject>(path);
    //    GameObject block = Instantiate(prefab, position + Vector3.up * prefab.transform.position.y, Quaternion.identity);

    //    block.transform.parent = gameObject.transform;
    //    if (terrainObjects.ContainsKey(position)) terrainObjects[position] = block;
    //    else terrainObjects.Add(position, block);
    //}    
    ///// <summary>
    ///// (Useless) finds if there are a solid full and nonTransparent block
    ///// </summary>
    ///// <param name="position">block world position</param>
    ///// <returns></returns>
    //public bool getSolidBlockStatus(Vector3Int position)
    //{
    //    if (terrainObjects.ContainsKey(position))
    //        if (terrainObjects[position].tag != "Unsolid" && terrainObjects[position].tag != "InteractiveBlock") return true;
    //    return false;
    //}
    ///// <summary>
    ///// (Useless) finds if there are block in position
    ///// </summary>
    ///// <param name="position">block world position</param>
    ///// <returns></returns>
    //public bool getBlockStatus(Vector3Int position)
    //{
    //    if (terrainObjects.ContainsKey(position)) return true;
    //    return false;
    //}
    ///// <summary>
    ///// (Uselees)Destroy block but leaves registry
    ///// </summary>
    ///// <param name="position">Block world cordinates</param>    
    //public void UpdateDestroyBlock(Vector3Int position)
    //{
    //    Destroy(terrainObjects[position]);
    //    terrainObjects[position] = null;
    //}
}
