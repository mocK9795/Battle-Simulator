using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class BorderPointOrdering : MonoBehaviour
{
	public float maxPointDist = 2;
	public static float maxPointDistance = 2;

	public static List<Vector2Int> MarchingSquares(List<Vector2Int> points, Color[,] colorMap)
	{
		return BorderDetection.MarchingSquares(colorMap, points[0]);
	}
	public static List<Vector2Int> OrderBorderPoints(List<Vector2Int> borderPoints)
	{
		List<Vector2Int> convexHull = GetConvexHull(borderPoints);
		var orderedPoint = ReconstructPolygon(borderPoints, convexHull); orderedPoint.RemoveAt(orderedPoint.Count - 1);
		return orderedPoint;
	}
	public static List<Vector2> OrderBorderPoints(List<Vector2> borderPoints)
	{
		return GlobalData.listVector2(OrderBorderPoints(GlobalData.listVector2Int(borderPoints)));
	}
	public static List<Vector2Int> ReconstructPolygon(List<Vector2Int> borderPoints, List<Vector2Int> convexHull)
	{
		HashSet<Vector2Int> hullSet = new HashSet<Vector2Int>(convexHull);
		List<Vector2Int> orderedPoints = new List<Vector2Int>(convexHull);

		Stack<Vector2Int> stack = new Stack<Vector2Int>();
		Vector2Int startPoint = convexHull[0];
		stack.Push(startPoint);
		hullSet.Remove(startPoint);

		while (stack.Count > 0)
		{
			Vector2Int currentPoint = stack.Pop();
			List<Vector2Int> neighbors = GetNeighbors(currentPoint, borderPoints);

			foreach (Vector2Int neighbor in neighbors)
			{
				if (!hullSet.Contains(neighbor))
				{
					orderedPoints.Add(neighbor);
					stack.Push(neighbor);
					hullSet.Add(neighbor);
				}
			}
		}

		return orderedPoints;
	}
	public static List<Vector2Int> GetNeighbors(Vector2Int point, List<Vector2Int> borderPoints)
	{
		List<Vector2Int> neighbors = new List<Vector2Int>();
		foreach (Vector2Int p in borderPoints)
		{
			if (Vector2Int.Distance(point, p) == 1)
			{
				neighbors.Add(p);
			}
		}
		return neighbors;
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
			print(Vector2.Distance(nearestPoint, currentPoint));
			if (Vector2.Distance(nearestPoint, currentPoint) > maxPointDistance) break;
			sortedPoints.Add(nearestPoint);
			points.Remove(nearestPoint);
			currentPoint = nearestPoint;
		}

		return sortedPoints;
	}
	public static Vector3[] OptimizePath(Vector3[] path)
	{
		if (path.Length == 0)
		{
			print("Null Path"); return new Vector3[0];
		}
		Vector3 endPoint = path[path.Length - 1];
		List<Vector3> optimizedPath = new List<Vector3>();
		Vector3 lastAddedPoint = path[0];

		for (int i = 0; i < path.Length; i++)
		{
			if (i == path.Length - 1)
			{
				optimizedPath.Add(path[i]);
				lastAddedPoint = path[i];
				continue;
			}
			if (i == 0)
			{
				optimizedPath.Add(path[i]);
				lastAddedPoint = path[i];
				continue;
			}
			if (Vector3.Distance(lastAddedPoint, endPoint) < Vector3.Distance(path[i], endPoint)) continue;
			optimizedPath.Add(path[i]);
			lastAddedPoint = path[i];
		}

		return optimizedPath.ToArray();
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
	private void OnValidate()
	{
		maxPointDistance = maxPointDist;
	}
}

