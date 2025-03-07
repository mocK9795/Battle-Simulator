using UnityEngine;

public class Capital : WarObject
{
	[Header("Capital Settings")]
	public float controllRadius;
	public float healRate;
	MapRenderer mapRenderer;
	BattleManager battleManager;

	public bool underSeige = false;

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
		base.Start();
		mapRenderer = FindFirstObjectByType<MapRenderer>();
		battleManager =  FindFirstObjectByType<BattleManager>();
		CircleCollider2D[] sphere = GetComponents<CircleCollider2D>();
		float maxRadius = float.MinValue;
		foreach (CircleCollider2D c in sphere) {maxRadius = Mathf.Max(maxRadius, c.radius); }
		if (sphere.Length > 0) controllRadius = maxRadius * ((transform.localScale.x + transform.localScale.y) / 2); 
	}

	private new void Update()
	{
		base.Update();
		health = Mathf.Min(health + healRate * Time.deltaTime, maxHealth);
	}

	public new void Capture(string captureNation)
	{
		if (health > 0) return;
		Nation country = BattleManager.GetNation(nation);
		Color nationColor = country.nationColor;
		Nation enemyCountry = BattleManager.GetNation(captureNation);
		Color enemyColor = enemyCountry.nationColor;

		mapRenderer.CapitalChange(nationColor, enemyColor, mapRenderer.MapPosition(transform.position), controllRadius);
		base.Capture(captureNation);
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.collider == null) return;
		WarObject enemy = collision.collider.GetComponent<WarObject>();
		if (enemy == null) return;
		if (enemy.nation == nation) return;
		EnterSeige();
	}

	private void OnCollisionExit2D(Collision2D collision)
	{
		if (collision.collider == null) return;
		WarObject enemy = collision.collider.GetComponent<WarObject>();
		if (enemy == null) return;
		if (enemy.nation == nation) return;
		ExitSeige();
	}

	void EnterSeige()
	{
		underSeige = true;
	}

	void ExitSeige()
	{
		underSeige = false;
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
