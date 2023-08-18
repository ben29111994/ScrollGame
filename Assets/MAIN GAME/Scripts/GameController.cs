using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using MoreMountains.NiceVibrations;
using UnityEngine.EventSystems;
using GPUInstancer;
using System.Linq;
using TMPro;
using SimpleInputNamespace;

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    [Header("Variable")]
    public int maxLevel;
    public bool isPlaying = false;
    public static bool isControl = true;
    int maxPlusEffect = 0;
    bool isVibrate = false;
    float h, v;
    public float speed;
    Vector3 dir;
    bool isHold = false;
    public List<Rigidbody> pixels = new List<Rigidbody>();
    public float forceFactor;
    public static int totalPixel;
    public float rebuildHeight;
    public List<GameObject> listMeshs = new List<GameObject>();
    public float CameraOffsetY = 30, CameraOffsetZ = 20;
    public int totalBall;
    public int timeAttempt;
    public int lastLevel;
    public int ballCollected;


    [Header("UI")]
    public GameObject winPanel;
    public TextMeshProUGUI currentLevelText;
    public TextMeshProUGUI nextLevelText;
    int currentLevel;
    public Slider levelProgress;
    public Text cointTxt;
    public static int coin;
    public Canvas canvas;
    public GameObject startGameMenu;
    public InputField levelInput;
    public GameObject nextButton;
    public Text title;
    public Text winMenu_title;
    public Text winMenu_coin;
    public TextMeshProUGUI timerTxt;
    public GameObject upgradeMenu;
    public Joystick joystick;
    float timer;
    public GameObject restartButton;
    public List<Image> tasks = new List<Image>();
    public TextMeshProUGUI rateText;

    [Header("Objects")]
    public GameObject funnel;
    public GameObject plusVarPrefab;
    public GameObject conffeti;
    GameObject conffetiSpawn;
    public GameObject mapReader;
    public GameObject map;
    public Transform magnetPoint;
    public GameObject winBG;
    public GameObject gemAnim;
    public GameObject gameplay2;
    public List<Transform> listReleaseScroll = new List<Transform>();
    

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        Application.targetFrameRate = 60;
        CameraOffsetY = Camera.main.transform.position.y;
        StartCoroutine(delayRefreshInstancer());
        StartCoroutine(delayStart());
    }

    IEnumerator delayStart()
    {
        yield return new WaitForSeconds(0.01f);
        currentLevel = DataManager.Instance.LevelGame;
        //currentLevelText.text = currentLevel.ToString();
        currentLevelText.text = "LEVEL " + (currentLevel + 1).ToString();
        startGameMenu.SetActive(true);
        title.DOColor(new Color32(255,255,255,0), 3);
        levelProgress.maxValue = totalPixel;
        levelProgress.value = 0;
    }

    public void UpdateTimer(int bonusValue)
    {
        timer += bonusValue;
        timerTxt.text = ((int)timer).ToString() + "s";
    }

    private void Update()
    {
        //Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, new Vector3(transform.position.x, CameraOffsetY, transform.position.z - CameraOffsetZ), Time.deltaTime * 30);
        if (Input.GetMouseButton(0)) { ButtonStartGame(); }
        if (isPlaying && isControl)
        {
            //timer -= Time.deltaTime;
            //timerTxt.text = ((int)timer).ToString() + "s";
            //if(timer <= 5)
            //{
            //    timerTxt.color = Color.red;
            //    timerTxt.transform.localScale = Vector3.one * 1.5f;
            //}
            //if(timer <= 0)
            //{
            //    StartCoroutine(Win());
            //}
            Control();
        }
        if (Input.GetMouseButtonUp(0))
        {
            OnMouseUp();
        }
    }

    private void Control()
    {
        //if (joystick.joystickHeld)
        //{
        //    h = joystick.xAxis.value;
        //    v = joystick.yAxis.value;

        //    dir = new Vector3(h, 0, v);
        //    transform.position = Vector3.MoveTowards(transform.position, transform.position + dir, Time.deltaTime * speed);
        //}

        if (Input.GetMouseButtonDown(0))
        {
            OnMouseDown();
        }

        if (Input.GetMouseButton(0))
        {
            OnMouseDrag();
        }
    }

    public static bool isDrag = false;
    public LayerMask scrollMask;
    GameObject targetScroll;
    void OnMouseDown()
    {
        if (!isDrag)
        {
            MMVibrationManager.Haptic(HapticTypes.MediumImpact);

            Vector3 startPos = Vector3.zero;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000, scrollMask))
            {
                if (hit.transform.CompareTag("Scroll"))
                {
                    isDrag = true;
                    startPoint = hit.point;
                    targetScroll = hit.transform.gameObject;
                }
            }
        }
    }

    Vector3 currentMousePos;
    Vector3 lastMousePos;
    Vector3 startPoint;
    Vector3 endPoint;
    public LayerMask dragMask;
    void OnMouseDrag()
    {
        if (isDrag)
        {
            if (targetScroll != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 1000, scrollMask))
                {
                    endPoint = hit.point;
                }

                currentMousePos = Input.mousePosition;
                Vector3 delta = lastMousePos - currentMousePos;
                lastMousePos = currentMousePos;

                MMVibrationManager.Haptic(HapticTypes.MediumImpact);
                Vector3 dragVectorDirection = (endPoint - startPoint).normalized;
                float checkDrag = Vector3.Distance(startPoint, endPoint);
                DraggedDirection dir;
                if (checkDrag > 0.5f)
                {
                    dir = GetDragDirection(dragVectorDirection);

                    var scroll = targetScroll.transform.parent;
                    var scrollControl = targetScroll.GetComponent<ScrollControl>();

                    if (scrollControl.transform.localScale.x > 0)
                    {
                        if (dir == DraggedDirection.Right)
                        {
                            Debug.Log("Right");
                            if (scroll.transform.localEulerAngles.y == 0)
                            {
                                if (scrollControl.isReleased)
                                {
                                    scrollControl.transform.localScale = new Vector3(scrollControl.transform.localScale.x * -1, 1, 1);
                                    scrollControl.ScrollReapRevert();
                                }
                                else
                                {
                                    listReleaseScroll.Add(scroll);
                                    scrollControl.ScrollRelease(listReleaseScroll.Count * 0.01f);
                                }
                            }
                            else if (scroll.transform.localEulerAngles.y == 180)
                            {
                                listReleaseScroll.Remove(scroll);
                                RefreshHeight();
                                if (scrollControl.isReleased)
                                    scrollControl.ScrollReap();
                                else
                                    scrollControl.ScrollMove();
                            }
                        }
                        else if (dir == DraggedDirection.Left)
                        {
                            Debug.Log("Left");
                            if (scroll.transform.localEulerAngles.y == 180)
                            {
                                if (scrollControl.isReleased)
                                {
                                    scrollControl.transform.localScale = new Vector3(scrollControl.transform.localScale.x * -1, 1, 1);
                                    scrollControl.ScrollReapRevert();
                                }
                                else
                                {
                                    listReleaseScroll.Add(scroll);
                                    scrollControl.ScrollRelease(listReleaseScroll.Count * 0.01f);
                                }
                            }
                            else if (scroll.transform.localEulerAngles.y == 0)
                            {
                                listReleaseScroll.Remove(scroll);
                                RefreshHeight();
                                if (scrollControl.isReleased)
                                    scrollControl.ScrollReap();
                                else
                                    scrollControl.ScrollMove();
                            }
                        }
                        else if (dir == DraggedDirection.Up)
                        {
                            Debug.Log("Up");
                            if (scroll.transform.localEulerAngles.y == 270)
                            {
                                if (scrollControl.isReleased)
                                {
                                    scrollControl.transform.localScale = new Vector3(scrollControl.transform.localScale.x * -1, 1, 1);
                                    scrollControl.ScrollReapRevert();
                                }
                                else
                                {
                                    listReleaseScroll.Add(scroll);
                                    scrollControl.ScrollRelease(listReleaseScroll.Count * 0.01f);
                                }
                            }
                            else if (scroll.transform.localEulerAngles.y == 90)
                            {
                                listReleaseScroll.Remove(scroll);
                                RefreshHeight();
                                if (scrollControl.isReleased)
                                    scrollControl.ScrollReap();
                                else
                                    scrollControl.ScrollMove();
                            }
                        }
                        else if (dir == DraggedDirection.Down)
                        {
                            Debug.Log("Down");
                            if (scroll.transform.localEulerAngles.y == 90)
                            {
                                if (scrollControl.isReleased)
                                {
                                    scrollControl.transform.localScale = new Vector3(scrollControl.transform.localScale.x * -1, 1, 1);
                                    scrollControl.ScrollReapRevert();
                                }
                                else
                                {
                                    listReleaseScroll.Add(scroll);
                                    scrollControl.ScrollRelease(listReleaseScroll.Count * 0.01f);
                                }
                            }
                            else if (scroll.transform.localEulerAngles.y == 270)
                            {
                                listReleaseScroll.Remove(scroll);
                                RefreshHeight();
                                if (scrollControl.isReleased)
                                    scrollControl.ScrollReap();
                                else
                                    scrollControl.ScrollMove();
                            }
                        }
                    }
                    else //in case scale = -1
                    {
                        if (dir == DraggedDirection.Right)
                        {
                            Debug.Log("Right");
                            if (scroll.transform.localEulerAngles.y == 180)
                            {
                                if (scrollControl.isReleased)
                                {
                                    scrollControl.transform.localScale = new Vector3(scrollControl.transform.localScale.x * -1, 1, 1);
                                    scrollControl.ScrollReapRevert();
                                }
                                else
                                {
                                    listReleaseScroll.Add(scroll);
                                    scrollControl.ScrollRelease(listReleaseScroll.Count * 0.01f);
                                }
                            }
                            else if (scroll.transform.localEulerAngles.y == 0)
                            {
                                listReleaseScroll.Remove(scroll);
                                RefreshHeight();
                                if (scrollControl.isReleased)
                                    scrollControl.ScrollReap();
                                else
                                    scrollControl.ScrollMove();
                            }
                        }
                        else if (dir == DraggedDirection.Left)
                        {
                            Debug.Log("Left");
                            if (scroll.transform.localEulerAngles.y == 0)
                            {
                                if (scrollControl.isReleased)
                                {
                                    scrollControl.transform.localScale = new Vector3(scrollControl.transform.localScale.x * -1, 1, 1);
                                    scrollControl.ScrollReapRevert();
                                }
                                else
                                {
                                    listReleaseScroll.Add(scroll);
                                    scrollControl.ScrollRelease(listReleaseScroll.Count * 0.01f);
                                }
                            }
                            else if (scroll.transform.localEulerAngles.y == 180)
                            {
                                listReleaseScroll.Remove(scroll);
                                RefreshHeight();
                                if (scrollControl.isReleased)
                                    scrollControl.ScrollReap();
                                else
                                    scrollControl.ScrollMove();
                            }
                        }
                        else if (dir == DraggedDirection.Up)
                        {
                            Debug.Log("Up");
                            if (scroll.transform.localEulerAngles.y == 90)
                            {
                                if (scrollControl.isReleased)
                                {
                                    scrollControl.transform.localScale = new Vector3(scrollControl.transform.localScale.x * -1, 1, 1);
                                    scrollControl.ScrollReapRevert();
                                }
                                else
                                {
                                    listReleaseScroll.Add(scroll);
                                    scrollControl.ScrollRelease(listReleaseScroll.Count * 0.01f);
                                }
                            }
                            else if (scroll.transform.localEulerAngles.y == 270)
                            {
                                listReleaseScroll.Remove(scroll);
                                RefreshHeight();
                                if (scrollControl.isReleased)
                                    scrollControl.ScrollReap();
                                else
                                    scrollControl.ScrollMove();
                            }
                        }
                        else if (dir == DraggedDirection.Down)
                        {
                            Debug.Log("Down");
                            if (scroll.transform.localEulerAngles.y == 270)
                            {
                                if (scrollControl.isReleased)
                                {
                                    scrollControl.transform.localScale = new Vector3(scrollControl.transform.localScale.x * -1, 1, 1);
                                    scrollControl.ScrollReapRevert();
                                }
                                else
                                {
                                    listReleaseScroll.Add(scroll);
                                    scrollControl.ScrollRelease(listReleaseScroll.Count * 0.01f);
                                }
                            }
                            else if (scroll.transform.localEulerAngles.y == 90)
                            {
                                listReleaseScroll.Remove(scroll);
                                RefreshHeight();
                                if (scrollControl.isReleased)
                                    scrollControl.ScrollReap();
                                else
                                    scrollControl.ScrollMove();
                            }
                        }
                    }
                }
            }
        }
    }

    void OnMouseUp()
    {
        if (isDrag)
        {         
            isDrag = false;
        }
    }

    public void RefreshHeight()
    {
        if(listReleaseScroll.Count == 1)
        {
            var height = 0.002f;
            listReleaseScroll[0].transform.GetChild(0).transform.DOLocalMoveY(height, 0);
        }
        for (int i = 0; i < listReleaseScroll.Count - 1; i++)
        {
            var height = (i + 1) * 0.002f;
            listReleaseScroll[i].transform.GetChild(0).transform.DOLocalMoveY(height, 0);
        }
    }

    private enum DraggedDirection
    {
        Up,
        Down,
        Right,
        Left
    }

    private DraggedDirection GetDragDirection(Vector3 dragVector)
    {
        float positiveX = Mathf.Abs(dragVector.x);
        float positiveY = Mathf.Abs(dragVector.z);
        DraggedDirection draggedDir;
        if (positiveX > positiveY)
        {
            draggedDir = (dragVector.x > 0) ? DraggedDirection.Right : DraggedDirection.Left;
        }
        else
        {
            draggedDir = (dragVector.z > 0) ? DraggedDirection.Up : DraggedDirection.Down;
        }
        return draggedDir;
    }

    IEnumerator delayVibrate()
    {
        yield return new WaitForSeconds(0.2f);
        isVibrate = false;
    }

    public Vector3 worldToUISpace(Canvas parentCanvas, Vector3 worldPos)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        Vector2 movePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvas.transform as RectTransform, screenPos, parentCanvas.worldCamera, out movePos);
        return parentCanvas.transform.TransformPoint(movePos);
    }

    public void ButtonStartGame()
    {
        startGameMenu.SetActive(false);
        restartButton.SetActive(true);
        isPlaying = true;
        isHold = true;
    }

    public static int coinEarn;
    public void Win()
    {
        //yield return new WaitForSeconds(0.01f);
        if (isPlaying)
        {
            //AnalyticsManager.instance.CallEvent(AnalyticsManager.EventType.EndEvent);
            isPlaying = false;
            conffetiSpawn = Instantiate(conffeti);
            //winMenu_title.text = "LEVEL " + currentLevel.ToString();
            //winMenu_coin.text = coin.ToString();
            //yield return new WaitForSeconds(0.1f);
            winBG.SetActive(true);
            winBG.GetComponent<MeshRenderer>().material.DOFade(1, 1);
            conffetiSpawn.transform.parent = Camera.main.transform;
            conffetiSpawn.transform.localPosition = new Vector3(winBG.transform.localPosition.x, winBG.transform.localPosition.y - 20, winBG.transform.localPosition.z);
            winPanel.SetActive(true);
            //cointTxt.gameObject.SetActive(false);
            //timerTxt.gameObject.SetActive(false);
            levelProgress.gameObject.SetActive(false);
            //upgradeMenu.SetActive(false);
            //funnel.GetComponent<QuickOutline>().enabled = false;
            //winMenu_coin.text = (ballCollected).ToString();
            //coinEarn = ballCollected * 10;
            //gemAnim.SetActive(true);
            //var bonusCoin = coin + pixels.Count * 10;
            //cointTxt.DOCounter(coin, bonusCoin, 1.5f);
            //DataManager.Instance.Coin = bonusCoin;
        }
    }

    IEnumerator delayRefreshInstancer()
    {
        yield return new WaitForSeconds(0.01f);
        //AddRemoveInstances.instance.Setup();
    }

    public void LoadNewLevel()
    {
        winPanel.SetActive(false);
        var temp = conffetiSpawn;
        Destroy(temp);
        bool isWin = false;
        winBG.SetActive(false );
        //SceneManager.LoadScene(0);
        LevelGenerator.Instance.NextTask();
        Restart();
    }

    public void Restart()
    {
        //levelProgress.gameObject.SetActive(true);
        restartButton.SetActive(false);
        startGameMenu.SetActive(true);
        isPlaying = false;
        isHold = false;
        StartCoroutine(delayStart());
        //SceneManager.LoadScene(0);
    }   

    public void OnChangeMap()
    {
        if (levelInput != null)
        {
            int level = int.Parse(levelInput.text.ToString());
            Debug.Log(level);
            if (level < maxLevel)
            {
                DataManager.Instance.LevelGame = level;
                SceneManager.LoadScene(0);
            }
        }
    }

    public void ButtonNextLevel()
    {
        title.DOKill();
        isPlaying = true;
        currentLevel++;
        if (currentLevel > maxLevel)
        {
            currentLevel = 0;
        }
        DataManager.Instance.LevelGame = currentLevel;
        SceneManager.LoadScene(0);
    }

    public void Reset()
    {
        listReleaseScroll.Clear();
    }
}
