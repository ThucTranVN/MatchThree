using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : BaseManager<EffectManager>
{
    public GameObject clearFxPrefab;
    public GameObject breakFxPrefab;
    public GameObject doubleBreakFxPrefab;
    public GameObject bombFxPrefab;
    public GameObject starFxPrefab;

    public void ClearPieceFxAt(int x, int y, int z = 0)
    {
        GameObject clearFx = Instantiate(clearFxPrefab, new Vector3(x, y, z), Quaternion.identity) as GameObject;
        ParticlePlayer particlePlayer = clearFx.GetComponent<ParticlePlayer>();

        if(particlePlayer != null)
        {
            particlePlayer.Play();
        }
    }

    public void BreakTileFxAt(int breakableValue, int x, int y, int z = 0)
    {
        GameObject breakFx = null;
        ParticlePlayer particlePlayer = null;

        if(breakableValue > 1)
        {
            if(doubleBreakFxPrefab != null)
            {
                breakFx = Instantiate(doubleBreakFxPrefab, new Vector3(x, y, z), Quaternion.identity) as GameObject;
            }
        }
        else
        {
            if (breakFxPrefab != null)
            {
                breakFx = Instantiate(breakFxPrefab, new Vector3(x, y, z), Quaternion.identity) as GameObject;
            }     
        }

        if(breakFx != null)
        {
            particlePlayer = breakFx.GetComponent<ParticlePlayer>();
            if(particlePlayer != null)
            {
                particlePlayer.Play();
            }
        }
    }

    public void BombFxAt(int x, int y, int z = 0)
    {
        if(bombFxPrefab != null)
        {
            GameObject bombFx = Instantiate(bombFxPrefab, new Vector3(x, y, z), Quaternion.identity) as GameObject;
            ParticlePlayer particlePlayer = bombFx.GetComponent<ParticlePlayer>();

            if (particlePlayer != null)
            {
                particlePlayer.Play();
            }
        }
    }

}
