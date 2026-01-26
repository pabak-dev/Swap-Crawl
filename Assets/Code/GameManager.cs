using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Room 1 Setup")]
    public GameObject firstEnemy;
    public GameObject roadBlockToRoom2;
    public GameObject roadBlockToRoom1;

    [Header("Wave Setup")]
    public Transform[] spawnPoints;
    public GameObject[] enemyPrefabs;
    public KeyCode startWaveKey = KeyCode.F;
    public TextMeshProUGUI waveInfoText;

    private bool isRoom1Cleared = false;
    private bool isWavePhase = false;
    private int waveNumber = 1;
    [SerializeField] private ToolData[] allTools;

    private void Awake()
    {
        Instance = this;
        if (roadBlockToRoom1 != null) roadBlockToRoom1.SetActive(false);
        if (roadBlockToRoom2 != null) roadBlockToRoom2.SetActive(true);
        if (waveInfoText != null) waveInfoText.text = "";
    }

    private void Update()
    {
        if (!isRoom1Cleared && firstEnemy == null)
        {
            isRoom1Cleared = true;
            if (roadBlockToRoom2 != null) roadBlockToRoom2.SetActive(false);
        }

        if (isWavePhase)
        {
            if (Input.GetKeyDown(startWaveKey))
            {
                SpawnWave();
            }
        }
    }

    public void ActivateWavePhase()
    {
        if (isWavePhase) return;

        isWavePhase = true;
        
        if (roadBlockToRoom1 != null) roadBlockToRoom1.SetActive(true);
        if (roadBlockToRoom2 != null) roadBlockToRoom2.SetActive(false);

        UpdateWaveUI($"Ready! Press '{startWaveKey}' for Wave " + waveNumber);
    }

    private void SpawnWave()
    {
        int count = Mathf.CeilToInt(waveNumber * 1.5f) + Random.Range(1, 3);
        
        for (int i = 0; i < count; i++)
        {
            Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject enemyProto = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            GameObject currentEnemy = Instantiate(enemyProto, sp.position, Quaternion.identity);

            currentEnemy.GetComponent<EntityInventory>().currentTool = allTools[Random.Range(0, allTools.Length)];
        }

        waveNumber++;
        UpdateWaveUI("Wave " + (waveNumber - 1) + $" Spawned! Press '{startWaveKey}' for Next.");
    }

    private void UpdateWaveUI(string msg)
    {
        if (waveInfoText != null) waveInfoText.text = msg;
    }
}