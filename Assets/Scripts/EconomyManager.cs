using UnityEngine;
using System.Collections.Generic;

public class EconomyManager : MonoBehaviour
{
    public GameObject lineRendererContainer;
    [Tooltip("Size of one pixel on the map")] public Vector2 stateSize;
    public float lines;
    public float lineWidth;
    public float lineZ;
    public Color lineColor;

    [Space(1)]
    public float contructionTimeValue;
    public float contructionValueValue;
    public string symbol;

    [Space(1)]
    public MapRenderer mapRenderer;
    public Texture2D devlopmentTexture;
    public float devlopmentTextureScale;
    public float grantSpeed;
    float grantTimer;
    [HideInInspector]
    public float[,] devlopmentMap;
    public List<Site> contructionSites = new();
    [HideInInspector] public bool devlopmentMapChanged;
    [HideInInspector] List<LineRenderer> lineRenderers;

	private void Start()
	{
        SetDevlopmentMap();
        GrantWealth();
        GrantPoliticalPower();

        lineRenderers = new();
	}

    private void Update()
    {
        grantTimer += Time.deltaTime;
        if (grantTimer > grantSpeed) {
            GrantWealth(); GrantPoliticalPower();
            grantTimer = 0; }


        foreach (var line in lineRenderers) { line.positionCount = 0;}

        if (mapRenderer.drawMode == MapRenderer.DrawMode.DevlopmentMap) DrawContructionSites();
        ContructSites();
    }
	public static Site NormalizeSiteData(Site site)
	{
		site.contructionValue = Mathf.Max(site.contructionValue, 0.1f);
		site.contructionTime = Mathf.Max(site.contructionTime, 3f);
		return site;
	}
	public void ContructSite(Site site, Nation nation, Vector2Int position)
    {
        var realPositon = mapRenderer.MapPositionViaDisplacement(position);
        if (mapRenderer.mapData[realPositon.x, realPositon.y] != nation.nationColor) return;
        site = NormalizeSiteData(site);
        var cost = SiteCost(site);
        if (cost > nation.wealth) return;
        nation.wealth -= cost;
        contructionSites.Add(site);
    }
    public float SiteCost(Site site) 
    {
        return (site.contructionValue * contructionValueValue) - (site.contructionTime * contructionTimeValue);
    }   
    void ContructSites()
    {
        var completions = new List<Site>();
        foreach (var site in contructionSites)
        {
            site.completion += Time.deltaTime;
            if (site.completion > site.contructionTime)
            {
                DevlopArea(mapRenderer.MapPositionViaDisplacement(site.position), site.contructionValue);
				completions.Add(site);
			}
		}
        foreach (var site in completions) { contructionSites.Remove(site); }
    }
    public void DevlopArea(Vector2Int position, float value)
    {
        devlopmentMap[position.x, position.y] += value;
        devlopmentMapChanged = true;
	}
    public float AverageDevlopment()
    { 
        return TotalDevelopment() / devlopmentMap.LongLength;
    }
    public float TotalDevelopment()
    {
        float sum = 0;
        foreach (var devlopment in devlopmentMap) { sum += devlopment; }
        return sum;
    }
    void DrawContructionSites()
    {
		int totalLines = -1;
		foreach (var contructionArea in contructionSites)
		{
            Vector2Int site = contructionArea.position;
			float x = site.x * stateSize.x;
			float y = site.y * stateSize.y;
			for (int i = 0; i < lines; i++)
			{
				totalLines += 1;
				if (totalLines > lineRenderers.Count - 1) AddLineRenderer();

				Vector2 posA = new(
					site.x * stateSize.x, y
					);
				Vector2 posB = new(
					x, site.y * stateSize.y
					);

				lineRenderers[totalLines].positionCount = 2;
				lineRenderers[totalLines].SetPosition(0, GlobalData.vector3(posA, lineZ));
				lineRenderers[totalLines].SetPosition(1, GlobalData.vector3(posB, lineZ));

				x += stateSize.x / lines;
				y += stateSize.y / lines;
			}

			x = site.x * stateSize.x;
			y = site.y * stateSize.y;
			for (int i = 0; i < lines; i++)
			{
				totalLines += 1;
				if (totalLines > lineRenderers.Count - 1) AddLineRenderer();

				Vector2 posA = new(
					x, site.y * stateSize.y + stateSize.y
					);
				Vector2 posB = new(
					site.x * stateSize.x + stateSize.x, y
					);

				lineRenderers[totalLines].positionCount = 2;
				lineRenderers[totalLines].SetPosition(0, GlobalData.vector3(posA, lineZ));
				lineRenderers[totalLines].SetPosition(1, GlobalData.vector3(posB, lineZ));

				x += stateSize.x / lines;
				y += stateSize.y / lines;
			}
		}
	}
    void AddLineRenderer()
    {
        var rendererObj = new GameObject("Contruction Line Renderer");
        rendererObj.transform.parent = lineRendererContainer.transform;
        var renderer = rendererObj.AddComponent<LineRenderer>();
        renderer.material = GlobalData.lineMat;
        renderer.startWidth = lineWidth; renderer.endWidth = lineWidth;
        renderer.startColor = lineColor; renderer.endColor = lineColor;
        lineRenderers.Add(renderer);
    }

	[ContextMenu("Create Devlopment Map")]
    public void SetDevlopmentMap()
    {
        Color[,] colorMap = MapBorderRenderer.GetPixelData(devlopmentTexture);
        devlopmentMap = ConvertMap(colorMap, devlopmentTextureScale);
        devlopmentMapChanged = true;
    }

    [ContextMenu("Grant Wealth")]
    public void GrantWealth()
    {
        var nations = BattleManager.GetAllNations();
        Color[,] mapColors = MapBorderRenderer.GetPixelData(mapRenderer.map);

        int width = mapColors.GetLength(0);
        int height = mapColors.GetLength(1);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color color = mapColors[x, y];
                foreach (Nation nation in nations)
                {
                    if (nation.nationColor == color) nation.wealth += devlopmentMap[x, y];
                }
            }
        }
    }

    [ContextMenu("Reset Wealth")]
    public void ResetWealth()
    {
        var nations = BattleManager.GetAllNations();
        foreach (Nation nation in nations) {nation.wealth = 0;}
    }

    [ContextMenu("Grant Political Power")]
    public void GrantPoliticalPower()
    {
        var allNations = BattleManager.GetAllNations();
        foreach (var nation in allNations)
        {
            nation.politicalPower += GlobalData.basePoliticalPowerGain;
        }
    }
    public static float[,] ConvertMap(Color[,] map, float scale)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);
        float[,] valueMap = new float[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                valueMap[x, y] = (map[x, y].r + map[x, y].g + map[x, y].b) / 3 * map[x, y].a * scale;
            }
        }

        return valueMap;
    }
	
    [ContextMenu("Get Average Devlopment")]
	void PrintAverageDevlopment()
	{
		print(AverageDevlopment());
	}

	[ContextMenu("Get Total Development")]
	void PrintTotalDevlopment()
	{
		print(TotalDevelopment());
	}
}

[System.Serializable]
public class Site
{
    public Vector2Int position;
    public float contructionValue;
    public float contructionTime;
    public float completion;

    public Site Copy()
    {
        Site site = new Site();
        site.position = position;
        site.contructionValue = contructionValue;
        site.contructionTime = contructionTime;
        site.completion = completion;
        return site;
    }
}