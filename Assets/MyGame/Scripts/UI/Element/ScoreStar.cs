using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreStar : MonoBehaviour
{
    public Image star;
    public float delay = 0.5f;
    public bool isActivated = false;
    public ParticlePlayer startFx;

    void Start()
    {
        SetActive(false);
    }

    private void SetActive(bool state)
    {
        if(star != null)
        {
            star.gameObject.SetActive(state);
        }
    }

    public void Activate()
    {
        if (isActivated) return;

        StartCoroutine(ActivateRoutine());
    }

    private IEnumerator ActivateRoutine()
    {
        isActivated = true;
        startFx.Play();
        AudioManager.Instance.PlaySE(AUDIO.SE_ITEMIZE);
        yield return new WaitForSeconds(delay);
        SetActive(true);
    }
}
