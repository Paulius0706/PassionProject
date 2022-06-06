using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobBody : MonoBehaviour
{
    public int health;
    public int maxhealth;
    public Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        health = maxhealth;
    }
    
    public void doDamage(int damage, Vector3 damagePosition, float knockBackPower)
    {
        health -= damage;
        rb.AddForce((transform.position - damagePosition).normalized * knockBackPower,ForceMode.VelocityChange);

        if(health <= 0)
        {
            Destroy(transform.parent.gameObject);
        }

    }

}
