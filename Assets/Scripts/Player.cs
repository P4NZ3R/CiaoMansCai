using UnityEngine;

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
    Vector2 shootDirection;
    //for mobile
    Vector2 inputPosStart;
    Vector2 inputPosEnd;
    Vector2 inputPosActual;
    Vector2 inputPosWorldStart;
    Vector2 inputPosWorldEnd;
    Vector2 inputPosWorldActual;
    [SerializeField]AnimationCurve forceProjectileMobile;
    [HideInInspector]public bool canShoot = true;


    delegate void ShootSomething();

    ShootSomething shootSomething;
    // Use this for initialization
    void Start()
    {
        isActiveSign = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
//        isActiveSign.gameObject.GetComponent<Animation>().;
        RefreshPosition(Vector2.Angle(transform.position - planet.transform.position, Vector3.right));
        planet.GetComponent<SpriteRenderer>().color = Color.white;
        planet.GetComponent<SpriteRenderer>().sprite = GraphicGenerator.GetPlanetSprite(GameGenerator.GetTeam(teamId), planet.transform.lossyScale);
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
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A) || ButtonManager.Instance.requestMovement1)
        {
            RefreshPosition(anglePos + Time.deltaTime * speedMovement / planet.transform.localScale.x);
        }
        else
        {
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) || ButtonManager.Instance.requestMovement2)
            {
                RefreshPosition(anglePos - Time.deltaTime * speedMovement / planet.transform.localScale.x);
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ButtonManager.Instance.CurrentWeapon = 0;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ButtonManager.Instance.CurrentWeapon = 1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ButtonManager.Instance.CurrentWeapon = 2;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            shootSomething();
        }
        switch (ButtonManager.Instance.CurrentWeapon)
        {
            case 0:
                shootSomething = ShootBasic;
                break;
            case 1:
                shootSomething = ShootShotgun;
                break;
            case 2:
                shootSomething = ShootGrenadeLauncher;
                break;
        }
        //phone input
        if (Input.touchCount == 1 && !ButtonManager.Instance.requestMovement1 && !ButtonManager.Instance.requestMovement2)
        {
//            inputPosActual = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
            inputPosActual = Input.GetTouch(0).position;
            inputPosWorldActual = Camera.main.ScreenToWorldPoint(inputPosActual);
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                inputPosStart = inputPosActual;
                inputPosWorldStart = Camera.main.ScreenToWorldPoint(inputPosStart);
            }
            if (Input.GetTouch(0).phase == TouchPhase.Ended && Vector2.Distance(inputPosStart, inputPosEnd) > 30f)
            {
                inputPosEnd = inputPosActual;
                Debug.LogError(Vector2.Distance(inputPosStart, inputPosEnd));
                inputPosWorldEnd = Camera.main.ScreenToWorldPoint(inputPosEnd);
                shootDirection = inputPosWorldStart - inputPosWorldEnd;
                shootDirection = shootDirection.normalized * forceProjectileMobile.Evaluate(shootDirection.magnitude / 5f);
                if (shootDirection.magnitude > 0.2f)
                    shootSomething();
                
            }
            Debug.DrawLine(inputPosWorldStart, inputPosWorldActual, Color.grey);
            Debug.DrawRay(new Vector2(transform.position.x, transform.position.y), 6f * (inputPosWorldStart - inputPosWorldActual).normalized * forceProjectileMobile.Evaluate((inputPosWorldStart - inputPosWorldActual).magnitude / 5f), forceProjectileMobile.Evaluate((inputPosWorldStart - inputPosWorldActual).magnitude / 5f) > 0.2f ? Color.white : Color.red);
//            Debug.DrawRay(new Vector2(transform.position.x, transform.position.y), inputPosStart - inputPosActual, forceProjectileMobile.Evaluate((inputPosStart - inputPosActual).magnitude / 5f) > 0.2f ? Color.white : Color.red);
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
        shootDirection = transform.up;
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
        clone.GetComponent<Rigidbody2D>().AddForce(shootDirection * basicBulletForce);
    }

    public void ShootShotgun()
    {
        canShoot = false;
        const float spreadFactor = 0.4f;
        const int numProjectile = 5;
        for (int i = 0; i < numProjectile; i++)
        {
            GameObject clone = Instantiate(GameGenerator.ShotgunBullet, transform.TransformPoint(Vector3.up * (transform.localScale.y / 1.5f)), Quaternion.identity) as GameObject;
            GameGenerator.followedObject = clone.transform;
            Vector2 bulletDirection = Rng.ApplyInaccuracy(shootDirection, spreadFactor);
            clone.GetComponent<Rigidbody2D>().AddForce(bulletDirection * shotgunBulletForce);
        }
    }

    public void ShootGrenadeLauncher()
    {
        canShoot = false;
        const float spreadFactor = 0.2f;
        GameObject clone = Instantiate(GameGenerator.GrenadeLauncherBullet, transform.TransformPoint(Vector3.up * (transform.localScale.y / 1.2f)), Quaternion.identity) as GameObject;
        GameGenerator.followedObject = clone.transform;
        Vector2 bulletDirection = Rng.ApplyInaccuracy(shootDirection, spreadFactor);
        clone.GetComponent<Rigidbody2D>().AddForce(bulletDirection * grenadeLauncherBulletForce);
    }
}
