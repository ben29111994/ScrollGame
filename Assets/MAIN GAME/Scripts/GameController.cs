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
using UnityEditorInternal.VersionControl;
using static UnityEditor.PlayerSettings;

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    [Header("Variable")]
    public int maxLevel;
    public bool isPlaying = false;
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
    public Text currentLevelText;
    public Text nextLevelText;
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
    List<Transform> listReleaseScroll = new List<Transform>();
    

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        //PlayerPrefs.DeleteAll();
        //DOTween.SetTweensCapacity(5000, 5000);
        Application.targetFrameRate = 60;
        CameraOffsetY = Camera.main.transform.position.y;
        StartCoroutine(delayRefreshInstancer());
        StartCoroutine(delayStart());
    }

    IEnumerator delayStart()
    {
        yield return new WaitForSeconds(0.01f);
        //AnalyticsManager.instance.CallEvent(AnalyticsManager.EventType.StartEvent);
        currentLevel = DataManager.Instance.LevelGame;
        lastLevel = PlayerPrefs.GetInt("LastLevel");
        if (lastLevel == currentLevel)
        {
            timeAttempt = PlayerPrefs.GetInt("TimeAttempt");
            timeAttempt++;
            PlayerPrefs.SetInt("TimeAttempt", timeAttempt);
        }
        else
        {
            lastLevel = currentLevel;
            timeAttempt = 0;
            PlayerPrefs.SetInt("LastLevel", lastLevel);
            PlayerPrefs.SetInt("TimeAttempt", timeAttempt);
        }
        currentLevelText.text = currentLevel.ToString();
        nextLevelText.text = (currentLevel + 1).ToString();
        coin = DataManager.Instance.Coin;
        cointTxt.text = coin.ToString();
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

    private void FixedUpdate()
    {
        //Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, new Vector3(transform.position.x, CameraOffsetY, transform.position.z - CameraOffsetZ), Time.deltaTime * 30);
        if (Input.GetMouseButton(0)) { ButtonStartGame(); }
        if (isPlaying)
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

            for (int i = 0; i < pixels.Count; i++)
            {
                if (pixels[i] != null && pixels[i].GetComponent<Tile>().isCheck)
                {
                    Vector3 magnetField = magnetPoint.position - pixels[i].position;
                    var dis = Vector3.Distance(magnetPoint.position, pixels[i].position);
                    float distance = Vector3.Distance(magnetPoint.transform.position, pixels[i].transform.position);
                    if (distance <= 2f)
                    {
                        pixels[i].AddForce(magnetField * forceFactor * Time.fixedDeltaTime);
                    }
                    else
                    {
                        if (OnCollected.Instance.tempSizeLevel >= pixels[i].GetComponent<Tile>().ballLevel || DataManager.Instance.SizeLevel == 8)
                            pixels[i].AddForce(magnetField * forceFactor / distance * Time.fixedDeltaTime);
                        else
                        {
                            magnetField = new Vector3(magnetField.x, 0, magnetField.z);
                            pixels[i].AddForce(magnetField * 5000 / distance * Time.fixedDeltaTime);
                        }

                    }
                }
            }
        }
    }

    //public void RemovePixel(Rigidbody target)
    //{
    //    if (target != null)
    //    {
    //        pixels.Remove(target);
    //    }
    //}

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

        if (Input.GetMouseButtonUp(0))
        {
            OnMouseUp();
        }
    }

    bool isDrag = false;
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
            }
        }
    }

    void OnMouseUp()
    {
        if (isDrag)
        {
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
                        if (scroll.transform.localEulerAngles.y == 0 )
                        {
                            if (scrollControl.isReleased)
                            {
                                scrollControl.transform.localScale = new Vector3(scrollControl.transform.localScale.x * -1, 1, 1);
                                scrollControl.ScrollReap();
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
                                scrollControl.ScrollReap();
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
                                scrollControl.ScrollReap();
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
                                scrollControl.ScrollReap();
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
                                scrollControl.ScrollReap();
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
                                scrollControl.ScrollReap();
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
                                scrollControl.ScrollReap();
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
                                scrollControl.ScrollReap();
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
                            if (scrollControl.isReleased)
                                scrollControl.ScrollReap();
                            else
                                scrollControl.ScrollMove();
                        }
                    }
                }
            }
            isDrag = false;
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
        isPlaying = true;
        isHold = true;
    }

    public static int coinEarn;
    IEnumerator Win()
    {
        yield return new WaitForSeconds(0.01f);
        if (isPlaying)
        {
            //AnalyticsManager.instance.CallEvent(AnalyticsManager.EventType.EndEvent);
            isPlaying = false;
            conffetiSpawn = Instantiate(conffeti);
            //winMenu_title.text = "LEVEL " + currentLevel.ToString();
            winMenu_coin.text = coin.ToString();
            yield return new WaitForSeconds(0.1f);
            winBG.SetActive(true);
            winBG.GetComponent<MeshRenderer>().material.DOFade(1, 1);
            conffetiSpawn.transform.parent = Camera.main.transform;
            conffetiSpawn.transform.localPosition = new Vector3(winBG.transform.localPosition.x, winBG.transform.localPosition.y - 20, winBG.transform.localPosition.z);
            winPanel.SetActive(true);
            //cointTxt.gameObject.SetActive(false);
            timerTxt.gameObject.SetActive(false);
            levelProgress.gameObject.SetActive(false);
            upgradeMenu.SetActive(false);
            funnel.GetComponent<QuickOutline>().enabled = false;
            winMenu_coin.text = (ballCollected).ToString();
            coinEarn = ballCollected * 10;
            gemAnim.SetActive(true);
            var bonusCoin = coin + pixels.Count * 10;
            cointTxt.DOCounter(coin, bonusCoin, 1.5f);
            DataManager.Instance.Coin = bonusCoin;
        }
    }

    IEnumerator delayRefreshInstancer()
    {
        yield return new WaitForSeconds(0.01f);
        //AddRemoveInstances.instance.Setup();
    }

    public void LoadGamePlay2()
    {
        winPanel.SetActive(false);
        var temp = conffetiSpawn;
        Destroy(temp);
        gameplay2.SetActive(true);
        bool isWin = false;
        totalBall = LevelGenerator.Instance.totalBall;
        var collectedPercentage = ballCollected * 100 / totalBall;
        winBG.SetActive(false );
        if(collectedPercentage > 70 || (currentLevel == 1 && timeAttempt >= 1) || timeAttempt >= 5)
        {
            isWin = true;
        }
        SceneManager.LoadScene(0);
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
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

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pixel") && !other.GetComponent<Tile>().isCheck && isPlaying && !other.GetComponent<Tile>().isMagnet)
        {
            pixels.RemoveAll(item => item == null);
            //if (!isVibrate)
            //{
            //    isVibrate = true;
            //    StartCoroutine(delayVibrate());
            //    MMVibrationManager.Haptic(HapticTypes.LightImpact);
            //}
            other.GetComponent<Tile>().isCheck = true;

            pixels.Add(other.GetComponent<Rigidbody>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Pixel") && other.GetComponent<Tile>().isCheck && isPlaying)
        {
            other.GetComponent<Tile>().isCheck = false;
            other.GetComponent<Tile>().isMagnet = false;
            other.transform.parent = transform.parent;
            pixels.Remove(other.GetComponent<Rigidbody>());
        }
    }
}
