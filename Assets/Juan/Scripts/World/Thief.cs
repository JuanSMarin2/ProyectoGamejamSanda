using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Thief : MonoBehaviour
{
    [SerializeField] private int patrolPointCount = 3;
    [SerializeField] private float patrolRadius = 2f;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 6f;
    [SerializeField] private GameObject thiefPanel;
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text itemText;
    [SerializeField] private float panelDuration = 3f;
    [SerializeField] private float moneyStealPercent = 10f;

    private Transform player;
    private NpcAnimatorController npcAnimator;

    private readonly List<Vector3> patrolPoints = new();
    private int currentWaypointIndex;
    private bool playerInVision;
    private bool chasing;
    private bool stealing;

    private void Awake()
    {
        npcAnimator = GetComponent<NpcAnimatorController>();

        if (thiefPanel != null)
            thiefPanel.SetActive(false);

        GeneratePatrolPoints();
    }

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;
    }

    private void Update()
    {
        if (stealing)
        {
            if (npcAnimator != null)
                npcAnimator.PlayIdle();

            return;
        }

        if (chasing && playerInVision && player != null)
        {
            MoveTowards(player.position, chaseSpeed);

            if (npcAnimator != null)
                npcAnimator.SetMoveDirection(player.position - transform.position);

            return;
        }

        if (chasing && !playerInVision)
            chasing = false;

        Patrol();
    }

    private void GeneratePatrolPoints()
    {
        patrolPoints.Clear();

        float sectorDegrees = 360f / patrolPointCount;
        float startAngle = Random.Range(0f, 360f);

        for (int i = 0; i < patrolPointCount; i++)
        {
            float angle = startAngle + sectorDegrees * i + Random.Range(0f, sectorDegrees);

            Vector2 offset = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            ) * patrolRadius;

            patrolPoints.Add(transform.position + (Vector3)offset);
        }
    }

    private void Patrol()
    {
        if (patrolPoints.Count == 0)
        {
            if (npcAnimator != null)
                npcAnimator.PlayIdle();

            return;
        }

        Vector3 target = patrolPoints[currentWaypointIndex];

        MoveTowards(target, patrolSpeed);

        if (npcAnimator != null)
            npcAnimator.SetMoveDirection(target - transform.position);

        if (Vector3.Distance(transform.position, target) < 0.05f)
            NextWaypoint();
    }

    private void NextWaypoint()
    {
        currentWaypointIndex = (currentWaypointIndex + 1) % patrolPoints.Count;
    }

    private void MoveTowards(Vector3 targetPosition, float speed)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        playerInVision = true;
        chasing = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            playerInVision = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!chasing || stealing)
            return;

        if (!collision.collider.CompareTag("Player"))
            return;

        StartCoroutine(StealRoutine());
    }

    private IEnumerator StealRoutine()
    {
        chasing = false;
        stealing = true;

        int stolenMoney = StealMoney();
        string stolenItemName = StealItem();

        if (stolenMoney > 0 || stolenItemName != null)
            PlayStealSound();

        ShowThiefPanel(stolenMoney, stolenItemName);

        yield return new WaitForSeconds(panelDuration);

        if (thiefPanel != null)
            thiefPanel.SetActive(false);

        stealing = false;
    }

    private int StealMoney()
    {
        if (MoneyData.Instance == null || MoneyData.Instance.Money <= 0)
            return 0;

        int stolenMoney = Mathf.RoundToInt(MoneyData.Instance.Money * (moneyStealPercent / 100f));

        if (stolenMoney <= 0)
            return 0;

        MoneyData.Instance.RemoveMoney(stolenMoney);

        return stolenMoney;
    }

    private string StealItem()
    {
        if (InventoryData.Instance == null || InventoryData.Instance.Items.Count == 0)
            return null;

        int randomIndex = Random.Range(0, InventoryData.Instance.Items.Count);

        ObjectData stolenItem = InventoryData.Instance.GetItem(randomIndex);

        if (stolenItem == null)
            return null;

        InventoryData.Instance.RemoveItem(randomIndex);

        return stolenItem.itemName;
    }

    private void ShowThiefPanel(int stolenMoney, string stolenItemName)
    {
        if (thiefPanel != null)
            thiefPanel.SetActive(true);

        if (moneyText != null)
            moneyText.text = "-$" + stolenMoney;

        if (itemText != null)
            itemText.text = stolenItemName != null ? "-" + stolenItemName : "-";

        Debug.Log($"[THIEF] Te robaron ${stolenMoney} y {(stolenItemName ?? "nada")}.");
    }

    private void PlayStealSound()
    {
        if (AudioManager.instance == null || FMODEvents.instance == null || FMODEvents.instance.slapRapar.IsNull)
            return;

        
        AudioManager.instance.PlayOneShot(FMODEvents.instance.layeringRobo, transform.position);
    }
}