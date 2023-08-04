using DG.Tweening;
using GPUInstancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.NiceVibrations;

public class OnCollected : MonoBehaviour
{
    public static OnCollected Instance;
    //public static OnCollected Instance { get { return instance; } }
    public Transform spawnPos;
    public List<Rigidbody> listMaintainBalls = new List<Rigidbody>();
    public int limit = 100;
    bool isUpgrading = false;
    bool isVibrate = false;
    public int tempSizeLevel;

    private void Start()
    {
        Instance = this;
        var levelTimer = DataManager.Instance.TimerLevel;
        var levelSize = DataManager.Instance.SizeLevel;
        var levelPower = DataManager.Instance.PowerLevel;
        tempSizeLevel = levelSize;
        UpgradeTimer(levelTimer);
        UpgradeSize(levelSize);
        UpgradePower(levelPower);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pixel") && other.GetComponent<Tile>().isCheck && (tempSizeLevel >= other.GetComponent<Tile>().ballLevel || DataManager.Instance.SizeLevel >= 8))
        {
            if (!isVibrate)
            {
                isVibrate = true;
                StartCoroutine(delayVibrate());
                MMVibrationManager.Haptic(HapticTypes.LightImpact);
            }
            other.GetComponent<Rigidbody>().velocity = Vector3.zero;
            other.GetComponent<Tile>().isCheck = false;
            other.GetComponent<Tile>().isMagnet = false;
            other.transform.parent = transform.parent;
            other.transform.DOLocalMove(spawnPos.localPosition, 0.2f);
            other.GetComponent<SphereCollider>().isTrigger = false;
            other.GetComponent<Rigidbody>().drag = 20;
            other.GetComponent<Rigidbody>().angularDrag = 20;
            other.transform.localScale = new Vector3(16, 8, 16);
            //other.transform.DOScale(10f, 0.2f);
            listMaintainBalls.Add(other.GetComponent<Rigidbody>());
            GameController.Instance.ballCollected++;
            GameController.Instance.levelProgress.value++;
            if (listMaintainBalls.Count >= limit && tempSizeLevel < 8)
            {
                if (!isUpgrading)
                {
                    isUpgrading = true;
                    //UpgradeTimer(1);
                    tempSizeLevel++;
                    UpgradeSize(1);
                    UpgradePower(1);
                }
            }
        }
    }

    IEnumerator delayVibrate()
    {
        yield return new WaitForSeconds(0.2f);
        isVibrate = false;
    }

    private void Update()
    {
        if (listMaintainBalls.Count >= 400)
        {
            //if (!isUpgrading)
            //{
            //    isUpgrading = true;
            //    Upgrade();
            //}
            for (int i = 0; i < listMaintainBalls.Count - 400; i++)
            {
                try
                {
                    AddRemoveInstances.instance.RemoveInstances(listMaintainBalls[i].GetComponent<GPUInstancerPrefab>());
                }
                catch { }
            }
            listMaintainBalls.RemoveAll(item => item == null);
        }
    }

    public void UpgradeTimer(int level)
    {
        var value = 15 + level * 2;
        GameController.Instance.UpdateTimer(value);
    }

    public void UpgradeSize(int level)
    {
        var scaleValue = transform.parent.transform.parent.transform.localScale;
        transform.parent.transform.parent.transform.DOPunchScale(transform.parent.transform.parent.transform.localScale * 0.4f, 0.5f, 3, 1).SetEase(Ease.Linear).OnComplete(() =>
        {
            transform.parent.transform.parent.transform.DOScale(new Vector3(scaleValue.x + 0.25f * level, scaleValue.y + 0.25f * level, scaleValue.z + 0.25f * level), 0.2f);
            limit += 50;
            isUpgrading = false;
        }); ;
        DOTween.To(() => GameController.Instance.CameraOffsetY, x => GameController.Instance.CameraOffsetY = x, GameController.Instance.CameraOffsetY + 4 * level, 0.5f);
        DOTween.To(() => GameController.Instance.CameraOffsetZ, x => GameController.Instance.CameraOffsetZ = x, GameController.Instance.CameraOffsetZ + 4 * level, 0.5f);
    }

    public void UpgradePower(int level)
    {
        GameController.Instance.speed += level * 2;
        GameController.Instance.funnel.GetComponent<Renderer>().material.DOColor(Color.green, 0.4f).SetLoops(2, LoopType.Yoyo);
        GameController.Instance.forceFactor += 4000*level;
    }
}
