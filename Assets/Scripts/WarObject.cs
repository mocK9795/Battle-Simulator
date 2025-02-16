using UnityEngine;

public class WarObject : MonoBehaviour
{
    [Header("War Statistics")]
    public string nation;
    public float health;
    [HideInInspector()] public float maxHealth;
    public float damage;

	[HideInInspector()]
	public Sprite sprite
	{
		get { return GetComponent<SpriteRenderer>().sprite; }
		set { GetComponent<SpriteRenderer>().sprite = value; }
	}

	[HideInInspector()]
	public SpriteRenderer spriteRenderer
	{
		get { return GetComponent<SpriteRenderer>(); }
	}

	public void Start()
	{
		maxHealth = health;
	}

	public void Update()
	{
		if (health < 0) Destroy(gameObject);
	}
}
