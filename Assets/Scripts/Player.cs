using UnityEngine;
using System.Collections;

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
    // Use this for initialization
    void Start()
    {
        isActiveSign = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
//        isActiveSign.gameObject.GetComponent<Animation>().;
        RefreshPosition(Vector2.Angle(transform.position - planet.transform.position, Vector3.right));
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
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isAlive)
            return;
        //ruota il player secondo la posizione sul pianeta
        Vector3 vectorToTarget = planet.transform.position - transform.position;
        float anglePos = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
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
        transform.position = planet.transform.position + planet.transform.localScale.x / 2f * newNormal * 1.2f;
        anglePos = angle;
    }
}
