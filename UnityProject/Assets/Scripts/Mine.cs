using UnityEngine;

public class Mine : MonoBehaviour
{
    // ----- Essentielle variabler ----- \\

    private GameManager gameManager = null;
    [SerializeField] private Rigidbody rigid = null;

    private int mineID = -1;

    private bool networkUpdated = false;

    // ----- Generelle variabler ----- \\

    private int mineDamage = 20;

    // ----- Engine funktioner ----- \\

    private void OnCollisionEnter(Collision collision)
    {
        MineCollisionHandler(collision);
    }

    // ----- Custom funktioner ----- \\

    ///<summary>Bliver kaldt når en mine bliver oprettet</summary>
	public void MineCreatedHandler(GameManager gm, bool isMaster, int ID)
    {
        gameManager = gm;

        mineID = ID;

        if(isMaster == false)
        {
            Destroy(rigid);
        }
    }

    ///<summary>Bliver kaldt når vi ødelægger en mine</summary>
    public void MineDestroyed()
    {
        gameManager.OnMineDestroyed(this);
    }

    ///<summary>Bliver kaldt når vi rammer sammen med et andet objekt</summary>
    private void MineCollisionHandler(Collision collision)
    {
        IShip ship = collision.gameObject.GetComponent<IShip>();

        if (ship != null)
        {
            ship.ApplyDamage(mineDamage);

            MineDestroyed();
        }
    }

    // ----- API funktioner ----- \\

    public int GetMineID()
    {
        return mineID;
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
