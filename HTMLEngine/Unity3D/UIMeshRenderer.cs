using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
[RequireComponent(typeof(CanvasRenderer))]
public class UIMeshRenderer : MonoBehaviour
{
    public Material material1;
    public Material material2;
    public Material materialFont;
    public float cachedScale = 100;

    private CanvasRenderer canvasRenderer;
    private Mesh m_mesh;

    public void Awake()
    {
        canvasRenderer = GetComponent<CanvasRenderer>();
    }

    public void Start()
    {
        canvasRenderer.Clear();

        canvasRenderer.materialCount = 4;
        Material font = Resources.Load("fonts/code16", typeof(Material)) as Material;
        Material material = Resources.Load("atlases/smiles", typeof(Material)) as Material;
        Material materia2 = Resources.Load("atlases/faces", typeof(Material)) as Material;
        canvasRenderer.SetMaterial(material, 0);
        canvasRenderer.SetMaterial(material, 1);
        canvasRenderer.SetMaterial(materia2, 2);
        canvasRenderer.SetMaterial(materia2, 3);

        InitMesh();

        canvasRenderer.SetMesh(m_mesh);
    }

    void InitMesh()
    {
        m_mesh = new Mesh();
        m_mesh.name = "iMesh";

        // 为网格创建顶点数组
        List<int> vertNum = new List<int>();
        vertNum.Add(0);
        List<Vector3> vertices = new List<Vector3>();

        vertices.AddRange(GetSmileVertices(Vector3.zero));
        vertNum.Add(vertices.Count);
        vertices.AddRange(GetQuadVertices(Vector3.zero));
        vertNum.Add(vertices.Count);
        vertices.AddRange(GetSmileVertices(new Vector3(0, cachedScale, 0)));
        vertNum.Add(vertices.Count);
        vertices.AddRange(GetQuadVertices(new Vector3(0, -cachedScale, 0)));
        vertNum.Add(vertices.Count);

        m_mesh.vertices = vertices.ToArray();

        // 三角形
        m_mesh.subMeshCount = 4;
        m_mesh.SetTriangles(GetSmileTriangles(vertNum[0]), 0);
        m_mesh.SetTriangles(GetSmileTriangles(vertNum[2]), 1);
        m_mesh.SetTriangles(GetQuadTriangles(vertNum[1]), 2);
        m_mesh.SetTriangles(GetQuadTriangles(vertNum[3]), 3);

        // 为mesh设置纹理贴图坐标
        List<Vector2> uv = new List<Vector2>();
        uv.AddRange(GetSmileUV());
        uv.AddRange(GetQuadUV());
        uv.AddRange(GetSmileUV());
        uv.AddRange(GetQuadUV());
        m_mesh.uv = uv.ToArray();
    }

    Vector3[] GetSmileVertices(Vector3 offset)
    {
        Vector3[] vertices = new Vector3[4 * 4]{
            new Vector3(-cachedScale*2, 0, 0) + offset,
            new Vector3(-cachedScale*2, cachedScale, 0) + offset,
            new Vector3(-cachedScale, 0, 0) + offset,
            new Vector3(-cachedScale, cachedScale, 0) + offset,
            new Vector3(-cachedScale, 0, 0) + offset,
            new Vector3(-cachedScale, cachedScale, 0) + offset,
            new Vector3(0, 0, 0) + offset,
            new Vector3(0, cachedScale, 0) + offset,
            new Vector3(0, 0, 0) + offset,
            new Vector3(0, cachedScale, 0) + offset,
            new Vector3(cachedScale, 0, 0) + offset,
            new Vector3(cachedScale, cachedScale, 0) + offset,
            new Vector3(cachedScale, 0, 0) + offset,
            new Vector3(cachedScale, cachedScale, 0) + offset,
            new Vector3(cachedScale*2, 0, 0) + offset,
            new Vector3(cachedScale*2, cachedScale, 0) + offset,
        };
        return vertices;
    }

    int[] GetSmileTriangles(int startIdx)
    {
        int s = startIdx;
        int[] triangles = new int[2 * 4 * 3]{
            s+0, s+1, s+2, s+2, s+1, s+3,
            s+4, s+5, s+6, s+6, s+5, s+7,
            s+8, s+9, s+10, s+10, s+9, s+11,
            s+12, s+13, s+14, s+14, s+13, s+15,
        };
        return triangles;
    }

    Vector2[] GetSmileUV()
    {
        Vector2[] uv = new Vector2[4 * 4]{
            new Vector2(0, 0),
            new Vector2(0, 0.5f),
            new Vector2(0.5f, 0),
            new Vector2(0.5f, 0.5f),
            new Vector2(0, 0.5f),
            new Vector2(0, 1),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 1),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 1),
            new Vector2(1, 0.5f),
            new Vector2(1, 1),
            new Vector2(0.5f, 0),
            new Vector2(0.5f, 0.5f),
            new Vector2(1, 0),
            new Vector2(1, 0.5f),
        };
        return uv;
    }

    Vector3[] GetQuadVertices(Vector3 offset)
    {
        Vector3[] vertices = new Vector3[4]{
            new Vector3(-cachedScale, -cachedScale, 0) + offset,
            new Vector3(-cachedScale, 0, 0) + offset,
            new Vector3(0, 0, 0) + offset,
            new Vector3(0, -cachedScale, 0) + offset,
        };
        return vertices;
    }

    int[] GetQuadTriangles(int startIdx)
    {
        int s = startIdx;
        int[] triangles = new int[2 * 3]{
            s+0, s+1, s+2,
            s+2, s+3, s+0,
        };
        return triangles;
    }

    Vector2[] GetQuadUV()
    {
        Vector2[] uv = new Vector2[4]{
            new Vector2(0, 0),
            new Vector2(0, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, 0),
        };
        return uv;
    }

    //private void OnEnable()
    //{
    //    canvasRenderer.materialCount = 2;
    //    canvasRenderer.SetMaterial(material1, 0);
    //    canvasRenderer.SetMaterial(material2, 1);
    //    canvasRenderer.SetMesh(mesh);
    //}

    //public void OnDisable()
    //{
    //    canvasRenderer.SetMaterial(material2, null);
    //    canvasRenderer.SetMesh(mesh);
    //}

    //public void Update()
    //{
    //    if (scale != cachedScale)
    //    {
    //        cachedScale = scale;
    //        SetVertices(mesh);
    //        canvasRenderer.SetMesh(mesh);
    //    }
    //}

    //    public List<AnimeVertex> ConvertMesh()
    //    {
    //        Vector3[] vertices = mesh.vertices;
    //        int[] triangles = mesh.triangles;
    //        Vector3[] normals = mesh.normals;
    //        Vector2[] uv = mesh.uv;

    //        List<AnimeVertex> vertexList = new List<AnimeVertex>(triangles.Length);

    //        AnimeVertex vertex;
    //        for (int i = 0; i < triangles.Length; i++)
    //        {
    //            vertex = new AnimeVertex();
    //            int index = triangles[i];

    //            vertex.position = ((vertices[index] - mesh.bounds.center) * scale);
    //            vertex.uv0 = uv[index];
    //            vertex.normal = normals[index];

    //            vertexList.Add(vertex);

    //            if (i % 3 == 0)
    //                vertexList.Add(vertex);
    //        }

    //        return vertexList;
    //    }
}