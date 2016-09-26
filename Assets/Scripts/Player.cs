﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public GameGenerator gameGenerator;
    public GameObject planet;
    SpriteRenderer isActiveSign;
    public int teamId;
    public int playerId;
    public bool isAlive = true;
    float anglePos = 0;
    float speedMovement = 3f;
    float basicBulletForce = 1000f;
    float shotgunBulletForce = 800f;
    float grenadeLauncherBulletForce = 1100f;
    [HideInInspector]public bool canShoot = true;

    delegate void ShootSomething();

    ShootSomething shootSomething;
    // Use this for initialization
    void Start()
    {
        isActiveSign = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
//        isActiveSign.gameObject.GetComponent<Animation>().;
        RefreshPosition(Vector2.Angle(transform.position - planet.transform.position, Vector3.right));
        planet.GetComponent<SpriteRenderer>().color = GameGenerator.GetTeam(teamId).color;
        shootSomething = ShootBasic;
    }

    void Update()
    {
        if (!isAlive || gameGenerator.activeTeam != teamId || GameGenerator.GetTeam(teamId).ActivePlayer != playerId || !canShoot)
        {
            isActiveSign.enabled = false;
            return;
        }
        isActiveSign.enabled = true;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            RefreshPosition(anglePos + Time.deltaTime * speedMovement / planet.transform.localScale.x);
        }
        else
        {
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                RefreshPosition(anglePos - Time.deltaTime * speedMovement / planet.transform.localScale.x);
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            shootSomething = ShootBasic;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            shootSomething = ShootShotgun;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            shootSomething = ShootGrenadeLauncher;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            shootSomething();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isAlive)
            return;
        //ruota il player secondo la posizione sul pianeta
        Vector3 vectorToTarget = planet.transform.position - transform.position;
        float anglePos = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg + 90f;
        Quaternion q = Quaternion.AngleAxis(anglePos, Vector3.forward);
        transform.rotation = q;
    }

    void RefreshPosition(float angle)
    {
        if (angle == anglePos)
            return;
        if (anglePos > 360)
            anglePos -= 360;
        Vector3 newNormal = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
        transform.position = planet.transform.position + (planet.transform.localScale.x / 2f + transform.localScale.y / 2f) * newNormal;
        anglePos = angle;
    }

    public void HitPlayer()
    {
        GameGenerator.GetTeam(teamId).players[playerId] = null;
        Destroy(gameObject);
    }

    public void ShootBasic()
    {
        canShoot = false;
        GameObject clone = Instantiate(GameGenerator.BasicBullet, transform.TransformPoint(Vector3.up * (transform.localScale.y / 1.5f)), Quaternion.identity) as GameObject;
        GameGenerator.followedObject = clone.transform;
        clone.GetComponent<Rigidbody2D>().AddForce(transform.up * basicBulletForce);
    }

    public void ShootShotgun()
    {
        canShoot = false;
        const float spreadFactor = 0.3f;
        const int numProjectile = 15;
        for (int i = 0; i < numProjectile; i++)
        {
            GameObject clone = Instantiate(GameGenerator.ShotgunBullet, transform.TransformPoint(Vector3.up * (transform.localScale.y / 1.5f)), Quaternion.identity) as GameObject;
            GameGenerator.followedObject = clone.transform;
            Vector2 direction = new Vector2(Rng.GetNumber(-spreadFactor, spreadFactor) + transform.up.x, Rng.GetNumber(-spreadFactor, spreadFactor) + transform.up.y);
            clone.GetComponent<Rigidbody2D>().AddForce(direction * shotgunBulletForce);
        }
    }

    public void ShootGrenadeLauncher()
    {
        canShoot = false;
        const float spreadFactor = 0.05f;
        GameObject clone = Instantiate(GameGenerator.GrenadeLauncherBullet, transform.TransformPoint(Vector3.up * (transform.localScale.y / 1.2f)), Quaternion.identity) as GameObject;
        GameGenerator.followedObject = clone.transform;
        Vector2 direction = new Vector2(Rng.GetNumber(-spreadFactor, spreadFactor) + transform.up.x, Rng.GetNumber(-spreadFactor, spreadFactor) + transform.up.y);
        clone.GetComponent<Rigidbody2D>().AddForce(direction * grenadeLauncherBulletForce);
    }
}
