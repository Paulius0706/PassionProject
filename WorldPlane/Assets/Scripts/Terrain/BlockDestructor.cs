using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// rename to simple block destructor
public class BlockDestructor : MonoBehaviour
{
    public int hitPoints;
    public bool restoreToNonInteractive;
    public float maxHitIntervalTime;
    public float lastHitTime;


    public int maxHitPoints;

    private void Awake()
    {
        hitPoints = maxHitPoints;
        lastHitTime = 0;
    }
    private void Update()
    {
        lastHitTime += Time.deltaTime;
        if(lastHitTime > maxHitIntervalTime)
        {
            if (restoreToNonInteractive)
            {
                transform.parent.gameObject.GetComponent<ChunkController>().RestoreBlockToNoninteractive(Vector3Int.RoundToInt(transform.position), gameObject.name.Split('(')[0]);
                
            }
            else
            {
                hitPoints = maxHitPoints;
                transform.localScale = Vector3.one;
            }
        }
    }


    public void doDamage(int damage)
    {
        lastHitTime = 0;
        hitPoints -= damage;
        Color color = GetComponent<MeshRenderer>().material.color;
        color.a = 0.2f + 0.8f * ((float)hitPoints / (float)maxHitPoints);
        GetComponent<MeshRenderer>().material.color = color;
        if(hitPoints <= 0)
        {
            transform.parent.gameObject.GetComponent<ChunkController>().DestroyInteractiveBlock(Vector3Int.RoundToInt(transform.position));
        }
    }
}
