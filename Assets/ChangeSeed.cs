using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ChangeSeed : MonoBehaviour
{
    public static string seed;

    public void NewSeed()
    {
        seed = GameObject.Find("seed").GetComponent<Text>().text;
    }
}
