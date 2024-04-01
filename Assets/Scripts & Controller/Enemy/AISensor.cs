using System.Collections;
using UnityEngine;

public class AISensor : MonoBehaviour, ICustomUpdatable
{
    #region Fields

    // FOV variables
    public float distance = 10;
    public float angle = 30;
    public float height = 1.0f;
    public Color meshColor = Color.red;
    public int scanFrequency = 10;
    public LayerMask layers;
    public LayerMask occlusionLayers;
    public bool playerInSight = false;
    bool savePlayerInSight = false;
    bool paused = false;

    [SerializeField] private Transform head;
    [SerializeField] private Transform camRoot;
    private Collider[] colliders = new Collider[10];
    private Mesh mesh;  
    private int count;
    private float scanInterval;
    private float scanTimer;

    private Enemy enemy;
    private UnityEngine.AI.NavMeshAgent agent;

    private Coroutine coroutine;

    public bool hidden = false;
    bool once = false;

    #endregion

    #region MonoBehaviour Callbacks

    void Start()
    {
        scanInterval = 1.0f / scanFrequency;
        if (camRoot == null)
        {
            camRoot = GameManager.Instance.player.transform.GetChild(0);
        }

        enemy = GetComponent<Enemy>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }

    #endregion

    #region ICustomUpdatable Implementation

    public void CustomUpdate(float deltaTime)
    {
        if (!paused && !hidden) 
        {
            ScanUpdate(deltaTime);
        }

        if (hidden)
        {
            Hidden();
        }
        else once = false;
    }

    #endregion

    #region Private Methods

    private void Hidden()
    {
        if (!once)
        {
            StartCoroutine(StopChase());
            if (gameObject.activeSelf) Debug.Log("AISensor: Player hidden from enemy!");
        }
        once = true;
        playerInSight = false;
    }

    private void ScanUpdate(float deltaTime)
    {
        scanTimer -= deltaTime;
        if (scanTimer < 0) 
        {
            scanTimer += scanInterval;
            PerformScan();
        }
    }

    private void PerformScan()
    {
        count = Physics.OverlapSphereNonAlloc(transform.position, distance, colliders, layers, QueryTriggerInteraction.Collide);
        
        for (int i = 0; i < count; i++)
        {
            if (colliders[i].CompareTag("Player"))
            {
                CheckPlayerInFOV(colliders[i].transform.position);
                if (gameObject.activeSelf) Debug.Log($"{colliders[i].gameObject.name} within sensing area!");
            }
        }
    }

    private void CheckPlayerInFOV(Vector3 playerPosition)
    {
        // Calculate the direction to the player
        Vector3 directionToPlayer = playerPosition - transform.position;

        // Calculate the angle between the forward direction of the sensor and the direction to the player
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        // Check if the player is within the FOV angle
        if (angleToPlayer <= angle)
        {
            bool lineOfSight = Physics.Linecast(head.position, camRoot.position, occlusionLayers);
            Debug.DrawLine(head.position, camRoot.position, lineOfSight ? Color.red : Color.green, 0.5f);
            // Perform line of sight check
            if (!lineOfSight)
            {
                playerInSight = true;
                if (gameObject.activeSelf) Debug.Log($"PlayerInSight: {playerInSight} from {gameObject.name}");
                return;
            }
            else 
            {
                if (gameObject.activeSelf) Debug.Log($"Player not in LOS of {gameObject.name}");
                playerInSight = false;
            }
        }
        {
            playerInSight = false;
        }
    }

    public void PausePlayerInSight()
    {
        savePlayerInSight = playerInSight;
        playerInSight = false;
        paused = true;
    }

    public void ResumePlayerInSight()
    {
        playerInSight = savePlayerInSight;
        paused = false;
    }

    public void PlayerInSightForced(float time)
    {
        StartCoroutine(ForcedPlayerInSight(time));
    }

    IEnumerator ForcedPlayerInSight(float time)
    {
        savePlayerInSight = playerInSight;
        while (time > 0)
        {
            time -= Time.deltaTime;
            playerInSight = true;
            yield return null;
        }
        playerInSight = savePlayerInSight;
    }

    IEnumerator StopChase()
    {
        agent.SetDestination(transform.position);
        agent.isStopped = true;
        Vector3 lastPosition = transform.position;
        float elpasedTime = 0;
        while (elpasedTime < 1.0f)
        {
            Debug.Log("Stopping chase");
            elpasedTime += Time.deltaTime;
            transform.position = lastPosition;
            yield return null;
        }
        agent.isStopped = false;
    }

    private Mesh CreateWedgeMesh() 
    {
        Mesh mesh = new Mesh();

        int segments = 10;
        int numTriangles = (segments * 4) + 2 + 2;
        int numVertices = numTriangles * 3;

        Vector3[] vertices = new Vector3[numVertices];
        int[] triangles = new int[numVertices];

        Vector3 bottomCenter = Vector3.zero;
        Vector3 bottomLeft = Quaternion.Euler(0, -angle, 0) * Vector3.forward * distance;
        Vector3 bottomRight = Quaternion.Euler(0, angle, 0) * Vector3.forward * distance;

        Vector3 topCenter = bottomCenter + Vector3.up * height;
        Vector3 topLeft = bottomLeft + Vector3.up * height;
        Vector3 topRight = bottomRight + Vector3.up * height;

        int vert = 0;

        // left side
        vertices[vert++] = bottomCenter;
        vertices[vert++] = bottomLeft;
        vertices[vert++] = topLeft;

        vertices[vert++] = topLeft;
        vertices[vert++] = topCenter;
        vertices[vert++] = bottomCenter;

        // right side
        vertices[vert++] = bottomCenter;
        vertices[vert++] = topCenter;
        vertices[vert++] = topRight;

        vertices[vert++] = topRight;
        vertices[vert++] = bottomRight;
        vertices[vert++] = bottomCenter;

        float currentAngle = -angle;
        float deltaAngle = (angle * 2) / segments;
        for (int i = 0; i < segments; i++)
        {
            bottomLeft = Quaternion.Euler(0, currentAngle, 0) * Vector3.forward * distance;
            bottomRight = Quaternion.Euler(0, currentAngle + deltaAngle, 0) * Vector3.forward * distance;

            topLeft = bottomLeft + Vector3.up * height;
            topRight = bottomRight + Vector3.up * height;

            // far side
            vertices[vert++] = bottomLeft;
            vertices[vert++] = bottomRight;
            vertices[vert++] = topRight;

            vertices[vert++] = topRight;
            vertices[vert++] = topLeft;
            vertices[vert++] = bottomLeft;

            // top
            vertices[vert++] = topCenter;
            vertices[vert++] = topLeft;
            vertices[vert++] = topRight;

            // bottom
            vertices[vert++] = bottomCenter;
            vertices[vert++] = bottomRight;
            vertices[vert++] = bottomLeft;

            currentAngle += deltaAngle;
        }

        for(int i = 0; i < numVertices; i++) 
        {
            triangles[i] = i;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    #endregion

    #region Unity Callbacks

    void OnValidate() 
    {
        mesh = CreateWedgeMesh();
        scanInterval = 1.0f / scanFrequency;
    }

    void OnDrawGizmos() 
    {
        //DrawSensorMesh();
        DrawDetectionRadius();
    }

    private void DrawSensorMesh()
    {
        if (mesh) 
        {
            Gizmos.color = meshColor;
            Gizmos.DrawMesh(mesh, transform.position, transform.rotation);
        }
    }

    private void DrawDetectionRadius()
    {
        Gizmos.DrawWireSphere(transform.position, distance);
    }

    #endregion
}
