using UnityEngine;
using System.Collections.Generic;

// il nostro seed speciale <3 = 636061745828876374
// 636061842353274677

public class GameGenerator : MonoBehaviour
{
    [Header("Generation")]
    public string seed = "CiaoMansCai";
    public bool positionFix;
    public int maxMovements = 100;

    [Header("Planets")]
    public float planetMinMass;

    float planetTotalMass;
    float planetMaxMass;
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

        #if UNITY_EDITOR
        foreach (KeyValuePair<int, Vector3[]> kv in debugMovements)
            Debug.DrawLine(kv.Value[0], kv.Value[1], Color.red);
        #endif
    }

    string RandomSeed()
    {
        return System.DateTime.Now.Ticks.ToString();
    }

    void GenerateMap()
    {
        if (Universe.map != null)
        {
            for (int i = 0; i < Universe.map.Length; i++)
            {
                if (Universe.map[i].go != null)
                    Destroy(Universe.map[i].go);
                Universe.map[i].go = null;
            }
        }

        if (string.IsNullOrEmpty(seed))
            seed = RandomSeed();
        Rng.SetSeed(seed);
        GameObject planet = Resources.Load("Planet") as GameObject;

        debugMovements.Clear();
        planetTotalMass = Rng.GetNumber(10f, 25f);
        planetMaxMass = planetTotalMass / 2f;
        float totalMass = planetTotalMass;
        Universe.map = new Universe.Planet[20];

        //genera numeri casuali per posizione e massa
        for (int i = 0; i < Universe.map.Length && totalMass > planetMinMass; i++)
        {
            Universe.map[i].pos = new Vector3(Rng.GetNumber(-16, 16f), Rng.GetNumber(-9f, 9f), 0);
            Universe.map[i].mass = Rng.GetNumber(planetMinMass, planetMaxMass);
            if (Universe.map[i].mass > totalMass || i == Universe.map.Length - 1)
                Universe.map[i].mass = totalMass;
            totalMass -= Universe.map[i].mass;
        }
        //ordina i pianeti per massa decrescente
        Universe.map = Universe.PlanetSort(Universe.map);

        //fix planet position
        int movements = 0;
        for (int i = 0; positionFix && i < Universe.map.Length; i++)
        {
            if (!Universe.PlanetExists(Universe.map, i))
                continue;
            
            for (int j = 0; j < i && movements < maxMovements; j++)
            {
                if (!Universe.PlanetExists(Universe.map, j))
                    continue;
      
                Vector3 dir = (Universe.map[i].pos - Universe.map[j].pos).normalized;
                float d = Vector3.Distance(Universe.map[i].pos - dir * (Universe.map[i].mass / 2f), Universe.map[j].pos + dir * (Universe.map[j].mass / 2f));
                float neededDistance = (Universe.map[i].mass + Universe.map[j].mass) / 2f;
                if (d < neededDistance)
                {
                    Debug.Log("Planet" + i + " con " + "Planet" + j + " = " + "current:" + d + " needed:" + neededDistance);
                    Vector3 newPos = Universe.map[j].pos + dir * (Universe.map[j].mass / 2f + Universe.map[i].mass / 2f + neededDistance * (1.3f + movements / 10f));
                    if (debugMovements.ContainsKey(movements))
                        Debug.LogError(movements);
                    debugMovements.Add(movements + i * maxMovements, new []{ Universe.map[i].pos, newPos });
                    Universe.map[i].pos = newPos;
                    j = -1;
                    movements++;
                }
            }
            if (movements >= maxMovements)
            {
                Universe.map[i].mass = -1;
                Universe.map[i].pos = Vector3.zero;
                Debug.LogError("Planet" + i + " destroyed!");
            }
            movements = 0;
        }

        //move planets to center
        Vector3 average = Vector3.zero; 
        int k = 0;
        for (int i = 0; i < Universe.map.Length; i++)
        {
            if (!Universe.PlanetExists(Universe.map, i))
                continue;
            k++;
            average += Universe.map[i].pos;
        }
        average /= k;
        for (int i = 0; i < Universe.map.Length; i++)
        {
            if (!Universe.PlanetExists(Universe.map, i))
                continue;
            Universe.map[i].pos -= average;
        }
        #if UNITY_EDITOR
        foreach (KeyValuePair<int, Vector3[]> kv in debugMovements)
        {
            kv.Value[0] -= average;
            kv.Value[1] -= average;
        }
        #endif

        //instantiate planets
        for (int i = 0; i < Universe.map.Length; i++)
        {
            if (!Universe.PlanetExists(Universe.map, i))
                continue;
            Universe.map[i].go = Instantiate(planet, Universe.map[i].pos, Quaternion.identity) as GameObject;
            Universe.map[i].go.transform.localScale = new Vector3(Universe.map[i].mass, Universe.map[i].mass, 1);
            Universe.map[i].go.name = "Planet" + i;
        }
    }


}
