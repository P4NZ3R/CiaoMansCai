using UnityEngine;

public static class GameColors
{
    public static Color GetRandomColor()
    {
        HSBColor hsbc = HSBColor.FromColor(Color.red);
        int colorSelect = Rng.GetNumber(0, 62);
        if (colorSelect <= 60)
            hsbc.h += colorSelect / 60f;
        else
            hsbc.s = 0;
        return hsbc.ToColor();
    }
}
