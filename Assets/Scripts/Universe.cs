using UnityEngine;

public static class Universe
{
    public static Planet[] map;

    [System.Serializable]
    public struct Planet
    {
        public GameObject go;
        public Vector3 pos;
        public float mass;
        public Color color;
        public int teamOwner;
        public int playerOwner;
    }

    public static Planet[] PlanetSort(Planet[] p)
    {
        for (int i = 0; i < p.Length; i++)
        {
            int max = i;
            for (int j = i + 1; j < p.Length; j++)
            {
                if (Universe.map[j].mass > Universe.map[max].mass)
                {
                    max = j;
                }
            }
            Planet tmp = Universe.map[max];
            Universe.map[max] = Universe.map[i];
            Universe.map[i] = tmp;
        }
        return p;
    }

    public static bool PlanetExists(Universe.Planet[] m, int i)
    {
        return !(m[i].mass <= 0 || m[i].pos == Vector3.zero);
    }

    public static void DebugPlanets(Universe.Planet[] m)
    {
        string tmp = "";
        for (int i = 0; i < m.Length; i++)
        {
            if (!PlanetExists(m, i))
            {
                Debug.Log("planet☠");
                continue;
            }
            tmp = m[i].go.name + " pos:" + m[i].pos.ToString("F1") + " mass:" + m[i].mass.ToString("F2") + " teamOwner:" + m[i].teamOwner + " playerOwner:" + m[i].playerOwner + "\n"; 
            Debug.Log(tmp);
        }
    }
}

