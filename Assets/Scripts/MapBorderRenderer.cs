using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapBorderRenderer : MonoBehaviour
{
    public float scale;
    public float colliderScale;
    public Vector2 colliderOffset;
    public float lineWidth;
    [Range(0f, 1f)]
    public float transparency;
    public Material borderMaterial;
    public Texture2D map;
    public enum OutlineMode { ConvexHull, NearestPoint, Default, Polygon, MarchingSquares};
    public OutlineMode outlineMode;
    public bool onlySetCollision;
    public bool colliderIsTriger;
    public LayerMask collisionIncludeLayers;
    public LayerMask collisionExcludeLayers;

    public RawImage miniMapImage;

    [ContextMenu("Draw Borders")]
    public void DrawAllBorders()
    {
        RemoveBorders();
        Color[,] pixelData = GetPixelData(map);
        Nation[] nations = BattleManager.GetAllNations();
        var rawBorderPoints = BorderDetection.GetBorderPoints(map);
        var mergedBorderPoints = MergeBordersByColor(rawBorderPoints, pixelData);
        Dictionary<Nation, int> nationLineRendererCounts = new();
        Dictionary<Nation, List<List<Vector2Int>>> nationBorderPoints = new();
        for (int i = 0; i < nations.Length; i++)
        {
            Nation nation = nations[i];
            nationBorderPoints.Add(nation, mergedBorderPoints[nation.nationColor]);
            nationLineRendererCounts.Add(nation, nationBorderPoints[nation].Count);
        }
        var lineRenderers = GetLineRenderers(nationLineRendererCounts);

        for (int n = 0; n < nations.Length; n++)
        {
            Nation nation = nations[n];
            for (int i = 0; i < lineRenderers[nation].Count; i++)
            {
                if (nationBorderPoints[nation].Count <= i) break;
                DrawMapBorder(lineRenderers[nation][i], nationBorderPoints[nation][i], pixelData);
            }
        }
    }
    public Dictionary<Color, List<List<Vector2Int>>> MergeBordersByColor(List<List<Vector2Int>> regions, Color[,] pixelData)
    {
        Dictionary<Color, List<List<Vector2Int>>> mergedBorders = new Dictionary<Color, List<List<Vector2Int>>>();

        foreach (var region in regions)
        {
            if (region.Count < 1) continue;
            Color regionColor = pixelData[region[0].x, region[0].y];

            if (regionColor.a < transparency) continue;

            if (!mergedBorders.ContainsKey(regionColor))
            {
                mergedBorders[regionColor] = new List<List<Vector2Int>>();
            }

            mergedBorders[regionColor].Add(region);
        }

        return mergedBorders;
    }
    public static Color[,] GetPixelData(Texture2D map)
    {
        if (map == null)
        {
            print("Map Provided Is Null");
            return null;
        }

        Color[] colorData1D = map.GetPixels();
        int width = map.width;
        int height = map.height;

        Color[,] colorData2D = new Color[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorData2D[x, y] = colorData1D[y * width + x];
            }
        }

        return colorData2D;
    }
    public static Texture2D SetPixelData(Texture2D map, Color[,] pixelData)
    {
		map.filterMode = FilterMode.Point;

		int width = map.width;
        int height = map.height;

        Color[] pixelData1D = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                pixelData1D[y * width + x] = pixelData[x, y];
            }
        }

        map.SetPixels(pixelData1D);
        map.Apply();
        return map;
    }
    public Color[] GetMapColors(Color[,] pixelData)
    {
        List<Color> uniqueColors = new List<Color>();

        for (int i = 0; i < pixelData.GetLength(0); i++)
        {
            for (int j = 0; j < pixelData.GetLength(1); j++)
            {
                if (pixelData[i, j].a < transparency) continue;
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
	public void DrawMapBorder(LineRenderer lineRenderer, List<Vector2Int> mapBorderPoints, Color[,] colorMap)
    {
        List<Vector2> borderPoints = GlobalData.listVector2(mapBorderPoints);

		if (outlineMode == OutlineMode.ConvexHull)
        {
            borderPoints = BorderPointOrdering.GetConvexHull(borderPoints);
        }
        else if (outlineMode == OutlineMode.NearestPoint)
        {
            borderPoints = BorderPointOrdering.SortByNearestPoint(borderPoints);
        }
        else if (outlineMode == OutlineMode.Polygon)
        {
            borderPoints = BorderPointOrdering.OrderBorderPoints(borderPoints);
        }
        else if (outlineMode == OutlineMode.MarchingSquares)
        {
            borderPoints = GlobalData.listVector2(BorderPointOrdering.MarchingSquares(GlobalData.listVector2Int(borderPoints), colorMap));
        }

		Vector3[] positions = new Vector3[borderPoints.Count];
		for (int i = 0; i < borderPoints.Count; i++)
		{
			positions[i] = GlobalData.vector3(borderPoints[i]) * scale + transform.position;
		}

        if (!onlySetCollision)
        {
            lineRenderer.positionCount = positions.Length;
            lineRenderer.SetPositions(positions);
        }
        SetBorderCollision(borderPoints, lineRenderer.gameObject);
	}
    public Dictionary<Nation, List<LineRenderer>> GetLineRenderers(Dictionary<Nation, int> creationCount)
    {
        Nation[] nations = BattleManager.GetAllNations();
		Dictionary<Nation, List<LineRenderer>> renderers = new();
        foreach (Nation nation in nations)
        {
            renderers.Add(nation, new(nation.GetComponentsInChildren<LineRenderer>()));
        }

        foreach (Nation nation in nations)
        {
            for (int i = 0; i < creationCount[nation]; i++)
            {
                LineRenderer lineRenderer;
                if (renderers[nation].Count <= i)
                {
                    GameObject renderParent = new GameObject("Border");
                    lineRenderer = renderParent.AddComponent<LineRenderer>();
                    renderParent.transform.parent = nation.transform;
                    renderers[nation].Add(lineRenderer);
                }
                else lineRenderer = renderers[nation][i];

				lineRenderer.material = borderMaterial;
				lineRenderer.loop = true;
				lineRenderer.startWidth = lineWidth; lineRenderer.endWidth = lineWidth;
				Color showColor = nation.nationColor; if (showColor.a > transparency) showColor.a = transparency;
				lineRenderer.startColor = showColor; lineRenderer.endColor = showColor;
			}
        }

        return renderers;
    }

    [ContextMenu("Remove Borders")]
    public void RemoveBorders()
    {
        Border[] borders = GetAllBorders();
        LineRenderer[] lineRenderers = new LineRenderer[borders.Length];
        for (int i = 0; i < borders.Length; i++) { lineRenderers[i] = borders[i].GetComponent<LineRenderer>(); }

        for (int i = 0; i < lineRenderers.Length; i++)
        {
            lineRenderers[i].positionCount = 0;
        }
    }
    public void SetBorderCollision(List<Vector2> borderPoints, GameObject parentObject)
    {
		PolygonCollider2D collider = parentObject.GetComponent<PolygonCollider2D>();
        if (collider == null) collider = parentObject.AddComponent<PolygonCollider2D>();
        collider.isTrigger = colliderIsTriger;
        for (int i = 0; i < borderPoints.Count; i++) {
            borderPoints[i] = borderPoints[i] * colliderScale + colliderOffset; 
        }
        collider.points = borderPoints.ToArray();
        collider.includeLayers = collisionIncludeLayers;
        collider.excludeLayers = collisionExcludeLayers;

        Border border = parentObject.GetComponent<Border>();
        if (border == null) parentObject.AddComponent<Border>();
    }
	public static Color[,] ChangeColorOwnership(Color[,] colorMap, Color targetColor,  Color changeColor, Vector2Int coordinate, float radius)
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
    public void ChangeColorOwnership(Color targetColor, Color changeColor, Vector2Int coordinate, float radius)
    {
        Color[,] pixelData = GetPixelData(map);
        pixelData = ChangeColorOwnership(pixelData, targetColor, changeColor, coordinate, radius);
        map = SetPixelData(map, pixelData);

        miniMapImage.texture = map;
        DrawAllBorders();
    }
	public void SetColliderTrigerStatus(bool status)
    {
        colliderIsTriger = status;
        SetColliderTrigerStatus();
    }
    public void SetColliderTrigerStatus()
    {
		Border[] borders = GetAllBorders();
		PolygonCollider2D[] colliders = new PolygonCollider2D[borders.Length];
		for (int i = 0; i < borders.Length; i++) { colliders[i] = borders[i].GetComponent<PolygonCollider2D>(); colliders[i].isTrigger = colliderIsTriger; }
    }
    private void Start()
	{
        miniMapImage.texture = map;
	}
    public static Color RGB(Color c) { c.a = 1; return c; }

    public static Border[] GetAllBorders()
    {
        return FindObjectsByType<Border>(FindObjectsSortMode.None);

	}
}


