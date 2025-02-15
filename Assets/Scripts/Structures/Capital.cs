using Unity.VisualScripting;
using UnityEngine;

public class Capital : Warrior
{
	[Header("Capital Settings")]
	public float controllRadius;
	public float healRate;
	public enum MapEffects {Border, Map};
	public MapEffects mapEffects = MapEffects.Map;
	MapBorderRenderer borderRenderer;
	MapRenderer mapRenderer;
	BattleManager battleManager;
	Warrior lastAttacker = null;

	public CircleCollider2D GetCollider()
	{
		CircleCollider2D[] spheres = GetComponents<CircleCollider2D>();
		if (spheres.Length == 2) return (spheres[0].radius < spheres[1].radius) ? spheres[0] : spheres[1];
		return null;
	}

	public CircleCollider2D GetController()
	{
		CircleCollider2D[] spheres = GetComponents<CircleCollider2D>();
		if (spheres.Length == 2) return (spheres[0].radius > spheres[1].radius) ? spheres[0] : spheres[1];
		return null;
	}


	private new void Start()
	{
		rescale = false;
		base.Start();
		borderRenderer = FindFirstObjectByType<MapBorderRenderer>();
		mapRenderer = FindFirstObjectByType<MapRenderer>();
		battleManager =  FindFirstObjectByType<BattleManager>();
		CircleCollider2D[] sphere = GetComponents<CircleCollider2D>();
		float maxRadius = float.MinValue;
		foreach (CircleCollider2D c in sphere) {maxRadius = Mathf.Max(maxRadius, c.radius); }
		if (sphere.Length > 0) controllRadius = maxRadius * ((transform.localScale.x + transform.localScale.y) / 2); 
	}

	private new void Update()
	{
		health = Mathf.Min(health + healRate * Time.deltaTime, maxHealth);

		if (health < 0)
		{
			if (lastAttacker == null) { health = GlobalData.capitalChangeHealth; return; }

			Nation country = BattleManager.GetNation(nation);
			Color nationColor = country.nationColor;
			Nation enemyCountry = BattleManager.GetNation(lastAttacker.nation);
			Color enemyColor = enemyCountry.nationColor;

			if (mapEffects == MapEffects.Border)
			{
				borderRenderer.ChangeColorOwnership(nationColor, enemyColor, borderRenderer.worldToMap(transform.position), controllRadius);
			}
			if (mapEffects == MapEffects.Map) 
			{
				print(mapRenderer.MapPosition(transform.position));
				mapRenderer.CapitalChange(nationColor, enemyColor, mapRenderer.MapPosition(transform.position), controllRadius);
			}


			health = GlobalData.capitalChangeHealth;
			nation = lastAttacker.nation;
			lastAttacker = null;
			battleManager.SetWarriorNationData();
		}
	}

	public new void OnCollisionStay2D(Collision2D collision)
	{
		if (collision.collider == null) return;
		Warrior enemy = collision.collider.GetComponent<Warrior>();
		if (enemy == null) return;
		if (enemy.nation == nation) return;
		lastAttacker = enemy;
	}

	[ContextMenu("Print Map Position")]
	public void PrintMapPosition()
	{
		mapRenderer = FindFirstObjectByType<MapRenderer>();
		print(mapRenderer.MapPosition(transform.position));
	}

	private void OnValidate()
	{
		
	}
}
