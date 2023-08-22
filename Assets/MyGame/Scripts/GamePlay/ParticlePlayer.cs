using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePlayer : MonoBehaviour
{
    public ParticleSystem[] allParticles;
    public float lifeTime = 1f;
    public bool destroyImmediately = true;

    void Start()
    {
        allParticles = GetComponentsInChildren<ParticleSystem>();
        if (destroyImmediately)
        {
            Destroy(gameObject, lifeTime);
        }
    }

    public void Play()
    {
        foreach (var ps in allParticles)
        {
            ps.Stop();
            ps.Play();
        }

        Destroy(gameObject, lifeTime);
    }
}
