using UnityEngine;

public static class GameColors
{
    public static Color GetRandomColor(float selection = 1f)
    {
        HSBColor hsbc = HSBColor.FromColor(Color.red);
        hsbc.s = hsbc.b = selection;
        int colorSelect = Rng.GetNumber(0, 8 + 1);
        if (colorSelect <= 7)
            hsbc.h += colorSelect / 7f;
        else
            hsbc.s = 0;
        return hsbc.ToColor();
    }
}
