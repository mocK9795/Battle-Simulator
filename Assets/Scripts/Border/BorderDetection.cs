using UnityEngine;
using System.Collections.Generic;

public static class BorderDetection
{
	public static List<List<Vector2Int>> GetBorderPoints(Texture2D map)
	{
		Color[,] pixelData = MapBorderRenderer.GetPixelData(map);
		List<List<Vector2Int>> regions = GetRegions(pixelData);
		List<List<Vector2Int>> borders = GetBordersForRegions(regions, pixelData);

		return borders;
	}

	public static List<List<Vector2Int>> GetRegions(Color[,] pixelData)
	{
		int width = pixelData.GetLength(0);
		int height = pixelData.GetLength(1);
		bool[,] visited = new bool[width, height];
		List<List<Vector2Int>> regions = new List<List<Vector2Int>>();

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				if (!visited[x, y])
				{
					Color targetColor = pixelData[x, y];
					List<Vector2Int> region = new List<Vector2Int>();
					FloodFill(pixelData, visited, x, y, targetColor, region);
					regions.Add(region);
				}
			}
		}

		return regions;
	}

	static void FloodFill(Color[,] pixelData, bool[,] visited, int x, int y, Color targetColor, List<Vector2Int> region)
	{
		int width = pixelData.GetLength(0);
		int height = pixelData.GetLength(1);
		Stack<Vector2Int> stack = new Stack<Vector2Int>();
		stack.Push(new Vector2Int(x, y));

		while (stack.Count > 0)
		{
			Vector2Int current = stack.Pop();
			int cx = current.x;
			int cy = current.y;

			if (cx < 0 || cx >= width || cy < 0 || cy >= height)
				continue;

			if (visited[cx, cy] || pixelData[cx, cy] != targetColor)
				continue;

			visited[cx, cy] = true;
			region.Add(current);

			stack.Push(new Vector2Int(cx + 1, cy));
			stack.Push(new Vector2Int(cx - 1, cy));
			stack.Push(new Vector2Int(cx, cy + 1));
			stack.Push(new Vector2Int(cx, cy - 1));
		}
	}

	public static List<List<Vector2Int>> GetBordersForRegions(List<List<Vector2Int>> regions, Color[,] pixelData)
	{
		List<List<Vector2Int>> borders = new List<List<Vector2Int>>();

		foreach (var region in regions)
		{
			bool[,] regionMask = new bool[pixelData.GetLength(0), pixelData.GetLength(1)];

			foreach (var point in region)
			{
				regionMask[point.x, point.y] = true;
			}

			List<Vector2Int> borderPoints = ApplySobelEdgeDetectionToRegion(regionMask, pixelData);
			borders.Add(borderPoints);
		}

		return borders;
	}

	public static List<Vector2Int> ApplySobelEdgeDetectionToRegion(bool[,] regionMask, Color[,] pixelData)
	{
		int width = pixelData.GetLength(0);
		int height = pixelData.GetLength(1);
		List<Vector2Int> edges = new List<Vector2Int>();

		int[,] gx = new int[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
		int[,] gy = new int[,] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };

		for (int y = 1; y < height - 1; y++)
		{
			for (int x = 1; x < width - 1; x++)
			{
				if (!regionMask[x, y])
					continue;

				float pixelX = 0.0f;
				float pixelY = 0.0f;

				for (int i = -1; i <= 1; i++)
				{
					for (int j = -1; j <= 1; j++)
					{
						Color color = pixelData[x + i, y + j];
						float intensity = color.grayscale;
						pixelX += intensity * gx[i + 1, j + 1];
						pixelY += intensity * gy[i + 1, j + 1];
					}
				}

				float magnitude = Mathf.Sqrt(pixelX * pixelX + pixelY * pixelY);
				if (magnitude > 0.5f)
				{
					edges.Add(new Vector2Int(x, y));
				}
			}
		}

		return edges;
	}

	public static List<Vector2Int> MarchingSquares(Color[,] colorMap, Color target)
	{
		int width = colorMap.GetLength(0);
		int height = colorMap.GetLength(1);

		List<Vector2Int> border = new();
		Vector2Int current = new Vector2Int(-1, -1);
		bool found = false;
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				if (colorMap[x, y] == target)
				{
					border.Add(new(x, y));
					current = new(x, y);
					found = true;
					break;
				}
			}
			if (found) break;
		}

		return MarchingSquares(colorMap, current);
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
	
	public static bool IsBorderPixel(Vector2Int xy, Color[,] map) {return IsBorderPixel(xy.x, xy.y, map); }

	public static List<Vector2Int> MarchingSquares(Color[,] colorMap, Vector2Int current)
	{
		List<Vector2Int> border = new();
		Color target = colorMap[current.x, current.y];

		while (true)
		{
			Vector2Int topLeft = new(current.x - 1, current.y + 1);
			Vector2Int topRight = new(current.x + 1, current.y + 1);
			Vector2Int bottomLeft = new(current.x - 1, current.y - 1);
			Vector2Int bottomRight = new(current.x + 1, current.y - 1);

			Vector2Int left = new(current.x - 1, current.y);
			Vector2Int right = new(current.x + 1, current.y);
			Vector2Int top = new(current.x, current.y + 1);
			Vector2Int bottom = new(current.x, current.y - 1);

			if (!border.Contains(left) && colorMap[left.x, left.y] == target && IsBorderPixel(left, colorMap) ) current = left;
			else if (!border.Contains(right) && colorMap[right.x, right.y] == target && IsBorderPixel(right, colorMap) ) current = right;
			else if (!border.Contains(top) && colorMap[top.x, top.y] == target && IsBorderPixel(top, colorMap) ) current = top;
			else if (!border.Contains(bottom) && colorMap[bottom.x, bottom.y] == target && IsBorderPixel(bottom, colorMap) ) current = bottom;

			else if (!border.Contains(topLeft) && colorMap[topLeft.x, topLeft.y] == target && IsBorderPixel(topLeft, colorMap) ) current = topLeft;
			else if (!border.Contains(bottomLeft) && colorMap[bottomLeft.x, bottomLeft.y] == target && IsBorderPixel(bottomLeft, colorMap) ) current = bottomLeft;
			else if (!border.Contains(topRight) && colorMap[topRight.x, topRight.y] == target && IsBorderPixel(topRight, colorMap) ) current = topRight;
			else if (!border.Contains(bottomRight) && colorMap[bottomRight.x, bottomRight.y] == target && IsBorderPixel(bottomRight, colorMap)) current = bottomRight;

			else break;
			border.Add(current);
		}

		return border;
	}

	public static bool WillEndChain(Vector2Int current, Color[,] colorMap, Color target, List<Vector2Int> border)
	{
		Vector2Int topLeft = new(current.x - 1, current.y + 1);
		Vector2Int topRight = new(current.x + 1, current.y + 1);
		Vector2Int bottomLeft = new(current.x - 1, current.y - 1);
		Vector2Int bottomRight = new(current.x + 1, current.y - 1);

		Vector2Int left = new(current.x - 1, current.y);
		Vector2Int right = new(current.x + 1, current.y);
		Vector2Int top = new(current.x, current.y + 1);
		Vector2Int bottom = new(current.x, current.y - 1);

		if (!border.Contains(left) && colorMap[left.x, left.y] == target && IsBorderPixel(left, colorMap)) return false;
		else if (!border.Contains(right) && colorMap[right.x, right.y] == target && IsBorderPixel(right, colorMap)) return false;
		else if (!border.Contains(top) && colorMap[top.x, top.y] == target && IsBorderPixel(top, colorMap)) return false;
		else if (!border.Contains(bottom) && colorMap[bottom.x, bottom.y] == target && IsBorderPixel(bottom, colorMap)) return false;

		else if (!border.Contains(topLeft) && colorMap[topLeft.x, topLeft.y] == target && IsBorderPixel(topLeft, colorMap)) return false;
		else if (!border.Contains(bottomLeft) && colorMap[bottomLeft.x, bottomLeft.y] == target && IsBorderPixel(bottomLeft, colorMap)) return false;
		else if (!border.Contains(topRight) && colorMap[topRight.x, topRight.y] == target && IsBorderPixel(topRight, colorMap)) return false;
		else if (!border.Contains(bottomRight) && colorMap[bottomRight.x, bottomRight.y] == target && IsBorderPixel(bottomRight, colorMap)) return false;
		
		else return true;
	}
}
