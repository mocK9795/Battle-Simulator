using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public GameObject lineRendererContainer;
    public enum ContructionDisplay {LineOverlay, Model}
    public ContructionDisplay display;
    [Tooltip("Size of one pixel on the map")] public Vector2 stateSize;
    public float lines;
    public float lineWidth;
    public float lineZ;
    public Color lineColor;
    public Transform modelContainer;

	[Space(1)]
    public float contructionTimeValue;
    public float contructionValueValue;
    public string symbol;

    [Space(1)]
    public MapRenderer mapRenderer;
    public Texture2D populationTexture;
    public float populationTextureScale;
    public float grantSpeed;
    float grantTimer;
    [HideInInspector]
    public float[,] populationMap;
    public List<Site> contructionSites = new();
    [HideInInspector] public bool populationMapChanged;
    List<LineRenderer> lineRenderers;
	
    private void Start()
	{
        SetPopulationMap();
        UpdatePopulation();
        GrantPoliticalPower();

        lineRenderers = new();
	}

    private void Update()
    {
        grantTimer += Time.deltaTime;
        if (grantTimer > grantSpeed) {
            //UpdatePopulation(); 
            GrantPoliticalPower();
            GrantWealth();

            grantTimer = 0;
        }


        foreach (var line in lineRenderers) { line.positionCount = 0;}

        if (display == ContructionDisplay.LineOverlay) DrawContructionSites();
        ContructSites();
    }

    public void GrantWealth()
    {
		var factories = BattleManager.GetAll<Factory>();
		foreach (var factory in factories)
		{
			BattleManager.GetNation(factory.nation).wealth += factory.TakeOuput() * GlobalData.factoryOutputValue;
		}
		foreach (var nation in BattleManager.GetAllNations())
		{
			nation.wealth += GlobalData.baseWealthGain;
		}
	}

	public static Site NormalizeSiteData(Site site)
	{
		site.efficency = Mathf.Max(site.efficency, 0.1f);
		site.capacity = Mathf.Max(site.capacity, 3f);
		return site;
	}
	public void ContructSite(Site site, Nation nation, Vector2Int position)
    {
        var realPositon = mapRenderer.MapPositionViaDisplacement(position);
        if (mapRenderer.mapData[realPositon.x, realPositon.y] != nation.nationColor) return;
        if (BattleManager.GetNearest<Structure>(mapRenderer.WorldPosition(realPositon), null, GlobalData.structureSpacing) != null) return;
        site = NormalizeSiteData(site);
        var cost = SiteCost(site);
        if (cost > nation.wealth) return;
        nation.wealth -= cost;
        contructionSites.Add(site);

        if (display == ContructionDisplay.Model){ 
            var model = SpawnObject<Structure>(
                mapRenderer.WorldPosition(realPositon), 
                WarObject.ModelType.ContructionSite, 
                new(nation.nation, GlobalData.capitalChangeHealth, 0));
            model.applyColorOnModel = false;
            site.model = model.gameObject;
		}
    }
    public float SiteCost(Site site) 
    {
        return (site.efficency * contructionValueValue) + (site.capacity * contructionTimeValue);
    }   
    void ContructSites()
    {
        var completions = new List<Site>();
        foreach (var site in contructionSites)
        {
            site.completion += Time.deltaTime;
            if (site.completion > site.capacity)
            {
                FinishSite(site);
				completions.Add(site);
			}
		}
        foreach (var site in completions) { contructionSites.Remove(site); }
    }

    void FinishSite(Site site)
    {
        var pos = mapRenderer.MapPositionViaDisplacement(site.position);
		Nation parentNation = BattleManager.GetNation(mapRenderer.mapData[pos.x, pos.y]);
        var factory = SpawnObject<Factory>(mapRenderer.WorldPosition(pos), WarObject.ModelType.Factory, new(parentNation.nation, GlobalData.capitalChangeHealth, 0));
        factory.efficiency = site.efficency;
        factory.health = site.capacity;
        Destroy(site.model);
    }

    public static T SpawnObject<T>(Vector3 position, WarObject.ModelType model, WarObjectData objectData) where T : WarObject
    {
        var warObjParent = new GameObject(model.ToString());
        var warObject = warObjParent.AddComponent<T>();
        warObjParent.AddComponent<SpriteRenderer>();
        warObjParent.AddComponent<BoxCollider2D>();
        warObjParent.transform.position = position;
        warObject.AssignData(objectData);
        warObject.modelType = model;
        return warObject;
    }
	public static Warrior SpawnWarrior(Vector3 position, WarObject.ModelType model, WarriorData data) 
	{
        var warrior = SpawnObject<Warrior>(position, model, data);
        warrior.AssignData(data);
		return warrior;
	}
	public void PopulateArea(Vector2Int position, float value)
    {
        populationMap[position.x, position.y] += value;
        populationMapChanged = true;
	}
    public float AverageDevlopment()
    { 
        return TotalDevelopment() / populationMap.LongLength;
    }
    public float TotalDevelopment()
    {
        float sum = 0;
        foreach (var devlopment in populationMap) { sum += devlopment; }
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

	[ContextMenu("Create Population Map")]
    public void SetPopulationMap()
    {
        Color[,] colorMap = MapBorderRenderer.GetPixelData(populationTexture);
        populationMap = ConvertMap(colorMap, populationTextureScale);
        populationMapChanged = true;
    }

    public void UpdatePopulation()
    {
        var nations = BattleManager.GetAllNations();

        int width = mapRenderer.mapData.GetLength(0);
        int height = mapRenderer.mapData.GetLength(1);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color color = mapRenderer.mapData[x, y];
                foreach (Nation nation in nations)
                {
                    if (nation.nationColor == color) nation.manPower += populationMap[x, y];
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
    public float efficency;
    public float capacity;
    public float completion;
    public GameObject model = null;

    public Site Copy()
    {
        Site site = new Site();
        site.position = position;
        site.efficency = efficency;
        site.capacity = capacity;
        site.completion = completion;
        return site;
    }
}