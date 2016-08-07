using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    public GameObject planet;
    public int teamId;
    public int playerId;
    // Use this for initialization
    void Start()
    {
	
    }
	
    // Update is called once per frame
    void FixedUpdate()
    {
        //ruota il player secondo la posizione sul pianeta
        Vector3 vectorToTarget = planet.transform.position - transform.position;
        float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = q;
    }
}
