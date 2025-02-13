using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof(MeshRenderer))]
public class MapRenderer : MonoBehaviour
{
	public Color mapColor;
    public Texture2D map;
	public RawImage miniMap;
	public MeshRenderer mapRenderer;

	public static Color[,] CapitalChange(Color[,] colorMap, Color targetColor, Color changeColor, Vector2Int coordinate, float radius)
	{
		int width = colorMap.GetLength(0);
		int height = colorMap.GetLength(1);
		float radiusSquared = radius * radius;
		print(width); print(height);

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				Vector2Int currentCoord = new Vector2Int(x, y);
				float distanceSquared = (currentCoord - coordinate).sqrMagnitude;
				if (distanceSquared <= radiusSquared && colorMap[x, y] == RGB(targetColor))
				{
					colorMap[x, y] = RGB(changeColor);
				}
			}
		}

		return colorMap;
	}
	public void CapitalChange(Color targetColor, Color changeColor, Vector2Int coordinate, float radius)
	{
		Color[,] pixelData = MapBorderRenderer.GetPixelData(map);
		pixelData = CapitalChange(pixelData, targetColor, changeColor, coordinate, radius);
		map = MapBorderRenderer.SetPixelData(new(map.width, map.height), pixelData);
		SetMapTexture(map);
	}

	public void SetMapTexture(Texture2D texture) 
	{
		mapRenderer.sharedMaterial.mainTexture = texture;
		miniMap.texture = texture;
	}
	[ContextMenu("Update Map Texture")] public void SetMapTexture()
	{
		SetMapTexture(map);
	}

	public static Color RGB(Color c) { c.a = 1; return c; }
	public Vector2Int MapPosition(Vector2 position)
	{
		float xScale = transform.localScale.x;
		float yScale = transform.localScale.y;
		return new(Mathf.RoundToInt(position.x / xScale * 10) + map.width / 2,
			Mathf.RoundToInt(position.y / yScale * 10) + map.height / 2);
	}

	private void OnValidate()
	{
		if (mapRenderer == null) return;
		mapRenderer.sharedMaterial.color = mapColor;
	}

	[ContextMenu("Assign Nation Colors")]
	public void AssignNationColors()
	{
		Color[] colors = GetUniqueColors(MapBorderRenderer.GetPixelData(map));
		List<Nation> nations = new(BattleManager.GetAllNations());
		
		foreach (Color color in colors)
		{
			if (nations.Count == 0) break;
			Nation nearestNation = nations[0];
			foreach (Nation nation in nations)
			{
				if (GlobalData.Closest(nearestNation.nationColor, nation.nationColor, color) != nation.nationColor) continue;
				nearestNation = nation;
			}

			nearestNation.nationColor = color;
			nations.Remove(nearestNation);
		}
	}

	public Color[] GetUniqueColors(Color[,] pixelData) {
		List<Color> uniqueColors = new List<Color>();

		for (int i = 0; i < pixelData.GetLength(0); i++)
		{
			for (int j = 0; j < pixelData.GetLength(1); j++)
			{
				if (pixelData[i, j].a < mapColor.a) continue;
				bool found = false;
				foreach (Color color in uniqueColors)
				{
					if (!color.CompareRGB(pixelData[i, j])) continue;
					found = true; break;
				}
				if (found) continue;
				uniqueColors.Add(pixelData[i, j]);
			}
		}

		return uniqueColors.ToArray();
	}
} 
