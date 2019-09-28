using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public enum CurrentSide { left, right };
    public CurrentSide currentSide;
    public Camera mainCamera;
    public float cameraSpeed;
    // Start is called before the first frame update

    public Transform GreenLightPosition;
    public int greenLightIndex, bridgeIndex, nextBridgeIndex;
    public float elevatorLightSpeed;
    public int playerHealth;
    public int currentLevel;
    public bool awaitingTap;
    public bool StopLight;
    public bool TapNow;
    public bool changingLevels;
    public bool gameOver = false;

    public TextMeshProUGUI HealthText, LevelText, GameOverText;

    public GameObject NextLevelPrefab, RunnerPrefab;

    //Game Constants
    public int numberOfElevatorsPerLevel = 5;
    public float elevatorHeight;
    public int pixelsPerUnit = 100;

    private void Awake()
    {
        Application.targetFrameRate = 70;
    }
    void Start()
    {
        elevatorHeight = (Screen.height / numberOfElevatorsPerLevel)/ numberOfElevatorsPerLevel;
        currentSide = CurrentSide.right;
        ResetToLevelOne();
        StartCoroutine(changeGreenLightIndex_Co());
        SpawnNextLevel();
        ChangeText(playerHealth);
    }

    //void OnGUI()//Debugging Window
    //{
    //        GUI.Box(new Rect(0, Screen.height - 80, 250, 80), GUIContent.none);
    //        GUI.Label(new Rect(0, Screen.height - 50, 250, 20), "fps: " + 1.0f / Time.deltaTime);
    //}

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxisRaw("Fire1") != 0 && !changingLevels)
        {
            if (gameOver)
            {
                ResetToLevelOne();
            }
            else
            {
                Debug.Log("Pressed Mouse Button");
                if (awaitingTap)
                {
                    awaitingTap = false;
                    StopLight = true;
                    StartCoroutine(OpenElevators());
                }
            }
            
        }
        if (bridgeIndex == Mathf.Abs(greenLightIndex))
            TapNow = true;
        else
            TapNow = false;
    }

    void ChangeText(int health)
    {
        HealthText.text = "Health: " + health;
        LevelText.text = "Level: " + currentLevel;
    }

    IEnumerator OpenElevators()
    {
        GameObject runner = spawnRunner();
        if (Mathf.Abs(greenLightIndex) == bridgeIndex)
            yield return StartCoroutine(SafelyCrossBridge(runner));
        else if (greenLightIndex < bridgeIndex)
            yield return StartCoroutine(DropBelowBridge(runner));
        else if (greenLightIndex > bridgeIndex)
            yield return StartCoroutine(DropOnTopBridge(runner));
        yield return null;
        awaitingTap = true;
        Debug.Log("Exit OpenElevatorCoroutine");
    }

    public GameObject spawnRunner()
    {
        Vector3 runnerStartPos = new Vector3();
        switch (Mathf.Abs(greenLightIndex))
        {
            case 4:
                if (currentSide == CurrentSide.left)
                    runnerStartPos = new Vector3(-3.2f, 3.14f, 0);
                else if (currentSide == CurrentSide.right)
                    runnerStartPos = new Vector3(3.2f, 3.14f, 0);
                break;
            case 3:
                if (currentSide == CurrentSide.left)
                    runnerStartPos = new Vector3(-3.2f, 1.22f, 0);
                else if (currentSide == CurrentSide.right)
                    runnerStartPos = new Vector3(3.2f, 1.22f, 0);
                break;
            case 2:
                if (currentSide == CurrentSide.left)
                    runnerStartPos = new Vector3(-3.2f, -0.7f, 0);
                else if (currentSide == CurrentSide.right)
                    runnerStartPos = new Vector3(3.2f, -0.7f, 0);
                break;
            case 1:
                if (currentSide == CurrentSide.left)
                    runnerStartPos = new Vector3(-3.2f, -2.62f, 0);
                else if (currentSide == CurrentSide.right)
                    runnerStartPos = new Vector3(3.2f, -2.62f, 0);
                break;
            case 0:
                if (currentSide == CurrentSide.left)
                    runnerStartPos = new Vector3(-3.2f, -4.54f, 0);
                else if (currentSide == CurrentSide.right)
                    runnerStartPos = new Vector3(3.2f, -4.54f, 0);
                break;
        }
        GameObject runner = Instantiate(RunnerPrefab, runnerStartPos, Quaternion.identity);
        runner.transform.parent = GameObject.Find("Elevators/Level" + currentLevel).transform;
        return runner;
    }

    public void StartRunning(GameObject runner)
    {
        if (currentSide == CurrentSide.left)
        {
            runner.GetComponent<Runner>().runDir = Runner.RunDir.right;
            runner.transform.localScale = new Vector3(-0.15f, 0.15f, 1);
        }
        else if (currentSide == CurrentSide.right)
        {
            runner.GetComponent<Runner>().runDir = Runner.RunDir.left;
            runner.transform.localScale = new Vector3(0.15f, 0.15f, 1);
        }
    }

    public IEnumerator OpenDoors()
    {
        
        GameObject leftDoor = GameObject.Find("Elevators/Level" + currentLevel + "/LeftDoors/LeftDoor" + Mathf.Abs(greenLightIndex));
        GameObject rightDoor = GameObject.Find("Elevators/Level" + currentLevel + "/RightDoors/RightDoor" + Mathf.Abs(greenLightIndex));
        leftDoor.GetComponent<Animator>().SetTrigger("open");
        rightDoor.GetComponent<Animator>().SetTrigger("open");
        yield return null;
    }

    public IEnumerator CloseDoors()
    {
        GameObject leftDoor = GameObject.Find("Elevators/Level" + currentLevel + "/LeftDoors/LeftDoor" + Mathf.Abs(greenLightIndex));
        GameObject rightDoor = GameObject.Find("Elevators/Level" + currentLevel + "/RightDoors/RightDoor" + Mathf.Abs(greenLightIndex));
        leftDoor.GetComponent<Animator>().SetTrigger("close");
        rightDoor.GetComponent<Animator>().SetTrigger("close");
        yield return null;
    }

    IEnumerator SafelyCrossBridge(GameObject runner)
    {
        //play animation for safely cross the bridge
        
        Debug.Log("win");
        StartCoroutine(OpenDoors());
        SpeedUpLight();
        yield return new WaitForSeconds(0.5f);
        StartRunning(runner);
        yield return new WaitForSeconds(1.2f);
        Destroy(runner);
        StartCoroutine(CloseDoors());
        yield return new WaitForSeconds(0.5f);
        StopLight = false;
        StartCoroutine(changeGreenLightIndex_Co());
        ChangeSides();
        yield return StartCoroutine(ShiftDownLevels());
        ChangeText(playerHealth);
    }

    void ChangeSides()
    {
        if (currentSide == CurrentSide.left)
            currentSide = CurrentSide.right;
        else if (currentSide == CurrentSide.right)
            currentSide = CurrentSide.left;
    }

    IEnumerator DropBelowBridge(GameObject runner)
    {
        //play animation for falling
        Debug.Log("lose");
        StartCoroutine(OpenDoors());
        yield return new WaitForSeconds(0.5f);
        StartRunning(runner);
        yield return new WaitForSeconds(0.4f);
        runner.GetComponent<Runner>().falling = true;
        StartCoroutine(CloseDoors());
        yield return new WaitForSeconds(0.7f);
        Destroy(runner);
        StopLight = false;
        StartCoroutine(changeGreenLightIndex_Co());
        ChangeSides();
        playerHealth -= 10;
        ChangeText(playerHealth);
        if (playerHealth <= 0)
            GameOver();
    }

    IEnumerator DropOnTopBridge(GameObject runner)
    {
        //play animation for falling
        Debug.Log("lose");
        StartCoroutine(OpenDoors());
        yield return new WaitForSeconds(0.5f);
        StartRunning(runner);
        yield return new WaitForSeconds(0.4f);
        runner.GetComponent<Runner>().falling = true;
        StartCoroutine(CloseDoors());
        yield return new WaitForSeconds(0.7f);
        Destroy(runner);
        StopLight = false;
        StartCoroutine(changeGreenLightIndex_Co());
        ChangeSides();
        playerHealth -= 10;
        ChangeText(playerHealth);

        if (playerHealth <= 0)
            GameOver();
    }

    void SpeedUpLight()
    {
        elevatorLightSpeed = Mathf.Round(elevatorLightSpeed * 0.75f * 10000)/10000;
    }

    void loseHealth()
    {
        if (playerHealth <= 0)
            GameOver();
    }

    void GameOver()
    {
        //reset all variables
        gameOver = true;
        HealthText.gameObject.SetActive(false);
        LevelText.gameObject.SetActive(false);
        GameOverText.gameObject.SetActive(true);
    }

    void ResetToLevelOne(){
        gameOver = false;
        HealthText.gameObject.SetActive(true);
        LevelText.gameObject.SetActive(true);
        GameOverText.gameObject.SetActive(false);
        currentLevel = 1;
        greenLightIndex = 1;
        elevatorLightSpeed = 0.5f;
        playerHealth = 50;
        ChangeText(playerHealth);
    }

    IEnumerator changeGreenLightIndex_Co()
    {
        while (!StopLight)
        {
            greenLightIndex++;
            if (greenLightIndex > 4)
                greenLightIndex = -4;
            ChangeGreenLightSprite(Mathf.Abs(greenLightIndex));
            if(Mathf.Abs(greenLightIndex) == 4)
                yield return new WaitForSeconds(elevatorLightSpeed/2);
            else
                yield return new WaitForSeconds(elevatorLightSpeed);
        }
    }

    void ChangeGreenLightSprite(int index)
    {
        //changes the visual appearance of the elevators
        switch (index)
        {
            case 4:
                GreenLightPosition.position = new Vector3(0, 3.84f, 0);
                break;
            case 3:
                GreenLightPosition.position = new Vector3(0, 1.92f, 0);
                break;
            case 2:
                GreenLightPosition.position = new Vector3(0, 0, 0);
                break;
            case 1:
                GreenLightPosition.position = new Vector3(0, -1.92f, 0);
                break;
            case 0:
                GreenLightPosition.position = new Vector3(0, -3.84f, 0);
                break;
        }
    }

    void SpawnNextLevel()
    {
        nextBridgeIndex = Random.Range(0, 4);
        GameObject nextLevel = Instantiate(NextLevelPrefab);
        nextLevel.name = "Level" + (currentLevel + 1).ToString();
        nextLevel.transform.parent = GameObject.Find("Elevators").transform;
        nextLevel.transform.position = new Vector3(0, 9.6f, 0);
        Transform bridge = nextLevel.transform.Find("Bridge");

        switch (nextBridgeIndex)
        {
            case 4:
                bridge.localPosition = new Vector3(0, 3.12f, 0);
                break;
            case 3:
                bridge.localPosition = new Vector3(0, 1.2f, 0);
                break;
            case 2:
                bridge.localPosition = new Vector3(0, -0.72f, 0);
                break;
            case 1:
                bridge.localPosition = new Vector3(0, -2.64f, 0);
                break;
            case 0:
                bridge.localPosition = new Vector3(0, -4.56f, 0);
                break;
        }
    }

    IEnumerator ShiftDownLevels()
    {
        Debug.Log("Elevators/Level" + (currentLevel + 1).ToString());
        GreenLightPosition.gameObject.SetActive(false);
        changingLevels = true;
        GameObject currentLevelGroup = GameObject.Find("Elevators/Level" + currentLevel);
        GameObject nextLevelGroup =  GameObject.Find("Elevators/Level" + (currentLevel + 1).ToString());
        while (nextLevelGroup.transform.position.y > 0)
        {
            currentLevelGroup.transform.position = Vector3.MoveTowards(currentLevelGroup.transform.position, new Vector3(0, -9.6f, 0), 0.3f);
            nextLevelGroup.transform.position = Vector3.MoveTowards(nextLevelGroup.transform.position, new Vector3(0, 0, 0), 0.3f);
            Debug.Log("Stuck in the loop");
            yield return null;
        }
        currentLevel++;
        bridgeIndex = nextBridgeIndex;
        SpawnNextLevel();
        Destroy(currentLevelGroup);
        changingLevels = false;
        GreenLightPosition.gameObject.SetActive(true);
    }
}
