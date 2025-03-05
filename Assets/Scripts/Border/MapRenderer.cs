using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof(MeshRenderer))]
public class MapRenderer : MonoBehaviour
{
	public enum DrawMode {Default, DevlopmentMap}
	[Header("Draw Mode")]
	public DrawMode drawMode = DrawMode.Default;
	DrawMode prevDrawMode = DrawMode.Default;
	public MeshRenderer overlay;
	public EconomyManager economony;

	[Space(1)]
	public Vector2Int mapOffset;
	public Vector2 scaleOffset;
	public float seaThresshold = 0.5f;
	public Color mapColor;
    public Texture2D map;
	public RawImage miniMap;
	public MeshRenderer mapRenderer;
	[HideInInspector] public Color[,] mapData;

	private void Start()
	{
		UpdateMapData();
	}
	[ContextMenu("Set Map Data")]
	public void UpdateMapData()
	{
		mapData = MapBorderRenderer.GetPixelData(map);

	}
	public static Color[,] CapitalChange(Color[,] colorMap, Color targetColor, Color changeColor, Vector2Int coordinate, float radius)
	{
		int width = colorMap.GetLength(0);
		int height = colorMap.GetLength(1);
		float radiusSquared = radius * radius;

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
		mapData = CapitalChange(mapData, targetColor, changeColor, coordinate, radius);
		map = MapBorderRenderer.SetPixelData(new(map.width, map.height), mapData);
		SetMapTexture();
	}
	public void SetMapTexture(Texture2D texture) 
	{
		SetMapTexture(texture, mapRenderer);
		miniMap.texture = texture;
	}

	public void SetMapTexture(Texture2D texture, MeshRenderer target)
	{
		target.sharedMaterial.mainTexture = texture;
	}

	[ContextMenu("Update Map Texture")] public void SetMapTexture()
	{
		overlay.enabled = false;
		if (drawMode == DrawMode.Default) SetMapTexture(map);
		if (drawMode == DrawMode.DevlopmentMap && economony.populationMapChanged)
		{
			overlay.enabled = true;
			Texture2D devlopmentMap = GlobalData.CreateNoiseTexture(GlobalData.NormalizeLocalNoiseMap(economony.populationMap));
			SetMapTexture(devlopmentMap);
			SetMapTexture(map, overlay);
		}
	}
	public static Color RGB(Color c) { c.a = 1; return c; }
	public Vector2Int MapPosition(Vector2 position)
	{
		float xScale = transform.localScale.x / scaleOffset.x;
		float yScale = transform.localScale.y / scaleOffset.y;
		return new(Mathf.RoundToInt(position.x / xScale) + map.width / 2 + mapOffset.x,
			Mathf.RoundToInt(position.y / yScale) + map.height / 2 + mapOffset.y);
	}
	public Vector2 WorldPosition(Vector2Int position)
	{
		float xScale = transform.localScale.x / scaleOffset.x;
		float yScale = transform.localScale.y / scaleOffset.y;
		float x = (position.x - map.width / 2 - mapOffset.x) * xScale;
		float y = (position.y - map.height / 2 - mapOffset.y) * yScale;
		return new Vector2(x, y);
	}
	public Vector2Int MapPositionViaDisplacement(Vector2Int position)
	{
		return position + new Vector2Int(map.width/2, map.height/2);
	}
	public Vector2Int MapPositionInOriginContext(Vector2 position) { return MapPosition(position) - new Vector2Int(map.width/2, map.height/2); }

	
	private void OnValidate()
	{
		if (mapRenderer == null) return;
		mapRenderer.sharedMaterial.color = mapColor;
		OnDrawModeChange();
	}
	public void OnDrawModeChange()
	{
		if (prevDrawMode != drawMode) economony.populationMapChanged = true;
		prevDrawMode = drawMode;
		SetMapTexture();
	}

	[ContextMenu("Assign Nation Colors")]
	public void AssignNationColors()
	{
		Color[] colors = GetUniqueColors(mapData);
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
		for (int i = 0; i < colors.Length; i++)
		{
			if (positions[i].x < 0 || positions[i].y < 0 || positions[i].x >= map.width || positions[i].y >= map.height) continue;
			if (mapData[positions[i].x, positions[i].y].a < seaThresshold) continue;
			mapData[positions[i].x, positions[i].y] = colors[i];
		}
		map = MapBorderRenderer.SetPixelData(new(map.width, map.height), mapData);
		SetMapTexture();
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

	public void OnMapRenderModeChange(int value)
	{
		drawMode = (DrawMode) value;
		OnDrawModeChange();
	}

	[ContextMenu("Print Unique Colors")]
	void PrintUniqueColors()
	{
		var uniqueColors = GetUniqueColors(MapBorderRenderer.GetPixelData(map));
		foreach (Color color in uniqueColors)
		{
			print(color);
		}
		print(uniqueColors.Length);
	}
} 
