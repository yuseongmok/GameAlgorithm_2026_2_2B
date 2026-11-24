using System.Collections.Generic;
using UnityEngine;

namespace MetroLine
{
    public static class MetroShapeMeshUtility
    {
        private static Mesh circleMesh;
        private static Mesh triangleMesh;
        private static Mesh squareMesh;

        public static void Apply(MetroStationView station)
        {
            MeshFilter filter = station.GetComponent<MeshFilter>();
            if (filter == null) return;

            filter.sharedMesh = station.Shape switch
            {
                MetroShape.Circle => circleMesh ??= CreatePrism(24, 0f, "Metro_Circle"),
                MetroShape.Triangle => triangleMesh ??= CreatePrism(3, Mathf.PI * 0.5f, "Metro_Triangle"),
                _ => squareMesh ??= CreatePrism(4, Mathf.PI * 0.25f, "Metro_Square")
            };
            station.transform.localScale = Vector3.one;
        }

        private static Mesh CreatePrism(int sides, float angleOffset, string meshName)
        {
            const float radius = 0.62f;
            const float halfHeight = 0.18f;
            var vertices = new List<Vector3>();
            for (int i = 0; i < sides; i++)
            {
                float angle = angleOffset + Mathf.PI * 2f * i / sides;
                vertices.Add(new Vector3(Mathf.Cos(angle) * radius, halfHeight, Mathf.Sin(angle) * radius));
            }
            for (int i = 0; i < sides; i++)
            {
                Vector3 top = vertices[i];
                vertices.Add(new Vector3(top.x, -halfHeight, top.z));
            }

            var triangles = new List<int>();
            for (int i = 1; i < sides - 1; i++)
            {
                triangles.Add(0);
                triangles.Add(i + 1);
                triangles.Add(i);
                triangles.Add(sides);
                triangles.Add(sides + i);
                triangles.Add(sides + i + 1);
            }
            for (int i = 0; i < sides; i++)
            {
                int next = (i + 1) % sides;
                triangles.Add(i);
                triangles.Add(next);
                triangles.Add(sides + next);
                triangles.Add(i);
                triangles.Add(sides + next);
                triangles.Add(sides + i);
            }

            var mesh = new Mesh { name = meshName };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
