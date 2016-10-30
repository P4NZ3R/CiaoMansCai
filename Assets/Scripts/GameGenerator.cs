using UnityEngine;
using System.Collections.Generic;

// il nostro seed speciale <3 = 636061745828876374
// il nostro seed speciale <4 = 636061842353274677
// il nostro seed speciale <5 = 636103326405180112
using UnityEngine.UI;

public class GameGenerator : MonoBehaviour
{
    static GameGenerator instance = null;

    //    [Header("Generation")]
    //    [SerializeField]string seed = "CiaoMansCai";

    public static string Seed { get { return ChangeSeed.seed; } set { ChangeSeed.seed = value; } }

    public int playerMin;
    public int playerMax;
    public int teamNum;
    public bool positionFix;
    public int maxMovements = 50;
    public bool drawPlanetRays = true;
    [HideInInspector]public static Transform followedObject;
    Vector2[] zoomStartPos;

    [Header("Players")]
    public int activeTeam = 0;

    public int ActiveTeam
    {
        get{ return activeTeam; }
        set
        {
            if (value >= team.Length)
                activeTeam = 0;
            else
                activeTeam = value;
        }
    }

    public Team[] team;
    GameObject basicBullet;
    GameObject shotgunBullet;
    GameObject grenadeLauncherBullet;

    public static GameObject BasicBullet { get { return instance.basicBullet; } }

    public static GameObject ShotgunBullet { get { return instance.shotgunBullet; } }

    public static GameObject GrenadeLauncherBullet { get { return instance.grenadeLauncherBullet; } }

    Camera mainCamera;

    float planetTotalMass;
    float planetMinMass;
    float planetMaxMass;
    Dictionary<int, Vector3[]> debugMovements;
    int currentProjectiles = 0;

    public static int CurrentProjectiles
    {
        get { return instance.currentProjectiles; }
        set
        {
            instance.currentProjectiles = value;
            if (instance.currentProjectiles <= 0)
            {
                if (instance.team[instance.activeTeam].players[instance.team[instance.activeTeam].ActivePlayer])
                    instance.team[instance.activeTeam].players[instance.team[instance.activeTeam].ActivePlayer].GetComponent<Player>().canShoot = true;
                instance.EndTurn();
            }
        }
    }

    public struct Team
    {
        public string styleSeed;
        public Color color;
        public Color color2;
        public Color color3;
        int activePlayer;

        public int ActivePlayer
        {
            get { return activePlayer; }
            set
            {
                if (value < players.Length)
                    activePlayer = value;
                else
                    activePlayer = 0;
            }
        }

        public GameObject[] players;
        public GameObject[] planets;
        public bool teamAlive;
    }

    public static Team GetTeam(int IDteam)
    {
        return instance.team[IDteam];
    }

    void Awake()
    {
        if (instance != null)
            Destroy(instance.gameObject);
        instance = this;

        teamNum = Mathf.Clamp(teamNum, 2, 8);
        playerMax = Mathf.Clamp(playerMax, 1, int.MaxValue);
        playerMin = Mathf.Clamp(playerMin, 1, playerMax);
        zoomStartPos = new Vector2[2];
    }

    // Use this for initialization
    void Start()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        debugMovements = new Dictionary<int, Vector3[]>();
        basicBullet = Resources.Load("BasicBullet") as GameObject;
        shotgunBullet = Resources.Load("ShotgunBullet") as GameObject;
        grenadeLauncherBullet = Resources.Load("GrenadeLauncherBullet") as GameObject;
        GenerateMap();   
    }
	
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            ChangeSeed.seed = "";
            activeTeam = team[activeTeam].ActivePlayer = 0;
            GenerateMap();
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            GenerateMap();
        }
//        if (Input.GetKeyUp(KeyCode.Space))
//        {
//            EndTurn();
//        }
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
        if (drawPlanetRays)
        {
            foreach (KeyValuePair<int, Vector3[]> kv in debugMovements)
                Debug.DrawLine(kv.Value[0], kv.Value[1], Color.white);
        }
        #endif
        //posiziona la telecamera in base al player
        if (followedObject)
        {
            if (Vector2.Distance(mainCamera.transform.position, followedObject.position - Vector3.forward * 10f) > 0.1f)
                mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, followedObject.position - Vector3.forward * 10f, Time.deltaTime * 3f);
            else
                mainCamera.transform.position = followedObject.position - Vector3.forward * 10f;
            //mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, team[activeTeam].players[team[activeTeam].turnCounter].transform.rotation * Quaternion.Euler(0, 0, 90), Time.deltaTime * 2f); 
        }
        //input mobile
        if (Input.touchCount > 1 && Input.touchCount < 3 && Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved)
        {
            mainCamera.orthographicSize += (Vector2.Distance(zoomStartPos[0], zoomStartPos[1]) - Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position)) * Time.deltaTime * 2;
        }
        if (Input.touchCount > 0)
            zoomStartPos[0] = Input.GetTouch(0).position;
        if (Input.touchCount > 1)
            zoomStartPos[1] = Input.GetTouch(1).position;
    }

    void OnDestroy()
    {
        instance = null;
    }

    string RandomSeed()
    {
//        return System.DateTime.Now.Ticks.ToString();
        return Random.Range(100000, 999999).ToString();
    }

    void EndTurn()
    {
        ActiveTeam++;
        int tmpPlayer = team[activeTeam].ActivePlayer;
        do
        {
            team[activeTeam].ActivePlayer++;
        }
        while(team[activeTeam].players[team[activeTeam].ActivePlayer] == null && tmpPlayer != team[activeTeam].ActivePlayer);
        if (tmpPlayer == team[activeTeam].ActivePlayer && team[activeTeam].players[tmpPlayer] == null)
        {
            team[activeTeam].teamAlive = false;
            if (CheckVictory() == -2)//se la partita non è finita e ce qualche squadra attiva
                EndTurn();
            else
                return;
        }
        followedObject = team[activeTeam].players[team[activeTeam].ActivePlayer].transform;
    }

    /// <summary>
    /// Checks the victory.
    /// </summary>
    /// <returns> -2 means the game is not end, -1 means all deaths, >=0 means id of the winner team</returns>
    int CheckVictory()
    {
        int victoryTeam = -1;
        for (int i = 0; i < team.Length; i++)
        {
            if (team[i].teamAlive)
            {
                if (victoryTeam == -1)
                {
                    victoryTeam = i;
                }
                else
                {
                    Debug.Log("ops!");
                    return -2;
                }
            }
        }
        if (victoryTeam >= 0)
        {
//            Debug.Log("the winner is team" + victoryTeam + "!!");
            GameObject.Find("MyText").GetComponent<Text>().text = "the winner is team" + victoryTeam + "!!";
            Invoke("BackToMenu", 5f);
        }
        return victoryTeam;
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
        if (string.IsNullOrEmpty(ChangeSeed.seed))
            ChangeSeed.seed = RandomSeed();
        Rng.SetSeed(ChangeSeed.seed);
        GameObject.Find("seed").GetComponent<Text>().text = ChangeSeed.seed + " ";
        //carica i prefab
        GameObject planet = Resources.Load("Planet") as GameObject;
        GameObject player = Resources.Load("Player") as GameObject;
        debugMovements.Clear();

        //genera un numero di giocatori per team casuale
        team = new Team[teamNum];
        int numPlayer = Rng.GetNumber(playerMin, playerMax + 1);
        int peopleMultiplier = numPlayer * team.Length;
        mainCamera.orthographicSize = 10 + peopleMultiplier;
        for (int i = 0; i < team.Length; i++)
        {
            team[i].styleSeed = GraphicGenerator.GetNewSeed();
            Color teamColor = GameColors.GetRandomColor();
            for (int j = 0; j < i; j++)
            {
                if (teamColor.Equals(team[j].color))
                {
                    teamColor = GameColors.GetRandomColor();
                    j = -1;
                }
            }
            team[i].teamAlive = true;
            team[i].color = teamColor;
            teamColor = GameColors.GetRandomColor();
            for (int j = 0; j < i; j++)
            {
                if (teamColor.Equals(team[j].color))
                {
                    teamColor = GameColors.GetRandomColor();
                    j = -1;
                }
            }
            team[i].color2 = teamColor;
            team[i].color3 = GameColors.GetRandomColor();
            team[i].players = new GameObject[numPlayer];
            team[i].planets = new GameObject[numPlayer];
            team[i].ActivePlayer = -1;
        }

        //genera numeri casuali per posizione e massa
        planetTotalMass = Rng.GetNumber(4f * peopleMultiplier, 10f * peopleMultiplier);
        planetMinMass = planetTotalMass * 0.3f / peopleMultiplier;
        planetMaxMass = planetTotalMass / peopleMultiplier;
        float totalMass = planetTotalMass;
        float sizeMapMultiplier = planetTotalMass / 4f;
        Universe.map = new Universe.Planet[3 * peopleMultiplier];
        for (int i = 0; i < Universe.map.Length && totalMass > planetMinMass; i++)
        {
//            Universe.map[i].pos = new Vector3(Rng.GetNumber(-14 - peopleMultiplier, 14f + peopleMultiplier), Rng.GetNumber(-4.5f - 0.25f * peopleMultiplier, 4.5f + 0.25f * peopleMultiplier), 0);
            Universe.map[i].pos = new Vector3(Rng.GetNumber(-sizeMapMultiplier, sizeMapMultiplier), Rng.GetNumber(-sizeMapMultiplier / 2f, sizeMapMultiplier / 2f), 0);
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
                Debug.Log("Planet" + i + " destroyed!");
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
            Universe.map[i].go = Instantiate(planet, Universe.map[i].pos, Quaternion.Euler(0f, 0f, Rng.GetNumber(0, 360))) as GameObject;
            Universe.map[i].go.transform.localScale = new Vector3(Universe.map[i].mass, Universe.map[i].mass, 1);
            Universe.map[i].go.name = "Planet" + i;
            Universe.map[i].go.GetComponent<SpriteRenderer>().color = Universe.map[i].color = GameColors.GetRandomColor(0.4f);
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
        EndTurn();
    }

    void BackToMenu()
    {
        Application.LoadLevel(0);
    }
}
