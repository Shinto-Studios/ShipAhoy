using JetBrains.Annotations;
using UnityEngine;

public class CannonBall : MonoBehaviour
{
    // ----- Essentielle variabler ----- \\

    [SerializeField] private Rigidbody rigid = null;
    private IShip firedFromShip = null;

    private int cannonBallID = -1;

    private bool networkMaster = false;
    private bool networkUpdated = false;

    // ----- Generelle variabler ----- \\
    [SerializeField] private float speed = 20f;

    // ----- Engine funktioner ----- \\

    private void OnCollisionEnter(Collision collision)
    {
        if(networkMaster == true)
        {
            IShip ship = collision.gameObject.GetComponent<IShip>();

            if (ship != null)
            {
                if (ship != firedFromShip)
                {
                    ship.ApplyDamage(firedFromShip.GetCannonDamage());

                    firedFromShip.GetGameManager().OnCannonBallDestroyed(this);
                }
            }
            else
            {
                Mine mine = collision.gameObject.GetComponent<Mine>();

                if (mine != null)
                {
                    mine.MineDestroyed();
                }

                firedFromShip.GetGameManager().OnCannonBallDestroyed(this);
            }
        }
    }

    // ----- Custom funktioner ----- \\

    ///<summary>Bliver kaldt hver gang vi opretter en kugle</summary>
    public void CannonBallCreatedHandler(IShip ship, bool isMaster, int ID)
    {
        firedFromShip = ship;

        cannonBallID = ID;
        networkMaster = isMaster;

        if (isMaster == true)
        {
            rigid.velocity = transform.up * speed;

            Destroy(gameObject, 10.0f);
        }
        else
        {
            Destroy(rigid);
        }
    }

    // ----- API funktioner ----- \\

    public int GetCannonBallID()
    {
        return cannonBallID;
    }

    public IShip GetCannonBallOwner()
    {
        return firedFromShip;
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
