using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapBorderRenderer : MonoBehaviour
{
    public float scale;
    public float lineWidth;
    [Range(0f, 1f)]
    public float transparency;
    public Material borderMaterial;
    public Texture2D map;
    public enum OutlineMode { ConvexHull, NearestPoint, Default, Polygon};
    public OutlineMode outlineMode;

    public RawImage miniMapImage;

    [ContextMenu("Draw Borders")]
    public void DrawAllBorders()
    {
        RemoveBorders();
        Color[,] pixelData = GetPixelData(map);
        Nation[] nations = BattleManager.GetAllNations();
        var rawBorderPoints = BorderDetection.GetBorderPoints(map);
        var mergedBorderPoints = MergeBordersByColor(rawBorderPoints, pixelData);
        Color[] borderColors = mergedBorderPoints.Keys.ToArray();
        Dictionary<Nation, int> nationLineRendererCounts = new();
        Dictionary<Nation, List<List<Vector2Int>>> nationBorderPoints = new();
        for (int i = 0; i < nations.Length; i++)
        {
            Nation nation = nations[i];
            nation.nationColor = borderColors[i];
            nationBorderPoints.Add(nation, mergedBorderPoints[nation.nationColor]);
            nationLineRendererCounts.Add(nation, nationBorderPoints[nation].Count);
        }
        var lineRenderers = GetLineRenderers(nationLineRendererCounts);

        for (int n = 0; n < nations.Length; n++)
        {
            Nation nation = nations[n];
            print(lineRenderers[nation].Count); print(nationBorderPoints[nation].Count);
            for (int i = 0; i < lineRenderers[nation].Count; i++)
            {
                if (nationBorderPoints[nation].Count <= i) break;
                DrawMapBorder(lineRenderers[nation][i], nationBorderPoints[nation][i]);
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

    public Vector2 mapToWorld(int x, int y) { return new Vector2(x, y) * scale + GlobalData.vector2(transform.position); }
    public Vector2 mapToWorld(Vector2Int xy) { return mapToWorld(xy.x, xy.y);  }
    public Vector2 mapToWorld(Vector2 xy) { return mapToWorld(GlobalData.vector2Int(xy)); }

    public Vector2Int worldToMap(Vector2 position)
    {
        Vector2 rawPosition = position / scale - GlobalData.vector2(transform.position) / scale;
        return new Vector2Int(Mathf.RoundToInt(rawPosition.x), Mathf.RoundToInt(rawPosition.y));
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
    public static bool IsBorderPixel(int x, int y, Color[,] map)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);

        if (x == 0) return true;
        if (y == 0) return true;
        if (x == width - 1) return true;
        if (y == height - 1) return true;

        Color centerColor = map[x, y];
        Color topColor = map[x, y - 1];
        Color bottomColor = map[x, y + 1];
        Color rightColor = map[x + 1, y];
        Color leftColor = map[x - 1, y];

        if (centerColor == topColor && centerColor == bottomColor && centerColor == rightColor && centerColor == leftColor) return false;
        else return true;
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
    public static List<Vector2> GetConvexHull(List<Vector2> points)
    {
        // Sort the points lexicographically (by x, then by y)
        points.Sort((a, b) => a.x == b.x ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));

        // Remove duplicates
        points = new List<Vector2>(new HashSet<Vector2>(points));

        if (points.Count <= 1)
            return points;

        List<Vector2> hull = new List<Vector2>();

        // Build lower hull
        foreach (Vector2 p in points)
        {
            while (hull.Count >= 2 && Cross(hull[hull.Count - 2], hull[hull.Count - 1], p) <= 0)
                hull.RemoveAt(hull.Count - 1);
            hull.Add(p);
        }

        // Build upper hull
        int t = hull.Count + 1;
        for (int i = points.Count - 1; i >= 0; i--)
        {
            Vector2 p = points[i];
            while (hull.Count >= t && Cross(hull[hull.Count - 2], hull[hull.Count - 1], p) <= 0)
                hull.RemoveAt(hull.Count - 1);
            hull.Add(p);
        }

        hull.RemoveAt(hull.Count - 1);

        return hull;
    }
    public static List<Vector2Int> GetConvexHull(List<Vector2Int> points)
    {
        List<Vector2> nonIntPoints = GlobalData.listVector2(points);
        var nonIntHull = GetConvexHull(nonIntPoints);
        return GlobalData.listVector2Int(nonIntHull);
    }

    private static float Cross(Vector2 o, Vector2 a, Vector2 b)
	{
		return (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);
	}
	public static List<Vector2> SortByNearestPoint(List<Vector2> points)
	{
		if (points == null || points.Count == 0)
			return points;

		List<Vector2> sortedPoints = new List<Vector2>();
		Vector2 currentPoint = points[0];
		sortedPoints.Add(currentPoint);
		points.RemoveAt(0);

		while (points.Count > 0)
		{
			Vector2 nearestPoint = points.OrderBy(p => Vector2.Distance(currentPoint, p)).First();
			sortedPoints.Add(nearestPoint);
			points.Remove(nearestPoint);
			currentPoint = nearestPoint;
		}

		return sortedPoints;
	}

	public void DrawMapBorder(LineRenderer lineRenderer, List<Vector2Int> mapBorderPoints)
    {
        List<Vector2> borderPoints = GlobalData.listVector2(mapBorderPoints);

		if (outlineMode == OutlineMode.ConvexHull)
        {
            borderPoints = GetConvexHull(borderPoints);
        }
        else if (outlineMode == OutlineMode.NearestPoint)
        {
            borderPoints = SortByNearestPoint(borderPoints);
        }
        else if (outlineMode == OutlineMode.Polygon)
        {
            borderPoints = BorderPointOrdering.OrderBorderPoints(borderPoints);
        }

		Vector3[] positions = new Vector3[borderPoints.Count];
		for (int i = 0; i < borderPoints.Count; i++)
		{
			positions[i] = GlobalData.vector3(mapToWorld( borderPoints[i]  ));
		}

		lineRenderer.positionCount = positions.Length;
		lineRenderer.SetPositions(positions);
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
        LineRenderer[] lineRenderers = GetComponentsInChildren<LineRenderer>();

        for (int i = 0; i < lineRenderers.Length; i++)
        {
            lineRenderers[i].positionCount = 0;
        }
    }
    public void SetBorderCollision(List<Vector2> borderPoints, GameObject parentObject)
    {
		PolygonCollider2D collider = parentObject.GetComponent<PolygonCollider2D>();
        if (collider == null) collider = parentObject.AddComponent<PolygonCollider2D>();
        collider.isTrigger = true;
        collider.points = borderPoints.ToArray();

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
	private void Start()
	{
        miniMapImage.texture = map;
	}

    public static Color RGB(Color c) { c.a = 1; return c; }
}


