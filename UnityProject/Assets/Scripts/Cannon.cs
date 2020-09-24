using UnityEngine;

public class Cannon : MonoBehaviour
{
    // ----- Essentielle variabler ----- \\

    [SerializeField] private IShip parentShip = null;
    [SerializeField] private GameObject firePoint = null;

    // ----- Generelle variabler ----- \\
    private float nextFireReady = 0.0f;

    // ----- Custom funktioner ----- \\

    public void OnUpdate()
    {
        if(nextFireReady > 0.0f)
        {
            nextFireReady -= Time.deltaTime;
        }
    }

    ///<summary>Prøver at skyde vores kanoner</summary>
    public void ShootCannon()
    {
        if (nextFireReady <= 0.0f)
        {
            nextFireReady = parentShip.GetCannonNextFire();

            parentShip.GetGameManager().SpawnCannonBalls(parentShip, firePoint.transform.position, firePoint.transform.rotation, true);
        }
    }
}
