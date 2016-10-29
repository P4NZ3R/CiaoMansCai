using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    static ButtonManager instance;

    public static ButtonManager Instance
    {
        get{ return instance; }
        set
        {
            if (instance)
                Destroy(instance);
            instance = value;
        }
    }

    public bool requestMovement1;
    public bool requestMovement2;
    GameObject canvas;
    Text currentWeaponText;
    int currentWeapon;

    public int CurrentWeapon
    {
        get{ return currentWeapon; }
        set
        {
            if (value > 2)
                currentWeapon = 0;
            else
                currentWeapon = value;
            currentWeaponText.text = (currentWeapon + 1).ToString();
        }
    }

    void Start()
    {
        instance = this;
        canvas = GameObject.Find("Canvas");
        currentWeaponText = GameObject.Find("switchWeapon").transform.GetChild(0).GetComponent<Text>();
    }

    //    void Update()
    //    {
    //        if (Input.touchCount == 1 && canvas.activeSelf)
    //            canvas.SetActive(false);
    //        if (Input.touchCount != 1 && !canvas.activeSelf)
    //            canvas.SetActive(true);
    //    }

    public void RequestMovement1(bool value)
    {
        requestMovement1 = value;
    }

    public void RequestMovement2(bool value)
    {
        requestMovement2 = value;
    }

    public void SwitchWeapon()
    {
        CurrentWeapon++;
    }
}
