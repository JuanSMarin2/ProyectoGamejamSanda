using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoliceOfficer : MonoBehaviour
{
    [SerializeField] private int patrolPointCount = 3;
    [SerializeField] private float patrolRadius = 2f;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 6f;
    [SerializeField] private float punishDuration = 10f;
    [SerializeField] private GameObject policePunishPanel;

    private static readonly List<PoliceOfficer> activeOfficers = new();

    public static bool IsPlayerInVision
    {
        get
        {
            foreach (PoliceOfficer officer in activeOfficers)
            {
                if (officer.playerInVision)
                    return true;
            }

            return false;
        }
    }

    private Transform player;
    private PlayerMovement playerMovement;
    private NpcAnimatorController npcAnimator;

    private readonly List<Vector3> patrolPoints = new();
    private int currentWaypointIndex;
    private bool playerInVision;
    private bool chasing;
    private bool punishing;
    private int thievesInVision;

    private void Awake()
    {
        npcAnimator = GetComponent<NpcAnimatorController>();

        if (policePunishPanel != null)
            policePunishPanel.SetActive(false);

        GeneratePatrolPoints();
    }

    private void OnEnable()
    {
        activeOfficers.Add(this);
    }

    private void OnDisable()
    {
        activeOfficers.Remove(this);
    }

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
            playerMovement = playerObject.GetComponent<PlayerMovement>();
        }
    }

    private void Update()
    {
        if (punishing)
        {
            if (npcAnimator != null)
                npcAnimator.PlayIdle();

            return;
        }

        if (chasing && player != null)
        {
            if (thievesInVision > 0)
            {
                StopChase();
                Debug.Log("[POLICE] El policía vio a un ladrón y te dejó de perseguir.");

                return;
            }

            if (!playerInVision)
            {
                StopChase();
                Debug.Log("[POLICE] Saliste de su visión: el policía te dejó de perseguir.");

                return;
            }

            MoveTowards(player.position, chaseSpeed);

            if (npcAnimator != null)
                npcAnimator.SetMoveDirection(player.position - transform.position);

            return;
        }

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
        if (collision.CompareTag("Player"))
            playerInVision = true;

        if (collision.GetComponentInParent<Thief>() != null)
            thievesInVision++;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            playerInVision = false;

        if (collision.GetComponentInParent<Thief>() != null)
            thievesInVision = Mathf.Max(0, thievesInVision - 1);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!chasing || punishing)
            return;

        if (!collision.collider.CompareTag("Player"))
            return;

        AudioManager.instance.PlayOneShot(FMODEvents.instance.error, transform.position);
        StartCoroutine(PunishRoutine());
    }

    public static void ReportCrime()
    {
        foreach (PoliceOfficer officer in activeOfficers)
        {
            if (officer.playerInVision)
            {
                officer.StartChase();
                return;
            }
        }
    }

    private void StartChase()
    {
        if (chasing || punishing)
            return;

        chasing = true;
        AudioManager.instance.PlayOneShot(FMODEvents.instance.sirenaPolicia, transform.position);
        Debug.Log("[POLICE] Te han visto rebuscando. El policía va a por ti.");
    }

    private void StopChase()
    {
        chasing = false;
    }

    private IEnumerator PunishRoutine()
    {
        chasing = false;
        punishing = true;

        if (playerMovement != null)
            playerMovement.SetMovementEnabled(false);

        if (policePunishPanel != null)
            policePunishPanel.SetActive(true);

        yield return new WaitForSeconds(punishDuration);

        if (policePunishPanel != null)
            policePunishPanel.SetActive(false);

        if (playerMovement != null)
            playerMovement.SetMovementEnabled(true);

        punishing = false;

        Debug.Log("[POLICE] Castigo terminado.");
    }
}