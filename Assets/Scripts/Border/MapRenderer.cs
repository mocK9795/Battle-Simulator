using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

[RequireComponent (typeof(MeshRenderer))]
public class MapRenderer : MonoBehaviour
{
	public Vector2Int mapOffset;
	public float scaleOffset;
	public float seaThresshold = 0.5f;
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
		float xScale = transform.localScale.x / scaleOffset;
		float yScale = transform.localScale.y / scaleOffset;
		return new(Mathf.RoundToInt(position.x / xScale) + map.width / 2 + mapOffset.x,
			Mathf.RoundToInt(position.y / yScale) + map.height / 2 + mapOffset.y);
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

	public void SetColors(Color[] colors, Vector2Int[] positions)
	{
		var pixelData = MapBorderRenderer.GetPixelData(map);
		for (int i = 0; i < colors.Length; i++)
		{
			if (positions[i].x < 0 || positions[i].y < 0 || positions[i].x >= map.width || positions[i].y >= map.height) continue;
			if (pixelData[positions[i].x, positions[i].y].a < seaThresshold) continue;
			pixelData[positions[i].x, positions[i].y] = colors[i];
		}
		map = MapBorderRenderer.SetPixelData(new(map.width, map.height), pixelData);
		SetMapTexture(map);
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
