using UnityEngine;
using System.Collections.Generic;


namespace HTMLEngine
{
    public class ChunkDrawer : PoolableObject
    {
        /// <summary>
        /// is anime chunk or not
        /// </summary>
        public bool isAnimeChunk;
        /// <summary>
        /// HtmlEngine计算出来的相对rect，以左上角为原点，向下向右增长
        /// </summary>
        public HtRect rect;
        /// <summary>
        /// the position of the chunk vertices
        /// </summary>
        public readonly List<Vector3> position = new List<Vector3>();
        /// <summary>
        /// the color of the chunk vertices
        /// </summary>
        public readonly List<Color32> color = new List<Color32>();
        /// <summary>
        /// the uv0 of the chunk vertices
        /// </summary>
        public readonly List<Vector2> uv = new List<Vector2>();

        internal override void OnAcquire() { }

        internal override void OnRelease()
        {
            isAnimeChunk = false;
            position.Clear();
            color.Clear();
            uv.Clear();
        }
    }

    public class AnimeChunkDrawer : ChunkDrawer
    {
        /// <summary>
        /// the index in the mesh UV list
        /// </summary>
        public int startIdx;
        /// <summary>
        /// the frame time interval
        /// </summary>
        public float deltaTime;
        /// <summary>
        /// parent uv list
        /// </summary>
        public DrawDevice drawDevice;
        /// <summary>
        /// frame-uvs
        /// </summary>
        public Vector2[][] frameUVs;

        /// <summary>
        /// current frame index
        /// </summary>
        private int m_frameIndex;
        /// <summary>
        /// accumulated time
        /// </summary>
        private float m_elapseTime;

        public void Update()
        {
            if (frameUVs == null || drawDevice == null)
            {
                return;
            }
            m_elapseTime += Time.deltaTime;
            if (m_elapseTime > deltaTime)
            {
                m_elapseTime = 0;
                m_frameIndex %= frameUVs.Length;
                for (int i = 0; i < frameUVs[m_frameIndex].Length; i++)
                {
                    drawDevice.uvs[startIdx + i] = frameUVs[m_frameIndex][i];
                    drawDevice.isUVDirty = true;
                }
                m_frameIndex++;
            }
        }

        internal override void OnAcquire() { }

        internal override void OnRelease()
        {
            deltaTime = 0f;
            m_elapseTime = 0f;
            startIdx = 0;
            m_frameIndex = 0;
            drawDevice = null;
            frameUVs = null;
            base.OnRelease();
        }
    }

    public struct MaterialChunkDrawers
    {
        /// <summary>
        /// the material of the chunk
        /// </summary>
        public Material material;
        /// <summary>
        /// the material of the chunk
        /// </summary>
        public List<ChunkDrawer> chunkDrawers;
    }


    public class DrawDevice : PoolableObject
    {
        /// <summary>
        /// used to sign if need to update the mesh vertices
        /// </summary>
        public bool isVertDirty = true;
        /// <summary>
        /// used to sign if need to update the vertices color
        /// </summary>
        public bool isColorDirty = true;
        /// <summary>
        /// used to sign if need to update the vertices uv
        /// </summary>
        public bool isUVDirty = true;
        /// <summary>
        /// List of vertices position
        /// </summary>
        public Vector3[] verts;
        /// <summary>
        /// List of vertices color
        /// </summary>
        public Color32[] colors;
        /// <summary>
        /// List of uv0s
        /// </summary>
        public Vector2[] uvs;
        /// <summary>
        /// List of submesh triangle indices
        /// </summary>
        public readonly List<List<int>> triangles = new List<List<int>>();
        /// <summary>
        /// List of anime Chunks
        /// </summary>
        public readonly List<MaterialChunkDrawers> materialChunkList = new List<MaterialChunkDrawers>();
        /// <summary>
        /// List of anime Chunks
        /// </summary>
        private readonly List<AnimeChunkDrawer> animeChunks = new List<AnimeChunkDrawer>();
        /// <summary>
        /// total amount of all vertices
        /// </summary>
        private int m_numVerts = 0;

        /// <summary>
        /// 把某一个chunk 合并到materialChunkList 里去，先draw的chunk先渲染
        /// </summary>
        public void MergeChunks(Material material, ChunkDrawer chunk)
        {
            m_numVerts += chunk.position.Count;

            bool flag = true;
            for (int i= materialChunkList.Count-1; i>=0 && flag; i--)
            {
                var matList = materialChunkList[i];
                if (material == matList.material)
                {
                    matList.chunkDrawers.Add(chunk);
                    flag = false;
                }
                else
                {
                    var iter = matList.chunkDrawers.GetEnumerator();
                    while(iter.MoveNext())
                    {
                        var chunkDrawer = iter.Current;
                        if (Intersect(chunkDrawer, chunk))
                        {
                            var temp = new MaterialChunkDrawers();
                            temp.material = material;
                            temp.chunkDrawers = new List<ChunkDrawer>(); //todo: delete
                            temp.chunkDrawers.Add(chunk);
                            materialChunkList.Add(temp);
                            flag = false;
                            break;
                        }
                    }
                }
            }
            if (flag)
            {
                var temp = new MaterialChunkDrawers();
                temp.material = material;
                temp.chunkDrawers = new List<ChunkDrawer>(); //todo: delete
                temp.chunkDrawers.Add(chunk);
                materialChunkList.Add(temp);
            }
        }

        /// <summary>
        /// 所有chunk 顶点都收集完毕后填充到数组中
        /// </summary>
        public void PopulateVertices()
        {
            verts = new Vector3[m_numVerts];
            colors = new Color32[m_numVerts];
            uvs = new Vector2[m_numVerts];

            int totalCount = 0;
            var iter = materialChunkList.GetEnumerator();
            while (iter.MoveNext())
            {
                var it = iter.Current.chunkDrawers.GetEnumerator();
                int count = 0;
                while (it.MoveNext())
                {
                    var chunkDrawer = it.Current;
                    if (chunkDrawer.isAnimeChunk)
                    {
                        var temp = chunkDrawer as AnimeChunkDrawer;
                        temp.startIdx = totalCount;
                        animeChunks.Add(temp);
                    }
                    var position = chunkDrawer.position.ToArray();
                    System.Array.Copy(position, 0, verts, totalCount, position.Length);
                    var color = chunkDrawer.color.ToArray();
                    System.Array.Copy(color, 0, colors, totalCount, position.Length);
                    var uv = chunkDrawer.uv.ToArray();
                    System.Array.Copy(uv, 0, uvs, totalCount, position.Length);

                    count += chunkDrawer.position.Count;
                    totalCount += chunkDrawer.position.Count;
                }
                var indices = new List<int>();
                for (int i = totalCount - count; i < totalCount; i += 4)
                {
                    AddTriangle(indices, i, i + 1, i + 2);
                    AddTriangle(indices, i + 2, i + 3, i);
                }
                triangles.Add(indices);
            }
        }

        /// <summary>
        /// 添加一个三角形索引
        /// </summary>
        private void AddTriangle(List<int> triangle, int idx0, int idx1, int idx2)
        {
            triangle.Add(idx0);
            triangle.Add(idx1);
            triangle.Add(idx2);
        }

        /// <summary>
        /// 判断两个chunk是否交叉，HtRect 以左上角为原点，向下向右增长
        /// </summary>
        private bool Intersect(ChunkDrawer a, ChunkDrawer b)
        {
            if (a.rect.Left >= b.rect.Right || a.rect.Bottom <= b.rect.Top || a.rect.Right <= b.rect.Left || a.rect.Top >= b.rect.Bottom)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 更新所有动画chunk
        /// </summary>
        public void UpdateAllAnime()
        {
            var iter = animeChunks.GetEnumerator();
            while (iter.MoveNext())
            {
                iter.Current.Update();
            }
        }

        /// <summary>
        /// OnAcquire
        /// </summary>
        internal override void OnAcquire() { }

        /// <summary>
        /// OnRelease
        /// </summary>
        internal override void OnRelease()
        {
            Clear();
        }

        /// <summary>
        /// Clear
        /// </summary>
        public void Clear()
        {
            isVertDirty = true;
            isColorDirty = true;
            isUVDirty = true;
            verts = null;
            colors = null;
            uvs = null;
            m_numVerts = 0;
            
            var iter1 = triangles.GetEnumerator();
            while(iter1.MoveNext())
                iter1.Current.Clear();
            triangles.Clear();
            // 清空自己即可，所有的chunkDrawer在下面回收
            animeChunks.Clear();
            var iter3 = materialChunkList.GetEnumerator();
            while (iter3.MoveNext())
            {
                var it = iter3.Current.chunkDrawers.GetEnumerator();
                while (it.MoveNext())
                {
                    it.Current.Dispose();
                }
            }
            materialChunkList.Clear();
        }
    }
}