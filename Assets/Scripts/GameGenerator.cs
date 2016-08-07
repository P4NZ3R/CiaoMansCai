using UnityEngine;
using System.Collections.Generic;

// il nostro seed speciale <3 = 636061745828876374
// 636061842353274677
using UnityEngine.Networking.Match;

public class GameGenerator : MonoBehaviour
{
    [Header("Generation")]
    public string seed = "CiaoMansCai";
    public bool positionFix;
    public int maxMovements = 50;

    [Header("Players")]
    public int activeTeam = 0;
    public int activePlayer = 0;
    public Team[] team;

    Camera mainCamera;

    const float planetMinMass = 2f;
    float planetTotalMass;
    float planetMaxMass;
    Dictionary<int, Vector3[]> debugMovements;

    public struct Team
    {
        public Color color;
        public GameObject[] players;
        public GameObject[] planets;
    }

    // Use this for initialization
    void Start()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        debugMovements = new Dictionary<int, Vector3[]>();
        GenerateMap();   
        EndTurn();
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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EndTurn();
        }
        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            if (mainCamera.orthographicSize > 9)
                mainCamera.orthographicSize -= 3;
            
        }
        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            if (mainCamera.orthographicSize < 39)
                mainCamera.orthographicSize += 3;
        }
        #if UNITY_EDITOR
        foreach (KeyValuePair<int, Vector3[]> kv in debugMovements)
            Debug.DrawLine(kv.Value[0], kv.Value[1], Color.white);
        #endif
        //posiziona la telecamera in base al player
        if (Vector2.Distance(mainCamera.transform.position, team[activeTeam].players[activePlayer].transform.position - Vector3.forward * 10f) > 0.1f)
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, team[activeTeam].players[activePlayer].transform.position - Vector3.forward * 10f, Time.deltaTime);
        else
            mainCamera.transform.position = team[activeTeam].players[activePlayer].transform.position - Vector3.forward * 10f;
        mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, team[activeTeam].players[activePlayer].transform.rotation * Quaternion.Euler(0, 0, 90), Time.deltaTime * 2f);
    }

    string RandomSeed()
    {
        return System.DateTime.Now.Ticks.ToString();
    }

    void EndTurn()
    {
        if (activeTeam >= team.Length - 1)
        {
            activeTeam = 0;
            if (activePlayer >= team[activeTeam].players.Length - 1)
            {
                activePlayer = 0;
            }
            else
            {
                activePlayer++;
            }
        }
        else
        {
            activeTeam++;
        }
    }

    void GenerateMap()
    {
        //distrugge i precedenti pianeti se presenti
        if (Universe.map != null)
        {
            for (int i = 0; i < Universe.map.Length; i++)
            {
                if (Universe.map[i].go != null)
                    Destroy(Universe.map[i].go);
                Universe.map[i].go = null;
            }
        }
        //distrugge i precedenti player se presenti
        if (team != null)
        {
            for (int i = 0; i < team.Length; i++)
            {
                for (int j = 0; j < team[i].players.Length; j++)
                {
                    if (team[i].players[j] != null)
                    {
                        Destroy(team[i].players[j]);
                        team[i].players[j] = null;
                    }
                }
            }
        }
        //se il seed non e presente lo genera casualmente
        if (string.IsNullOrEmpty(seed))
            seed = RandomSeed();
        Rng.SetSeed(seed);

        //carica i prefab
        GameObject planet = Resources.Load("Planet") as GameObject;
        GameObject player = Resources.Load("Player") as GameObject;
        debugMovements.Clear();

        //genera un numero di giocatori per team casuale
        team = new Team[2];
        int numPlayer = Rng.GetNumber(1, 3 + 1);
        mainCamera.orthographicSize = 15 + (numPlayer - 1) * 4;
        for (int i = 0; i < team.Length; i++)
        {
            team[i].color = GameColors.GetRandomColor();
            team[i].players = new GameObject[numPlayer];
            team[i].planets = new GameObject[numPlayer];
        }

        //genera numeri casuali per posizione e massa
        planetTotalMass = Rng.GetNumber(10f * numPlayer, 25f * numPlayer);
        planetMaxMass = planetTotalMass / (numPlayer * 2.5f);
        float totalMass = planetTotalMass;
        Universe.map = new Universe.Planet[20];
        for (int i = 0; i < Universe.map.Length && totalMass > planetMinMass; i++)
        {
            Universe.map[i].pos = new Vector3(Rng.GetNumber(-14 - 2f * numPlayer, 14f + 2f * numPlayer), Rng.GetNumber(-4.5f - 0.5f * numPlayer, 4.5f + 0.5f * numPlayer), 0);
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
//                    Debug.Log("Planet" + i + " con " + "Planet" + j + " = " + "current:" + d + " needed:" + neededDistance);
                    Vector3 newPos = Universe.map[j].pos + dir * (Universe.map[j].mass / 2f + Universe.map[i].mass / 2f + neededDistance * (1.2f + movements / (25f * numPlayer)));
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
        int numPlanets = 0;
        for (int i = 0; i < Universe.map.Length; i++)
        {
            if (!Universe.PlanetExists(Universe.map, i))
                continue;
            Universe.map[i].go = Instantiate(planet, Universe.map[i].pos, Quaternion.identity) as GameObject;
            Universe.map[i].go.transform.localScale = new Vector3(Universe.map[i].mass, Universe.map[i].mass, 1);
            Universe.map[i].go.name = "Planet" + i;
            Universe.map[i].go.GetComponent<SpriteRenderer>().color = Universe.map[i].color = GameColors.GetRandomColor();
            Universe.map[i].teamOwner = -1;
            Universe.map[i].playerOwner = -1;
            numPlanets++;
        }

        //instantiate player
        Universe.PlanetSort(Universe.map);
        for (int i = 0; i < team.Length; i++)
        {
            for (int j = 0; j < team[i].players.Length; j++)
            {
                int rng = Rng.GetNumber(0, numPlanets);//planet selected
                team[i].planets[j] = Universe.map[rng].go;
                if (Universe.map[rng].go == null)
                {
                    Debug.LogError("Player try to spawn on an anexisting planet!\nid:" + rng);
                    return;
                }
                Vector3 normalDir = new Vector3(Rng.GetNumber(0f, 1f) - 0.5f, Rng.GetNumber(0f, 1f) - 0.5f).normalized;
                Vector3 posPlayer = Universe.map[rng].pos + (Universe.map[rng].mass + 1f) / 2f * normalDir;
                team[i].players[j] = Instantiate(player, posPlayer, Quaternion.identity) as GameObject;
                team[i].players[j].GetComponent<Player>().gameGenerator = GetComponent<GameGenerator>();
                team[i].players[j].GetComponent<Player>().planet = Universe.map[rng].go;
                team[i].players[j].GetComponent<Player>().teamId = i;
                team[i].players[j].GetComponent<Player>().playerId = j;
                team[i].players[j].GetComponent<SpriteRenderer>().color = team[i].color;
                Universe.map[rng].teamOwner = i;
                Universe.map[rng].playerOwner = j;

                numPlanets--;
                Universe.Planet tmp = Universe.map[rng]; 
                Universe.map[rng] = Universe.map[numPlanets];
                Universe.map[numPlanets] = tmp;
            }
        }
    }
}
