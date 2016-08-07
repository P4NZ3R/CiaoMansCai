using UnityEngine;
using System.Collections;
using System.Net.Mail;

[RequireComponent(typeof(Rigidbody2D))]
public class ProjectileGravity : MonoBehaviour
{
    Rigidbody2D rigidbody;
    float gravityMultiplier = 10f;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
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
}
