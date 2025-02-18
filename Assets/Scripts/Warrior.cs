using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Warrior : WarObject
{
	public float speed;

	public Vector2 target;
	Vector3 maxScale;
	bool hasAttacked = false;
	Rigidbody2D body;

	Vector2 position { get { return new Vector2(transform.position.x, transform.position.y); } }
	Vector2 forward { get { return new Vector2(transform.right.x, transform.right.y); } }
	float targetAngle;
	[HideInInspector()] public bool isAttacking = false;
	[HideInInspector()] public bool useAi = true;

	public new void Start()
	{
		base.Start();
		SetBody();
		Rescale();

		target = transform.position;
	}

	void SetBody()
	{
		body = GetComponent<Rigidbody2D>(); body.gravityScale = 0; body.linearDamping = GlobalData.friction;
	}

	public new void Update()
	{
		base.Update();
		UpdateTargetAngle();
		RotateTowardsTarget();
		hasAttacked = false;

		if (Vector2.Distance(position, target) > GlobalData.moveAccuracy)
		{
			body.linearVelocity += speed * forward;
		}


		body.linearVelocity = Vector2.ClampMagnitude(body.linearVelocity, speed);
		SetScale();

		if (useAi)
		{
			isAttacking = false;
			AttackEnemy();
		}
	}

	public void AttackEnemy()
	{
		var enemy = GetNearestEnemy();
		if (enemy == null) return;
		isAttacking = true;
		SetTarget(enemy.transform.position);
	}

	public Warrior GetNearestEnemy()
	{
		var nearbyObjects = Physics2D.OverlapCircleAll(transform.position, GlobalData.aiWarriorAttackRange);
		foreach (var obj in nearbyObjects)
		{
			if (obj.gameObject.GetInstanceID() == gameObject.GetInstanceID()) continue;
			Warrior warrior = obj.GetComponent<Warrior>();
			if (warrior == null) continue;
			if (warrior.nation == nation) continue;
			return warrior;
		}

		return null;
	}

	public void SetTargetFromOffset(Vector2 offset)
	{
		target = position + offset;
		UpdateTargetAngle();
	}

	public void SetTarget(Vector2 target)
	{
		this.target = target;
		UpdateTargetAngle();
	}

	public void UpdateTargetAngle()
	{
		targetAngle = GlobalData.Angle(transform.position, target);
	}

	public void RotateTowardsTarget()
	{
		transform.rotation = Quaternion.Euler(0, 0, targetAngle);
	}

	[ContextMenu("Print Data")]
	public void PrintData()
	{
		print(forward);
		print(position);
		print(targetAngle);
	}

	public void OnCollisionStay2D(Collision2D collision)
	{
		if (hasAttacked) return;
		WarObject enemy = Enemy(collision.gameObject);
		if (enemy == null) return;

		enemy.health -= damage * GlobalData.damageScale * Time.deltaTime;
		body.linearVelocity -= speed * forward * GlobalData.knockbackRatio;
		
		target = Vector2.Lerp(target, GlobalData.vector3(enemy.transform.position), Mathf.Clamp01(Time.deltaTime));
		hasAttacked = true;
	}

	public WarObject Enemy(Object obj)
	{
		if (obj == null) return null;
		WarObject enemy = obj.GetComponent<WarObject>();
		if (enemy == null) return null;
		if (enemy.nation == nation) return null;
		return enemy;
	}

	[ContextMenu("Set Scale")]
	public void Rescale()
	{
		maxScale = transform.localScale;
		maxScale = Vector2.ClampMagnitude(maxScale, 1);
		SetScale();
	}

	public void SetScale()
	{
		transform.localScale = maxScale * Mathf.Max(health / GlobalData.healthToScaleRatio, GlobalData.minScale);
	}

	private void OnValidate()
	{
		Rescale();
	}
}
