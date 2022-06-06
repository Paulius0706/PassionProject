using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobSpawner : MonoBehaviour
{
    public ChunkGenerator chunkGenerator;
    public int maxMobs;

    public float spawnMobInterval;
    private float spawnMobCounter;

    public float despawnRadius;
    public float spawnRadius;

    private List<GameObject> mobs;

    enum MobType
    {
        zombie = 0
    }

    // Start is called before the first frame update
    void Start()
    {
        spawnMobCounter = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        SpawnMob();
        DespawnMob();
    }
    private void SpawnMob() {
        spawnMobCounter += Time.deltaTime;
        if (spawnMobCounter > spawnMobInterval && maxMobs > mobs.Count)
        {
            Vector3 position = new Vector3(Random.value, 0, Random.value).normalized;
            Vector3Int position1 = Vector3Int.RoundToInt(position * spawnRadius + position * (despawnRadius - spawnRadius));
            int y = -1;
            for (int i = 0; i <= chunkGenerator.noise.max_elevation; i++)
            {
                if (!chunkGenerator.IsAnyBlockPalceExist(position1 + Vector3Int.up * i))
                {
                    position1.y = i;
                    y = i;
                    break;
                }
            }
            if (y > 0)
            {
                GameObject prefab = Resources.Load<GameObject>("Entities/Mob/Zombie");
                GameObject mob = Instantiate(prefab, position1, Quaternion.identity);
                mob.transform.parent = gameObject.transform;
                mobs.Add(mob);
            }
        }
    }
    private void DespawnMob()
    {
        for (int i = 0; i <= mobs.Count; i++)
        {
            //AHHHHH negaliu
        }
    }
}
