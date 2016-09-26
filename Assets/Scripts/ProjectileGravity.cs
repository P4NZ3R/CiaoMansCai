using UnityEngine;
using System.Collections;
using System.Net.Mail;

//[RequireComponent(typeof(Rigidbody2D))]
public class ProjectileGravity : MonoBehaviour
{
    Rigidbody2D rigidbody;
    Collider2D collider;
    const float gravityMultiplier = 10f;

    void Start()
    {
        GameGenerator.CurrentProjectiles++;
        rigidbody = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        if (!rigidbody)
        {
            Debug.LogError("no rigidbody attached!");
            enabled = false;
            return;
        }
    }

    void FixedUpdate()
    {
        Vector3 ris = Vector3.zero;
        for (int i = 0; i < Universe.map.Length; i++)
        {
            Vector3 dir = Vector3.zero;
            float force = 0;
            if (!Universe.PlanetExists(Universe.map, i))
                continue;
            force = Universe.map[i].mass / Mathf.Pow(Vector3.Distance(Universe.map[i].pos, transform.position), 2);
            dir = Universe.map[i].pos - transform.position;
            ris += dir * force * gravityMultiplier;
        }
        rigidbody.AddForce(ris);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            collision.gameObject.GetComponent<Player>().HitPlayer();
        }
        if (collision.transform.tag == "Planet")
        {
            Destroy(rigidbody);
            Destroy(collider);
            rigidbody = null;

            if (GameGenerator.GrenadeLauncherBullet.gameObject.name + "(Clone)" == gameObject.name)
            {
                Collider2D[] coll = Physics2D.OverlapCircleAll(transform.position, 4f);
                for (int i = 0; i < coll.Length; i++)
                {
                    Debug.Log(coll[i].gameObject.name + "," + Vector2.Distance(coll[i].transform.position, transform.position));
                    if (coll[i].tag == "Player")
                    {
                        coll[i].gameObject.GetComponent<Player>().HitPlayer();
                    }
                }
            }

            GameGenerator.CurrentProjectiles--;
            enabled = false;
        }
    }

    void OnDestroy()
    {
        if (GameGenerator.followedObject == gameObject)
            GameGenerator.followedObject = null;
    }
}
