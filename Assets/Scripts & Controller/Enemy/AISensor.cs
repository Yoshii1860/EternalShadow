using System.Collections;
using UnityEngine;

public class AISensor : MonoBehaviour, ICustomUpdatable
{
    #region Fields

    [Header("Detection Settings")]
    [SerializeField] private float _detectionRadius = 10;
    [SerializeField] private float _fieldOfViewAngle = 30;
    [SerializeField] private float _fieldOfViewHeight = 1.0f;
    [SerializeField] private Color _meshColor = Color.red;
    [SerializeField] private int _scanRate = 10;
    [SerializeField] private LayerMask _detectableLayers;
    [SerializeField] private LayerMask _obstacleLayers;
    public bool IsPlayerInSight = false;
    public bool IsPlayerHidden = false;
    [Space(10)]

    [Header("References")]
    [SerializeField] private Transform _headTransform;
    [SerializeField] private Transform _camRoot;
    [Space(10)]

    [SerializeField] private bool _debugMode = false;

    // Private Fields
    private UnityEngine.AI.NavMeshAgent _navMeshAgent;
    private Collider[] _detectedColliders = new Collider[10];
    private Mesh _detectionMesh;  

    private bool _savePlayerInSight = false;
    private bool _isPlayerAlreadyHidden = false;
    private bool _isPaused = false;
    
    private int _numDetected;
    private float _scanTimeRemaining;
    private float _scanTimer;

    #endregion




    #region MonoBehaviour Callbacks

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the scan timer
        _scanTimeRemaining = 1.0f / _scanRate;
        if (_camRoot == null)
        {
            _camRoot = GameManager.Instance.Player.transform.GetChild(0);
        }

        _navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }

    #endregion




    #region Custom Update

    // Custom Update method from ICustomUpdatable interface
    public void CustomUpdate(float deltaTime)
    {
        // Perform scan if the player is not hidden and the game is not paused
        if (!_isPaused && !IsPlayerHidden) 
        {
            ScanUpdate(deltaTime);
        }

        // If the player is hidden, call the Hidden method
        if (IsPlayerHidden)
        {
            Hidden();
        }
        else _isPlayerAlreadyHidden = false;
    }

    #endregion




    #region Public Methods

    public void PausePlayerInSight()
    {
        _savePlayerInSight = IsPlayerInSight;
        IsPlayerInSight = false;
        _isPaused = true;
    }

    public void ResumePlayerInSight()
    {
        IsPlayerInSight = _savePlayerInSight;
        _isPaused = false;
    }

    // Method to force the player in sight for a certain amount of time
    public void PlayerInSightForced(float time)
    {
        StartCoroutine(ForcedPlayerInSight(time));
    }

    #endregion




    #region Private Methods

    // Method to hide the player from the enemy
    private void Hidden()
    {
        if (!_isPlayerAlreadyHidden)
        {
            StartCoroutine(StopChase());
            if (_debugMode) Debug.Log("AISensor: Player IsPlayerHidden from enemy!");
        }
        else if (_debugMode) Debug.Log("AISensor: Player already IsPlayerHidden!");

        _isPlayerAlreadyHidden = true;
        IsPlayerInSight = false;
    }

    // Method to perform a scan for the player
    private void ScanUpdate(float deltaTime)
    {
        _scanTimer -= deltaTime;
        if (_scanTimer < 0) 
        {
            _scanTimer += _scanTimeRemaining;
            PerformScan();
        }
    }

    // Method to perform the scan
    private void PerformScan()
    {
        // Perform a sphere cast to detect the player
        _numDetected = Physics.OverlapSphereNonAlloc(transform.position, _detectionRadius, _detectedColliders, _detectableLayers, QueryTriggerInteraction.Collide);
        
        for (int i = 0; i < _numDetected; i++)
        {
            if (_detectedColliders[i].CompareTag("Player"))
            {
                CheckPlayerInFOV(_detectedColliders[i].transform.position);
                if (_debugMode) Debug.Log($"{_detectedColliders[i].gameObject.name} within sensing area!");
            }
        }
    }

    // Method to check if the player is within the field of view of the enemy
    private void CheckPlayerInFOV(Vector3 playerPosition)
    {
        // Calculate the direction to the player
        Vector3 directionToPlayer = playerPosition - transform.position;

        // Calculate the _fieldOfViewAngle between the forward direction of the sensor and the direction to the player
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        // Check if the player is within the FOV _fieldOfViewAngle
        if (angleToPlayer <= _fieldOfViewAngle)
        {
            bool lineOfSight = Physics.Linecast(_headTransform.position, _camRoot.position, _obstacleLayers);
            Debug.DrawLine(_headTransform.position, _camRoot.position, lineOfSight ? Color.red : Color.green, 0.5f);

            // Perform line of sight check
            if (!lineOfSight)
            {
                IsPlayerInSight = true;
                if (_debugMode) Debug.Log($"PlayerInSight: {IsPlayerInSight} from {gameObject.name}");
                return;
            }
            else 
            {
                if (_debugMode) Debug.Log($"Player not in LOS of {gameObject.name}");
                IsPlayerInSight = false;
            }
        }
        else IsPlayerInSight = false;
    }

    #endregion




    #region Coroutines

    // Coroutine to force the player in sight for a certain amount of time
    private IEnumerator ForcedPlayerInSight(float time)
    {
        _savePlayerInSight = IsPlayerInSight;
        while (time > 0)
        {
            time -= Time.deltaTime;
            IsPlayerInSight = true;
            yield return null;
        }
        IsPlayerInSight = _savePlayerInSight;
    }

    // Coroutine to stop the chase of the enemy
    private IEnumerator StopChase()
    {
        if (_debugMode) Debug.Log("AISensor: Stopping chase");
        _navMeshAgent.SetDestination(transform.position);
        _navMeshAgent.isStopped = true;

        // Lerp the position of the enemy back to the last position to prevent sliding
        Vector3 lastPosition = transform.position;
        float elpasedTime = 0;
        while (elpasedTime < 1.0f)
        {
            if (_debugMode) Debug.Log("AISensor: Stopping chase");
            elpasedTime += Time.deltaTime;
            transform.position = lastPosition;
            yield return null;
        }

        _navMeshAgent.isStopped = false;
    }

    #endregion




    #region Mesh Generation

    // Method to create a wedge mesh for the sensor
    private Mesh CreateWedgeMesh() 
    {
        Mesh _detectionMesh = new Mesh();

        int segments = 10;
        int numTriangles = (segments * 4) + 2 + 2;
        int numVertices = numTriangles * 3;

        Vector3[] vertices = new Vector3[numVertices];
        int[] triangles = new int[numVertices];

        Vector3 bottomCenter = Vector3.zero;
        Vector3 bottomLeft = Quaternion.Euler(0, -_fieldOfViewAngle, 0) * Vector3.forward * _detectionRadius;
        Vector3 bottomRight = Quaternion.Euler(0, _fieldOfViewAngle, 0) * Vector3.forward * _detectionRadius;

        Vector3 topCenter = bottomCenter + Vector3.up * _fieldOfViewHeight;
        Vector3 topLeft = bottomLeft + Vector3.up * _fieldOfViewHeight;
        Vector3 topRight = bottomRight + Vector3.up * _fieldOfViewHeight;

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

        float currentAngle = -_fieldOfViewAngle;
        float deltaAngle = (_fieldOfViewAngle * 2) / segments;
        for (int i = 0; i < segments; i++)
        {
            bottomLeft = Quaternion.Euler(0, currentAngle, 0) * Vector3.forward * _detectionRadius;
            bottomRight = Quaternion.Euler(0, currentAngle + deltaAngle, 0) * Vector3.forward * _detectionRadius;

            topLeft = bottomLeft + Vector3.up * _fieldOfViewHeight;
            topRight = bottomRight + Vector3.up * _fieldOfViewHeight;

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

        _detectionMesh.vertices = vertices;
        _detectionMesh.triangles = triangles;
        _detectionMesh.RecalculateNormals();

        return _detectionMesh;
    }

    #endregion




    #region Unity Callbacks

    void OnValidate() 
    {
        _detectionMesh = CreateWedgeMesh();
        _scanTimeRemaining = 1.0f / _scanRate;
    }

    void OnDrawGizmos() 
    {
        //DrawSensorMesh();
        DrawDetectionRadius();
    }

    private void DrawSensorMesh()
    {
        if (_detectionMesh) 
        {
            Gizmos.color = _meshColor;
            Gizmos.DrawMesh(_detectionMesh, transform.position, transform.rotation);
        }
    }

    private void DrawDetectionRadius()
    {
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);
    }

    #endregion
}
