using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UniqueIDComponent))]
public class EnemyFOVVisualizer : MonoBehaviour
{
    public Color gizmoColor = Color.red;

    // Draw the FOV visualization for a specific enemy
    public void DrawFOVVisualization(Transform enemyTransform, float fovRange, float fovAngle, int rayCount, float meshHeight)
    {
        Gizmos.color = gizmoColor;

        // Calculate the start direction of the FOV cone
        Vector3 startDirection = Quaternion.Euler(0f, -fovAngle * 0.5f, 0f) * enemyTransform.forward;

        // Calculate the angle between each ray
        float angleIncrement = fovAngle / rayCount;

        // Draw the mesh for the FOV cone
        DrawFOVVisualizer(enemyTransform.position, startDirection, fovRange, fovAngle, angleIncrement, rayCount, meshHeight);
    }

    private void DrawFOVVisualizer(Vector3 position, Vector3 startDirection, float fovRange, float fovAngle, float angleIncrement, int rayCount, float meshHeight)
    {
        for (int i = 0; i <= rayCount; i++)
        {
            // Calculate the current ray direction based on the current angle
            Vector3 rayDirection = Quaternion.Euler(0f, angleIncrement * i, 0f) * startDirection;

            // Cast a ray to check for obstacles in the FOV
            RaycastHit hit;
            if (Physics.Raycast(position, rayDirection, out hit, fovRange))
            {
                // If the ray hits something, draw the ray only up to the hit point
                Gizmos.DrawLine(position, hit.point);

                // Create a mesh for the FOV cone
                Mesh coneMesh = CreateConeMesh(Vector3.Distance(position, hit.point), meshHeight, fovAngle, angleIncrement * i, rayCount);

                // Draw the mesh using Gizmos.DrawMesh
                Gizmos.DrawMesh(coneMesh, position, Quaternion.LookRotation(rayDirection), Vector3.one);
            }
            else
            {
                // If the ray doesn't hit anything, draw the full FOV range
                Gizmos.DrawRay(position, rayDirection * fovRange);

                // Create a mesh for the FOV cone
                Mesh coneMesh = CreateConeMesh(fovRange, meshHeight, fovAngle, angleIncrement * i, rayCount);

                // Draw the mesh using Gizmos.DrawMesh
                Gizmos.DrawMesh(coneMesh, position, Quaternion.LookRotation(rayDirection), Vector3.one);
            }
        }
    }

    private Mesh CreateConeMesh(float radius, float height, float fovAngle, float angle, int rayCount)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // Add the center vertex
        vertices.Add(Vector3.zero);

        // Calculate the vertices for the base of the cone
        for (int i = 0; i < rayCount; i++)
        {
            float x = Mathf.Cos(Mathf.Deg2Rad * (angle + i * fovAngle / rayCount)) * radius;
            float z = Mathf.Sin(Mathf.Deg2Rad * (angle + i * fovAngle / rayCount)) * radius;
            Vector3 vertex = new Vector3(x, 0f, z);
            vertices.Add(vertex);
        }

        // Add the top vertex
        vertices.Add(new Vector3(0f, height, 0f));

        // Create triangles for the base of the cone
        for (int i = 1; i < rayCount; i++)
        {
            triangles.Add(0);
            triangles.Add(i);
            triangles.Add(i + 1);
        }

        triangles.Add(0);
        triangles.Add(rayCount);
        triangles.Add(1);

        // Create triangles for the sides of the cone
        for (int i = 1; i <= rayCount; i++)
        {
            triangles.Add(i);
            triangles.Add((i % rayCount) + 1);
            triangles.Add(rayCount + 1);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        return mesh;
    }
}