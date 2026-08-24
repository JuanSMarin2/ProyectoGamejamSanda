using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoliceOfficer : MonoBehaviour
{
    [SerializeField] private List<Transform> waypoints = new();
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

    private int currentWaypointIndex;
    private bool playerInVision;
    private bool chasing;
    private bool punishing;

    private void Awake()
    {
        npcAnimator = GetComponent<NpcAnimatorController>();

        if (policePunishPanel != null)
            policePunishPanel.SetActive(false);
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
            MoveTowards(player.position, chaseSpeed);

            if (npcAnimator != null)
                npcAnimator.SetMoveDirection(player.position - transform.position);

            return;
        }

        Patrol();
    }

    private void Patrol()
    {
        if (waypoints.Count == 0)
        {
            if (npcAnimator != null)
                npcAnimator.PlayIdle();

            return;
        }

        Transform target = waypoints[currentWaypointIndex];

        if (target == null)
        {
            NextWaypoint();
            return;
        }

        MoveTowards(target.position, patrolSpeed);

        if (npcAnimator != null)
            npcAnimator.SetMoveDirection(target.position - transform.position);

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
            NextWaypoint();
    }

    private void NextWaypoint()
    {
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
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
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            playerInVision = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!chasing || punishing)
            return;

        if (!collision.collider.CompareTag("Player"))
            return;

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

        Debug.Log("[POLICE] Te han visto rebuscando. El policía va a por ti.");
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