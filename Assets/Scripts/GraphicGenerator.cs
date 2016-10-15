using UnityEngine;
using System;
using System.Text;
using System.Reflection;

public static class GraphicGenerator
{
	const int pixelPerUnit = 8;

	public static string GetNewSeed()
	{
		int lenght = 2;
		string chars = "1234567890AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz";
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
		t.wrapMode = TextureWrapMode.Clamp;
		t.Apply();
		Sprite planetSprite = Sprite.Create(t, new Rect(Vector2.zero, new Vector2(t.width, t.height)), new Vector2(0.5f, 0.5f), t.width, 0, SpriteMeshType.Tight);
		planetSprite.name = "GeneratedSprite";

		//generazione texture
		int h = t.height;
		int w = t.width;

		// get the seed in binary

		string binarySeed = Convert.ToString(int.Parse(Encoding.UTF8.GetBytes(team.styleSeed)[0] + ""), 2); //Convert to binary in a string
		string binaryScnd = Convert.ToString(int.Parse(Encoding.UTF8.GetBytes(team.styleSeed)[1] + ""), 2); //Convert to binary in a string
		for (int i = binarySeed.Length; i < 8; i++)
			binarySeed = "0" + binarySeed;	// add 0s at the start

		// xnor the first 4 bits with the last 4 bits
		string xnorSeed = "";
		for (int i = 0; i < binarySeed.Length / 2; i++)
			xnorSeed += binarySeed[i] == binarySeed[i + binarySeed.Length / 2] ? "1" : "0";	// xnor

		// look in the xnor result how many '1' there are
		int xnorOneN = CountCharsInString(xnorSeed, '1'); // genSelection is the number of '1' in the xnor seed

		// planet generation variables
		int binaryOneN = CountCharsInString(binarySeed, '1'); // binaryOneN is the number of '1' in the binary seed

		int tiles = 1 + binaryOneN;
		float dotSize = 8 / (CountCharsInString(StringByteAnd(binarySeed, binaryScnd), '1') + 1);
		float amplitude = h / (binaryOneN * 2f);
		float frequency = w / (1 + binaryOneN * 3);

		// sprite texture generation
		for (int y = 0; y < h; y++)
		{
			for (int x = 0; x < w; x++)
			{
				Color pixelColor;
				int xTile = Mathf.FloorToInt(x / (w / (float)tiles));
				int yTile = Mathf.FloorToInt(y / (h / (float)tiles));
				switch (xnorOneN)
				{
				// RARE VAPORWAVE PLANET!
					case 0:
						pixelColor = x < h / 2f + Mathf.Sin(y * 41.8f) * 41.8f / 2f ? team.color : 
							(xTile + yTile) % (float)binarySeed.Length - binaryOneN == 0 ? team.color3 : 
							(xTile + yTile) % (float)binaryOneN == 0 ? team.color : team.color2;
						break;
				// dot planet
					case 1:
						pixelColor = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(w / 2f, h / 2f)) > w / (2f + dotSize) ? team.color : team.color2;
						break;
				// checker planet
					case 3:
						xTile = Mathf.FloorToInt(x / (w / (float)tiles));
						yTile = Mathf.FloorToInt(y / (h / (float)tiles));
						pixelColor = (xTile + yTile) % 2f == 0 ? team.color : team.color2;
						break;
				// waves planet
					default:
						pixelColor = x < (h / 2f) + Mathf.Sin(y / frequency) * amplitude ? team.color : team.color2;
						break;
				}
				pixelColor.a = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(w / 2f, h / 2f)) < (w / 2f) ? 1f : 0f;
				planetSprite.texture.SetPixel(x, y, pixelColor);
			}
		}

		planetSprite.texture.Apply();
		return planetSprite;
	}

	static string StringByteAnd(string b1, string b2)
	{
		string b = "";
		for (int i = 0; i < Mathf.Max(b1.Length, b2.Length); i++)
		{
			if (i < Mathf.Min(b1.Length, b2.Length))
				b += b1[i] == '1' && b2[i] == '1' ? 1 : 0;
			else
				b += 0;
		}
		return b;
	}

	static int CountCharsInString(string s, char c)
	{
		int n = 0;
		for (int i = 0; i < s.Length; i++)
			n += s[i] == c ? 1 : 0;
		return n;
	}
}
