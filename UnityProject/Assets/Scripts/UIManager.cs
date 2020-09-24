using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // ----- Essentielle variabler ----- \\

    [SerializeField] private GameObject damageTextPrefab = null;

    [SerializeField] private GameObject compass = null;
    [SerializeField] private Fuelbar fuelbar = null;
    [SerializeField] private Healthbar healthbar = null;

    // ----- Custom funktioner ----- \\

    public void ShowDamageText(GameObject parent, int damage, float length)
    {
        GameObject damageTextObj = Instantiate(damageTextPrefab);

        DamageUI damageTextScript = damageTextObj.GetComponentInChildren<DamageUI>();
        damageTextScript.SetFollowTransform(parent.transform);

        Text damageText = damageTextObj.GetComponentInChildren<Text>();
        damageText.text = damage.ToString();

        Destroy(damageTextObj, length);
    }

    // ----- API funktioner ----- \\

    ///<summary>Finder alle nødvendige ting i scenen som vi skal bruge til bruger interfacet</summary>
    public void Initialize()
    {
        compass = GameObject.FindGameObjectWithTag("Compass");

        fuelbar = FindObjectOfType<Fuelbar>();
        healthbar = FindObjectOfType<Healthbar>();
    }

    ///<summary>Skaffer vores healthbar</summary>
	public Healthbar GetHealthBar()
    {
        return healthbar;
    }

    ///<summary>Skaffer vores fuelbar</summary>
    public Fuelbar GetFuelBar()
    {
        return fuelbar;
    }

    ///<summary>Skaffer vores kompass</summary>
    public GameObject GetCompass()
    {
        return compass;
    }
}
