using UnityEngine;
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
    // Use this for initialization
    void Start()
    {
        isActiveSign = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
//        isActiveSign.gameObject.GetComponent<Animation>().;
        RefreshPosition(Vector2.Angle(transform.position - planet.transform.position, Vector3.right));
        planet.GetComponent<SpriteRenderer>().color = GameGenerator.GetTeam(teamId).color;
    }

    void Update()
    {
        if (!isAlive || gameGenerator.activeTeam != teamId || gameGenerator.activePlayer != playerId)
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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject clone = Instantiate(GameGenerator.BasicBullet, transform.TransformPoint(Vector3.up * (transform.localScale.y / 1.8f)), Quaternion.identity) as GameObject;
            clone.GetComponent<Rigidbody2D>().AddForce(transform.up * basicBulletForce);
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
}
