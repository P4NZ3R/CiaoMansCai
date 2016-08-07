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
    public Planet[] map;

    float planetTotalMass;
    float planetMaxMass;
    Dictionary<int, Vector3[]> debugMovements;

    [System.Serializable]
    public struct Planet
    {
        public Vector3 pos;
        public float mass;
        public GameObject go;
    }

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
        if (map != null)
        {
            for (int i = 0; i < map.Length; i++)
            {
                if (map[i].go != null)
                    Destroy(map[i].go);
                map[i].go = null;
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
        map = new Planet[20];

        //genera numeri casuali per posizione e massa
        for (int i = 0; i < map.Length && totalMass > planetMinMass; i++)
        {
            map[i].pos = new Vector3(Rng.GetNumber(-16, 16f), Rng.GetNumber(-9f, 9f), 0);
            map[i].mass = Rng.GetNumber(planetMinMass, planetMaxMass);
            if (map[i].mass > totalMass || i == map.Length - 1)
                map[i].mass = totalMass;
            totalMass -= map[i].mass;
        }
        //ordina i pianeti per massa decrescente
        map = PlanetSort(map);

        //fix planet position
        int movements = 0;
        for (int i = 0; positionFix && i < map.Length; i++)
        {
            if (!PlanetExists(map, i))
                continue;
            
            for (int j = 0; j < i && movements < maxMovements; j++)
            {
                if (!PlanetExists(map, j))
                    continue;
      
                Vector3 dir = (map[i].pos - map[j].pos).normalized;
                float d = Vector3.Distance(map[i].pos - dir * (map[i].mass / 2f), map[j].pos + dir * (map[j].mass / 2f));
                float neededDistance = (map[i].mass + map[j].mass) / 2f;
                if (d < neededDistance)
                {
                    Debug.Log("Planet" + i + " con " + "Planet" + j + " = " + "current:" + d + " needed:" + neededDistance);
                    Vector3 newPos = map[j].pos + dir * (map[j].mass / 2f + map[i].mass / 2f + neededDistance * (1.3f + movements / 10f));
                    if (debugMovements.ContainsKey(movements))
                        Debug.LogError(movements);
                    debugMovements.Add(movements + i * maxMovements, new []{ map[i].pos, newPos });
                    map[i].pos = newPos;
                    j = -1;
                    movements++;
                }
            }
            if (movements >= maxMovements)
            {
                map[i].mass = -1;
                map[i].pos = Vector3.zero;
                Debug.LogError("Planet" + i + " destroyed!");
            }
            movements = 0;
        }

        //move planets to center
        Vector3 average = Vector3.zero; 
        int k = 0;
        for (int i = 0; i < map.Length; i++)
        {
            if (!PlanetExists(map, i))
                continue;
            k++;
            average += map[i].pos;
        }
        average /= k;
        for (int i = 0; i < map.Length; i++)
        {
            if (!PlanetExists(map, i))
                continue;
            map[i].pos -= average;
        }
        #if UNITY_EDITOR
        foreach (KeyValuePair<int, Vector3[]> kv in debugMovements)
        {
            kv.Value[0] -= average;
            kv.Value[1] -= average;
        }
        #endif

        //instantiate planets
        for (int i = 0; i < map.Length; i++)
        {
            if (!PlanetExists(map, i))
                continue;
            map[i].go = Instantiate(planet, map[i].pos, Quaternion.identity) as GameObject;
            map[i].go.transform.localScale = new Vector3(map[i].mass, map[i].mass, 1);
            map[i].go.name = "Planet" + i;
        }
    }

    bool PlanetExists(Planet[] m, int i)
    {
        return !(m[i].mass <= 0 || m[i].pos == Vector3.zero);
    }

    Planet[] PlanetSort(Planet[] p)
    {
        for (int i = 0; i < p.Length; i++)
        {
            int max = i;
            for (int j = i + 1; j < p.Length; j++)
            {
                if (map[j].mass > map[max].mass)
                {
                    max = j;
                }
            }
            Planet tmp = map[max];
            map[max] = map[i];
            map[i] = tmp;
        }
        return p;
    }
}
