using Unity.Collections;
using UnityEngine;

public abstract class IShip : MonoBehaviour
{
	// ----- Essentielle variabler ----- \\

	private GameManager gameManager = null;
	[SerializeField] private Rigidbody rigid = null;

	// ----- Generelle variabler ----- \\

	protected int shipID = -1;

	[SerializeField] protected int shipHealth = 100;
	[SerializeField] protected float shipFuel = 100.0f;
	[SerializeField] private int shipCannonDamage = 15;
	[SerializeField] private float shipCannonNextFire = 0.5f;

	[SerializeField] protected Cannon leftCannon = null;
	[SerializeField] protected Cannon rightCannon = null;

	protected float moveSpeed = 8.0f;
	protected float moveSpeedMax = 8.0f;
	protected float rotationSpeed = 90.0f;

	private bool networkUpdated = false;

	// ----- API funktioner ----- \\

	///<summary>Bruges til at håndtere spiller input og andre ting</summary>
	protected abstract void ProcessShip();
	
	///<summary>Bruges til at lave physics beregninger på vores skib</summary>
	protected abstract void ProcessShipFixed();
	
	///<summary>Bliver kaldt når vi rammer sammen med et andet skib</summary>
	protected abstract void OnShipCollision();

	///<summary>Bliver kaldt når vi tager skade</summary>
	protected abstract void OnDamageTaken(int damage);

	///<summary>Bliver kaldt når vores skib dør</summary>
	protected abstract void OnShipDestroyed();

	// ----- Engine funktioner ----- \\

	private void OnCollisionEnter(Collision collision)
	{
		ShipCollisionHandler(collision);
	}

	// ----- Custom funktioner ----- \\

	///<summary>Bliver kaldt når vores skib bliver oprettet</summary>
	public void ShipCreatedHandler(GameManager gm, bool physics, int ID)
	{
		gameManager = gm;
		shipID = ID;

		if(physics == false)
        {
			Destroy(rigid);
		}
	}

	///<summary>Bliver kaldt hver frame</summary>
	public void OnUpdate()
	{
		leftCannon.OnUpdate();
		rightCannon.OnUpdate();

		ProcessShip();

		transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);
	}

	///<summary>Bliver kaldt consistently hver physics update</summary>
	public void OnFixedUpdate()
	{
		ProcessShipFixed();

		Utils.ApplyRigidSettings(rigid, moveSpeedMax);
	}

	///<summary>Bliver kaldt når vi rammer sammen med et andet objekt</summary>
	private void ShipCollisionHandler(Collision collision)
    {
		OnShipCollision();

		IShip ship = collision.gameObject.GetComponent<IShip>();

		if (ship != null)
		{
			ship.ApplyDamage(10 * (int)rigid.velocity.magnitude);
		}
	}

	///<summary>Bevæger skibet mod en bestemt retning</summary>
	protected void MoveShip(Vector3 direction)
	{
		if(shipFuel > 0)
        {
			TakeFuel(1.0f * Time.deltaTime);

			rigid.AddForce(direction);
		}
	}

	private void TakeFuel(float fuel)
	{
		shipFuel -= fuel;

		GetGameManager().GetUIManager().GetFuelBar().SetFuel((int)shipFuel);
	}

	///<summary>Giver skader til skibet</summary>
	public void ApplyDamage(int damage)
	{
		shipHealth -= damage;

		GetGameManager().GetUIManager().ShowDamageText(gameObject, damage, 2);

		OnDamageTaken(damage);

		if (shipHealth <= 0)
		{
			GetGameManager().GetUIManager().ShowDamageText(gameObject, damage, 2);
			GetGameManager().GetUIManager().GetHealthBar().SetHealth(shipHealth);

			OnDamageTaken(damage);

			OnShipDestroyed();

			gameManager.OnShipDestroyed(this);
		}
	}

	// ----- API funktioner ----- \\

	public void SetShipHealth(int health, bool applyToUI)
    {
		shipHealth = health;

		if(applyToUI)
        {
			GetGameManager().GetUIManager().GetHealthBar().SetHealth(health);
		}
	}

	public int GetShipHealth()
	{
		return shipHealth;
	}

	public void SetShipFuel(float fuel, bool applyToUI)
	{
		shipFuel = fuel;

		if (applyToUI)
		{
			GetGameManager().GetUIManager().GetFuelBar().SetFuel((int)shipFuel);
		}
	}

	public float GetShipFuel()
	{
		return shipFuel;
	}

	public float GetShipSpeed()
    {
		return rigid.velocity.magnitude;
	}

	public void RotateShip(float speed)
	{
		transform.Rotate(Vector3.up, speed * (GetShipSpeed() / moveSpeedMax) * Time.deltaTime);
	}

	public int GetCannonDamage()
	{
		return shipCannonDamage;
	}

	public float GetCannonNextFire()
	{
		return shipCannonNextFire;
	}

	public int GetShipID()
    {
		return shipID;
    }

	public GameManager GetGameManager()
    {
		return gameManager;
    }

	public void UpdateNetworkState()
	{
		networkUpdated = true;
	}

	public void ResetNetworkState()
    {
		networkUpdated = false;
    }

	public bool GetNetworkState()
	{
		return networkUpdated;
	}
}