using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordController : MonoBehaviour
{
    

    public ChunkGenerator chunkGenerator;
    public int damage;
    public int terrainDamage;
    public int knockBackPower;

    public float attackSpeed;
    public float attackPreperationSpeed;
    public float attackRetrackSpeed;

    private bool destroyTerrain;
    private ItemHolder itemHolder;
    private Collider hurtBox;

    // Start is called before the first frame update
    void Start()
    {
        hurtBox = GetComponent<Collider>();
        SetAttackSpeed();
        itemHolder = transform.parent.gameObject.GetComponent<ItemHolder>();
        chunkGenerator = itemHolder.chunkGenerator;
        destroyTerrain = true;
        hurtBox.enabled = false;

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0)){
            itemHolder.itemIsActive = true;
            itemHolder.canBeChanged = false;
        }
        else {
            itemHolder.itemIsActive = true;
            itemHolder.canBeChanged = true;
        }
    }
    public void EnableHurtBox()
    {
        hurtBox.enabled = true;
        destroyTerrain = true;
        itemHolder.playerMovement.Pulse();
    }
    public void DisableHurtBox()
    {
        hurtBox.enabled = false;
        destroyTerrain = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if((other.gameObject.tag == "Block")
            && destroyTerrain)
        {
            chunkGenerator.GiveBlockDamage(Vector3Int.RoundToInt(other.transform.position), terrainDamage);
            // turn false if we want to swing destroy 1 block at the time
            destroyTerrain = true;
        }
        if ((other.gameObject.tag == "MobBody"))
        {
            other.gameObject.GetComponent<MobBody>().doDamage(damage,transform.position + Vector3.down * 0.5f, knockBackPower);
        }
    }

    public void Activate(bool boolas)
    {
        if (boolas)
        {
            itemHolder.itemIsActive = true;
            itemHolder.canBeChanged = false;
            itemHolder.itemActivationMoveStop = true;
        }
        else
        {
            itemHolder.itemIsActive = false;
            itemHolder.canBeChanged = true;
            itemHolder.itemActivationMoveStop = false;
        }
    }
    public void SetAttackSpeed()
    {

    }
}
