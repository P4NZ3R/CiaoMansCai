using UnityEngine;
using UnityEngine.Experimental.Director;
using System.Runtime.Remoting.Messaging;
using System.Collections.Generic;

public class GameGenerator : MonoBehaviour
{
    public string seed = "CiaoMansCai";
    public int maxMovements = 100;
    public GameObject[] map;
    public float visualGeneratedMass;
    public bool positionFix;
    const float planetMinMass = 1f;
    const float planetMaxMass = 6f;
    const float planetTotalMass = 25f;
    Dictionary<int, Vector3[]> debugMovements;

    // Use this for initialization
    void Start()
    {
        debugMovements = new Dictionary<int, Vector3[]>();
        GenerateMap();   
    }
	
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            seed = "";
            GenerateMap();
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            GenerateMap();
        }

        foreach (KeyValuePair<int, Vector3[]> kv in debugMovements)
            Debug.DrawLine(kv.Value[0], kv.Value[1], Color.red);
    }

    string RandomSeed()
    {
        return System.DateTime.Now.Ticks.ToString();
    }

    void GenerateMap()
    {
        if (map != null)
        {
            for (int i = 0; i < map.Length; i++)
            {
                if (map[i] != null)
                    Destroy(map[i]);
                map[i] = null;
            }
        }

        if (string.IsNullOrEmpty(seed))
            seed = RandomSeed();
        Rng.SetSeed(seed);
        GameObject planet = Resources.Load("Planet") as GameObject;

        debugMovements.Clear();
        float totalMass = planetTotalMass;
        visualGeneratedMass = 0;
        map = new GameObject[20];

        int movements = 0;
        for (int i = 0; i < map.Length && totalMass > planetMinMass; i++)
        {
            Vector3 randomPos = new Vector3(Rng.GetNumber(-16, 16f), Rng.GetNumber(-9f, 9f), 0);
            float randomMass = Rng.GetNumber(planetMinMass, planetMaxMass);
            if (randomMass > totalMass || i == map.Length - 1)
                randomMass = totalMass;

            for (int j = 0; positionFix && j < map.Length && movements < maxMovements; j++)
            {
                if (map[j] == null)
                    continue;

                Vector3 controlledPos = map[j].transform.position;
                float controlledMass = map[j].transform.localScale.x;
                Vector3 dir = (randomPos - controlledPos).normalized;
                float d = Vector3.Distance(randomPos - dir * (randomMass / 2f), controlledPos + dir * (controlledMass / 2f));
                float neededDistance = (randomMass + controlledMass) / 2f;
                if (d < neededDistance)
                {
                    Vector3 newPos = controlledPos + dir * (controlledMass / 2f) + dir * 1.1f * (randomMass / 2f + neededDistance);
                    Debug.Log("Planet" + i + " con " + map[j].name + " = " + "current:" + d + " needed:" + neededDistance);
                    if (debugMovements.ContainsKey(movements))
                        Debug.LogError(movements);
                    debugMovements.Add(movements + i * maxMovements, new Vector3[2]{ randomPos, newPos });
                    randomPos = newPos;
                    j = 0;
                    movements++;
                }
            }
            if (movements < maxMovements)
            {
                map[i] = Instantiate(planet, randomPos, Quaternion.identity) as GameObject;
                map[i].transform.localScale = new Vector3(randomMass, randomMass, randomMass);
                map[i].name = "Planet" + i;
                totalMass -= randomMass;
                visualGeneratedMass += randomMass;
            }
            else
            {
                Debug.LogError("Max movements: " + movements);
            }
            movements = 0;
        }
    }
}
