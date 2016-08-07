using UnityEngine;

public static class Universe
{
    public static Planet[] map;

    [System.Serializable]
    public struct Planet
    {
        public Vector3 pos;
        public float mass;
        public GameObject go;
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
}
