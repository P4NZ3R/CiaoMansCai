using UnityEngine;

public static class GraphicGenerator
{
	public static string GetNewSeed()
	{
		int lenght = 2;
		string chars = "1234567890";
		string generatedSeed = "";
		for (int i = 0; i < lenght; i++)
			generatedSeed += chars[Rng.GetNumber(0, chars.Length)];
		return generatedSeed;
	}

	public static Sprite GetPlanetSprite(GameGenerator.Team team)
	{
		//creazione nuova sprite
		Texture2D t = new Texture2D(128, 128);
		t.filterMode = FilterMode.Bilinear;
		t.Apply();
		Sprite planetSprite = Sprite.Create(t, new Rect(Vector2.zero, new Vector2(t.width, t.height)), new Vector2(0.5f, 0.5f), t.width, 0, SpriteMeshType.Tight);
		planetSprite.name = "GeneratedSprite";

		//generazione texture
		string subSeed = GetNewSeed();
		System.Random rngStyle = new System.Random(team.styleSeed.GetHashCode());
		System.Random rngPlanet = new System.Random(subSeed.GetHashCode());
		int h = t.height;
		int w = t.width;
		//ULTRA RARE GLITCY PLANET!
		if (int.Parse(team.styleSeed[0] + "") == 0 && int.Parse(team.styleSeed[1] + "") == 0)
		{
			for (int y = 0; y < h; y++)
			{
				for (int x = 0; x < w; x++)
				{
					Color pixelColor = x < w / 2f + Mathf.Sin(y * h) * h / 2f ? team.color : team.color2;
					pixelColor.a = Mathf.Pow(x + 1f - (w + 0.5f) / 2f, 2f) + Mathf.Pow(y + 1f - (h + 0.5f) / 2f, 2f) < Mathf.Pow(w / 2f, 2f) ? 1f : 0f;
					planetSprite.texture.SetPixel(x, y, pixelColor);
				}
			}
		}
		else
		{
			float frequency = 1f * rngStyle.Next(5, 30);
			float amplitude = 1f / rngStyle.Next(5, 20);
			for (int y = 0; y < h; y++)
			{
				for (int x = 0; x < w; x++)
				{
					Color pixelColor = x < w / 2f + Mathf.Sin(y / frequency) * h * amplitude ? team.color : team.color2;
					pixelColor.a = Mathf.Pow(x + 1f - (w + 0.5f) / 2f, 2f) + Mathf.Pow(y + 1f - (h + 0.5f) / 2f, 2f) < Mathf.Pow(w / 2f, 2f) ? 1f : 0f;
					planetSprite.texture.SetPixel(x, y, pixelColor);
				}
			}
		}
		planetSprite.texture.Apply();
		return planetSprite;
	}
}
