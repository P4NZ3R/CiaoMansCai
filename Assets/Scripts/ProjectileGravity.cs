using UnityEngine;
using System.Collections;
using System.Net.Mail;

//[RequireComponent(typeof(Rigidbody2D))]
public class ProjectileGravity : MonoBehaviour
{
    Rigidbody2D rigidbody;
    Collider2D collider;
    const float gravityMultiplier = 10f;

    void Awake()
    {
        GameGenerator.CurrentProjectiles++;
    }

    void Start()
    {
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
                ShootDebris(collision.relativeVelocity);
                Destroy(gameObject);
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

    public void ShootDebris(Vector2 directionImpact)
    {
        const float spreadFactor = 50f;
        const float debrisForce = 500f;
        const int numProjectile = 8;
        for (int i = 0; i < numProjectile; i++)
        {
            GameObject clone = Instantiate(GameGenerator.ShotgunBullet, transform.position, Quaternion.identity) as GameObject;
            GameGenerator.followedObject = clone.transform;
//            Vector2 direction = new Vector2(Rng.GetNumber(-spreadFactor, spreadFactor), Rng.GetNumber(-spreadFactor, spreadFactor));
            Vector2 direction = Rng.ApplyInaccuracy(directionImpact.normalized, spreadFactor);
            clone.GetComponent<Rigidbody2D>().AddForce(direction * debrisForce);
        }
    }
}
