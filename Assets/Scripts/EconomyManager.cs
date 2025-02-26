using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public MapRenderer mapRenderer;
    public Texture2D devlopmentTexture;
    public float devlopmentTextureScale;
    public float grantSpeed;
    float grantTimer;
    [HideInInspector]
    public float[,] devlopmentMap;
    [HideInInspector] public bool devlopmentMapChanged;

	private void Start()
	{
        SetDevlopmentMap();
        GrantWealth();
	}

	private void Update()
	{
        grantTimer += Time.deltaTime;
        if (grantTimer > grantSpeed) {GrantWealth(); grantTimer = 0; }
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
}
