using MegaFiers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ScrollControl : MonoBehaviour
{
    public MegaBend bend;
    public float fullScrollLengthOffset;
    // Start is called before the first frame update

    public void ScrollLeft()
    {
        DOTween.KillAll();
        DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, 5, 0.5f);
    }

    public void ScrollRight()
    {
        DOTween.KillAll();
        DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, 5, 0.5f);
    }
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            DOTween.KillAll();
            DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, 5, 0.5f);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            DOTween.KillAll();
            DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, -10, 0.5f);
        }
    }
}
