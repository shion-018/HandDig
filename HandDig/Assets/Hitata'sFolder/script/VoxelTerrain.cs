using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider), typeof(MeshRenderer))]
public class VoxelTerrain : MonoBehaviour
{
    public int size = 16;
    public float digRadius = 2f;
    private VoxelChunk chunk;

    private void Start()
    {
        chunk = new VoxelChunk(size);
        RebuildMesh();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 localPos = transform.InverseTransformPoint(hit.point);
                chunk.Dig(localPos, digRadius);
                RebuildMesh();
            }
        }
    }

    private void RebuildMesh()
    {
        Mesh mesh = MarchingCubes.GenerateMesh(chunk.density);
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = null;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
