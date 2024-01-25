using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISensor : MonoBehaviour, ICustomUpdatable
{
    // FOV variables
    public float distance = 10;
    public float angle = 30;
    public float height = 1.0f;
    public Color meshColor = Color.red;
    public int scanFrequency = 10;
    public LayerMask layers;
    public LayerMask occlusionLayers;
    public bool playerInSight = false;

    Collider[] colliders = new Collider[10];
    Mesh mesh;
    int count;
    float scanInterval;
    float scanTimer;

    void Start()
    {
        scanInterval = 1.0f / scanFrequency;
    }

    public void CustomUpdate(float deltaTime)
    {
        scanTimer -= deltaTime;
        if (scanTimer < 0) 
        {
            scanTimer += scanInterval;
            Scan();
        }
    }

    private void Scan()
    {
        count = Physics.OverlapSphereNonAlloc(transform.position, distance, colliders, layers, QueryTriggerInteraction.Collide);
        
        for (int i = 0; i < count; i++)
        {
            Debug.Log("Scan: " + colliders[i].gameObject.name);
            if (colliders[i].gameObject.CompareTag("Player"))
            {
                // Calculate the direction to the player
                Vector3 directionToPlayer = colliders[i].transform.position - transform.position;

                // Calculate the angle between the forward direction of the sensor and the direction to the player
                float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

                // Check if the player is within the FOV angle
                if (angleToPlayer <= angle)
                {
                    // Perform line of sight check
                    if (Physics.Linecast(transform.position, colliders[i].transform.position, occlusionLayers))
                    {
                        Debug.Log("PlayerInSight: " + playerInSight + " from " + gameObject.name);
                        playerInSight = true;
                        return;
                    }
                }
            }
        }
        playerInSight = false;
    }

    Mesh CreateWedgeMesh() 
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

    void OnValidate() 
    {
        mesh = CreateWedgeMesh();
        scanInterval = 1.0f / scanFrequency;
    }

    void OnDrawGizmos() 
    {
        if (mesh) 
        {
            Gizmos.color = meshColor;
            Gizmos.DrawMesh(mesh, transform.position, transform.rotation);
        }

        Gizmos.DrawWireSphere(transform.position, distance);
    }
}
