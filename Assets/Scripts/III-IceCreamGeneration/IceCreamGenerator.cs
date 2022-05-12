using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceCreamGenerator : MonoBehaviour
{
    // Radius, height and resolution parameters
    public float radius = 1.5f;
    public float height = 6.0f;
    [Range(0, 250)]
    public int revolutionResolution = 40;

    // 'prev' variables to detect changes over time on original variables
    float prevRadius;
    float prevHeight;
    int prevRes;

    // Texture to apply to the generated mesh
    public Texture2D texture;

    // Boolean used to choose to skip resolution that triggers UVs artefacts or not
    public bool avoidProblematicResolutions = true;
    

    void Start()
    {
        // Initialize the 'prev' variables with parameters initial values
        prevRadius = radius;
        prevHeight = height;
        prevRes = revolutionResolution;

        // Generate the ice cream cornet with initial radius, height and resolution parameters
        GenerateIceCream(radius, height, revolutionResolution);
    }


    void Update()
    {
        // Checking if any parameter has been changed
        if (prevRadius != radius
            || prevHeight != height
            || prevRes != revolutionResolution)
        {
            // Skip problematic resolutions (keep only power of 2 resolutions) if asked to
            if (avoidProblematicResolutions && revolutionResolution % 2 == 1)
                revolutionResolution--;

            // Generate a new mesh with the new parameters
            GenerateIceCream(radius, height, revolutionResolution);

            // Update the 'prev' variables
            prevRadius = radius;
            prevHeight = height;
            prevRes = revolutionResolution;
        }
    }

    // This function generates an ice cream cornet mesh and apply it to the MeshFilter of that GameObject
    // TODO: Triangles and UVs are being calculated, but vertices are missing. Complete this function with vertices calculation
    // to fully generate the ice cream cornet
    public void GenerateIceCream(float radius, float height, int resolution)
    {
        // Mesh generation
        Mesh mesh = new Mesh();

        // TODO: Add vertices calculations
        #region Vertices calculation
        Vector3[] vertices = new Vector3[resolution + 2];
        int[] tris = new int[resolution * 2 * 3];
        Vector2[] uvs = new Vector2[resolution + 2];

        uvs[resolution] = new Vector2(0.5f, 0);
        uvs[resolution + 1] = new Vector2(0.5f, 1);
        for(int i = 0; i < resolution; i++)
        {
            float theta = i * 2 * Mathf.PI / resolution;

            // Triangles calcuation
            tris[6 * i] = resolution;
            tris[6 * i + 1] = i;
            tris[6 * i + 2] = ((i + 1) % resolution);

            tris[6 * i + 3] = resolution + 1;
            tris[6 * i + 4] = ((i + 1) % resolution);
            tris[6 * i + 5] = i;
                
            // UVs calculation
            uvs[i] = new Vector2(Mathf.Abs(i / (float) resolution - 0.5f)+0.5f, 0.8f);
        }
        #endregion
        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.uv = uvs;

        mesh.RecalculateNormals();

        // If not already done previously, add a MeshFilter and MeshRenderer on this gameObject
        // Apply the standard material to it and the donut texture
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        if (!meshFilter)
        {
            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
            meshRenderer.sharedMaterial.mainTexture = texture;

            meshFilter = gameObject.AddComponent<MeshFilter>();
        }

        // Apply the newly created mesh to the MeshFilter
        meshFilter.mesh = mesh;
    }

}
