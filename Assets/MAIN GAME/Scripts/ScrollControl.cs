using MegaFiers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEditor.SceneManagement;

public class ScrollControl : MonoBehaviour
{
    public MegaBend bend;
    public float fullScrollLengthOffset;
    float baseOffset;
    bool isReleasing;
    public bool isReleased;
    // Start is called before the first frame update

    void Start()
    {
        baseOffset = bend.Offset.x;
    }

    public void ScrollRelease(float moveY)
    {
        DOTween.KillAll();
        transform.DOLocalMoveY(0.05f + moveY, 0);
        isReleasing = true;
        DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, fullScrollLengthOffset, 0.5f).OnComplete(() => { 
            //Destroy(GetComponent<MeshCollider>()); 
            //var addCollider = gameObject.AddComponent<MeshCollider>();
            //addCollider.convex = true;
            //addCollider.isTrigger = true;
            isReleasing = false;
            isReleased = true;
        });      
    }

    public void ScrollReap()
    {
        DOTween.KillAll();
        DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, baseOffset, 0.5f).OnComplete(() => { 
            transform.DOLocalMoveY(0.05f, 0);
            //Destroy(GetComponent<MeshCollider>());
            //var addCollider = gameObject.AddComponent<MeshCollider>();
            //addCollider.convex = true;
            //addCollider.isTrigger = true;
            isReleased = false;
        });
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

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Scroll") || other.CompareTag("Wall"))
        {
            if(isReleasing)
            {
                DOTween.KillAll();
                ScrollReap();
            }
        }
    }
}
