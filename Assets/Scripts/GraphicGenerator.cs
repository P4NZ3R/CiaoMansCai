using UnityEngine;
using System;

public static class GraphicGenerator
{
	const int pixelPerUnit = 8;

	public static string GetNewSeed()
	{
		int lenght = 2;
		string chars = "1234567890";
		string generatedSeed = "";
		for (int i = 0; i < lenght; i++)
			generatedSeed += chars[Rng.GetNumber(0, chars.Length)];
		return generatedSeed;
	}

	public static Sprite GetPlanetSprite(GameGenerator.Team team, Vector3 scale)
	{
		//creazione nuova sprite
		Texture2D t = new Texture2D(Mathf.FloorToInt(pixelPerUnit * scale.x), Mathf.FloorToInt(pixelPerUnit * scale.y));
		t.filterMode = pixelPerUnit < 16 ? FilterMode.Point : FilterMode.Bilinear;
		t.Apply();
		Sprite planetSprite = Sprite.Create(t, new Rect(Vector2.zero, new Vector2(t.width, t.height)), new Vector2(0.5f, 0.5f), t.width, 0, SpriteMeshType.Tight);
		planetSprite.name = "GeneratedSprite";

		//generazione texture
		string subSeed = GetNewSeed();
		System.Random rngStyle = new System.Random(team.styleSeed.GetHashCode());
		System.Random rngPlanet = new System.Random(subSeed.GetHashCode());
		int h = t.height;
		int w = t.width;

		// get the seed in binary
		string binarySeed = Convert.ToString(int.Parse(team.styleSeed), 2); 				//Convert to binary in a string
		for (int i = binarySeed.Length; i < 8; i++)
			binarySeed = "0" + binarySeed;													// add 0s at the start

		// xnor the first 4 bits with the last 4 bits
		string xnorSeed = "";
		for (int i = 0; i < binarySeed.Length / 2; i++)
			xnorSeed += binarySeed[i] == binarySeed[i + binarySeed.Length / 2] ? "1" : "0";	// xnor

		// look in the xnor result how many 1s there are
		int genSelection = 0;
		for (int i = 0; i < xnorSeed.Length; i++)
			genSelection += int.Parse(xnorSeed[i] + "");
		
		Debug.Log(team.styleSeed + " -> " + xnorSeed + " -> " + genSelection);

		// planet generation variables
		int tiles = 1;
		for (int i = 0; i < binarySeed.Length; i++)
			tiles += int.Parse(binarySeed[i] + "");
		float frequency = 1f * rngStyle.Next(5, 30);
		float amplitude = 1f / rngStyle.Next(5, 20);

		// sprite texture generation
		for (int y = 0; y < h; y++)
		{
			for (int x = 0; x < w; x++)
			{
				Color pixelColor;
				switch (genSelection)
				{
				// RARE VAPORWAVE PLANET!
					case 0:
						pixelColor = x < w / 2f + Mathf.Sin(y * h) * h / 2f ? team.color : team.color2;
						break;
				// checker planet
					case 3:
						int xTile = Mathf.FloorToInt(x / (w / tiles));
						int yTile = Mathf.FloorToInt(y / (h / tiles));
						pixelColor = (xTile + yTile) % 2f == 0 ? team.color : team.color2;
						break;
				// waves planet
					default:
						pixelColor = x < w / 2f + Mathf.Sin(y / frequency) * h * amplitude ? team.color : team.color2;
						break;
				}
				pixelColor.a = Mathf.Pow(x + 1f - (w + 0.5f) / 2f, 2f) + Mathf.Pow(y + 1f - (h + 0.5f) / 2f, 2f) < Mathf.Pow(w / 2f, 2f) ? 1f : 0f;
				planetSprite.texture.SetPixel(x, y, pixelColor);
			}
		}

		planetSprite.texture.Apply();
		return planetSprite;
	}
}
