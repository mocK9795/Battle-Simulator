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
	float currentAngle;
	[HideInInspector()] public bool rescale = true;
	[HideInInspector()] public bool useAi = true;
	[HideInInspector()] public bool isAttacking = false;

	public new void Start()
	{
		base.Start();
		body = GetComponent<Rigidbody2D>();
		body.gravityScale = 0;
		body.linearDamping = GlobalData.friction;

		target = transform.position;

		if (rescale)
		{
			Rescale();
		}
	}

	public new void Update()
	{
		base.Update();
		UpdateTargetAngle();
		RotateTowardsTarget();
		if (Vector2.Distance(position, target) > GlobalData.moveAccuracy)
		{
			body.linearVelocity += speed * forward;
		}

		hasAttacked = false;

		body.linearVelocity = Vector2.ClampMagnitude(body.linearVelocity, speed);
		if (rescale)
		{
			SetScale();
		}

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
		float x = target.x - position.x;
		float y = target.y - position.y;
		targetAngle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
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
		print(currentAngle);
	}

	public void OnCollisionStay2D(Collision2D collision)
	{
		if (hasAttacked) return;
		if (collision.collider == null) return;
		WarObject enemy = collision.collider.GetComponent<WarObject>();
		if (enemy == null) return;
		if (enemy.nation == nation) return;

		enemy.health -= damage * Time.deltaTime;
		hasAttacked = true;
		body.linearVelocity -= speed * forward * GlobalData.knockbackRatio;
		target = Vector2.Lerp(target, GlobalData.vector3(enemy.transform.position), Mathf.Clamp01(Time.deltaTime));
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
