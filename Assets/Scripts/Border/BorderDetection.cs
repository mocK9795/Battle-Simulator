using UnityEngine;
using System.Collections.Generic;

public static class BorderDetection
{
	public static List<List<Vector2Int>> GetBorderPoints(Texture2D map)
	{
		Color[,] pixelData = GetPixelData(map);
		List<List<Vector2Int>> regions = GetRegions(pixelData);
		List<List<Vector2Int>> borders = GetBordersForRegions(regions, pixelData);

		return borders;
	}

	public static Color[,] GetPixelData(Texture2D texture)
	{
		Color[] pixels = texture.GetPixels();
		int width = texture.width;
		int height = texture.height;

		Color[,] pixelData = new Color[width, height];

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				pixelData[x, y] = pixels[y * width + x];
			}
		}

		return pixelData;
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
}
