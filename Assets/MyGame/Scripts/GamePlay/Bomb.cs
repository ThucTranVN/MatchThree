using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : GamePiece
{
    public BombType bombType;

    public override void OnPlaySFX()
    {
        if (AudioManager.HasInstance)
        {
            AudioManager.Instance.PlaySE(AUDIO.SE_CHEESEHEADBURGERPOP);
        }
    }
}
