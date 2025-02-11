using System.Collections.Generic;
using UnityEngine;
public class BorderPointOrdering : MonoBehaviour
{
	public static List<Vector2Int> OrderBorderPoints(List<Vector2Int> borderPoints)
	{
		List<Vector2Int> convexHull = MapBorderRenderer.GetConvexHull(borderPoints);
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
}

