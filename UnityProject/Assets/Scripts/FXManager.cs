using System.Collections.Generic;
using UnityEngine;

public class FXManager : MonoBehaviour
{
    // ----- Essentielle variabler ----- \\

    [SerializeField] private GameObject parentFX = null;

    [SerializeField] private List<GameObject> explosionsFX = new List<GameObject>();
    [SerializeField] private List<GameObject> fireBlastsFX = new List<GameObject>();

    // ----- API funktioner ----- \\

    public void ExplosionAtPos(Vector3 pos)
    {
        GameObject explosionObj = Instantiate(explosionsFX[Random.Range(0, explosionsFX.Count)], pos, Quaternion.identity);
        explosionObj.transform.parent = parentFX.transform;

        ParticleSystem explosion = explosionObj.GetComponent<ParticleSystem>();

        explosion.Play();

        Destroy(explosion.gameObject, explosion.main.duration + explosion.main.startLifetime.constantMax);
    }

    public void FireBlastAtPos(Vector3 pos)
    {
        GameObject fireObj = Instantiate(fireBlastsFX[Random.Range(0, fireBlastsFX.Count)], pos, Quaternion.identity);
        fireObj.transform.parent = parentFX.transform;

        ParticleSystem explosion = fireObj.GetComponent<ParticleSystem>();

        explosion.Play();

        Destroy(explosion.gameObject, explosion.main.duration + explosion.main.startLifetime.constantMax);
    }
}