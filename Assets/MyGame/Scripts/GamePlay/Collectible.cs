using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : GamePiece
{
    public bool clearedByBomb = false;
    public bool clearedAtBottom = true;

    void Start()
    {
        matchValue = MatchValue.None;
    }

    public override void OnPlaySFX()
    {
        if (AudioManager.HasInstance)
        {
            AudioManager.Instance.PlaySE(AUDIO.SE_PLUG);
        }
    }
}
