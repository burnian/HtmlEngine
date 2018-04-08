using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;


namespace HTMLEngine.Unity3D
{
    [AddComponentMenu("UI/Unity3DHTML")]
    [ExecuteInEditMode]
    public class Unity3DHTML : MaskableGraphic, ILayoutElement
    {
        /// <summary>
        /// 文本定宽
        /// </summary>
        public int maxLineWidth = 0;
        /// <summary>
        /// setting text here will raise changed flag
        /// </summary>
        public string html = "";
        /// <summary>
        /// 可点击区域注册的string
        /// </summary>
        private string _currentLink;
        public string currentLink
        {
            get { return _currentLink; }
            set { _currentLink = value; }
        }
        /// <summary>
        /// LayoutElement properties
        /// </summary>
        public virtual float minWidth { get { return 0; } }
        public virtual float preferredWidth { get { return cachedTransform != null ? cachedTransform.rect.width : 0; } }
        public virtual float flexibleWidth { get { return -1; } }
        public virtual float minHeight { get { return 0; } }
        public virtual float preferredHeight { get { return cachedTransform != null ? cachedTransform.sizeDelta.y : 0; } }
        public virtual float flexibleHeight { get { return -1; } }
        public virtual int layoutPriority { get { return 0; } }

        /// <summary>
        /// cache the RectTransform
        /// </summary>
        private RectTransform _rectTransform;
        private RectTransform cachedTransform
        {
            get
            {
                if (_rectTransform == null)
                {
                    _rectTransform = GetComponent<RectTransform>();
                }
                return _rectTransform;
            }
        }
        /// <summary>
        /// is the html content changed?
        /// </summary>
        private bool changed = true;
        /// <summary>
        /// cachedLineWidth
        /// </summary>
        private int cachedLineWidth;
        /// <summary>
        /// cachedHtml
        /// </summary>
        private string cachedHtml;
        /// <summary>
        /// unique mesh
        /// </summary>
        private Mesh _mesh;
        /// <summary>
        /// our html _compiler
        /// </summary>
        private readonly HtCompiler _compiler = HtEngine.GetCompiler();
        /// <summary>
        /// to deal with the vertices
        /// </summary>
        private readonly DrawDevice _drawDevice = HtEngine.GetDrawDevice();


        /// <summary>
        /// 
        /// </summary>
        public virtual void CalculateLayoutInputHorizontal() { }
        public virtual void CalculateLayoutInputVertical() { }

        /// <summary>
        /// Awake
        /// </summary>
        protected override void Awake()
        {
            HtEngine.RegisterLogger(new Unity3DLogger());
            HtEngine.RegisterDevice(new Unity3DDevice());
        }

        /// <summary>
        /// Start
        /// </summary>
        protected override void Start()
        {
            cachedTransform.pivot = Vector2.up;
            _mesh = new Mesh();
            _mesh.name = "Unity3DHTML Unique Mesh";
            _mesh.hideFlags = HideFlags.HideAndDontSave;
        }

        /// <summary>
        /// Update 调用太靠前，会导致渲染不出来
        /// </summary>
        private void LateUpdate()
        {
            if (maxLineWidth != cachedLineWidth || !string.Equals(html, cachedHtml))
            {
                changed = true;
                cachedLineWidth = maxLineWidth;
                cachedHtml = html;
            }

            if (changed)
            {
                _compiler.Compile(html, maxLineWidth > 0 ? maxLineWidth : Screen.width);
                _drawDevice.Clear();
                _compiler.Draw(Time.deltaTime, _drawDevice);
                _drawDevice.PopulateVertices();

                cachedTransform.sizeDelta = new Vector2(maxLineWidth, _compiler.CompiledHeight);

                changed = false;
            }

            _drawDevice.UpdateAllAnime();
            if (_drawDevice.isUVDirty && _drawDevice.verts != null)
            {
                SetVerticesDirty();
            }
        }

        /// <summary>
        /// Update the renderer's material.
        /// </summary>
        protected override void UpdateMaterial()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            if (!IsActive())
                return;

            canvasRenderer.materialCount = _drawDevice.materialChunkList.Count;

            var components = new List<Component>();
            GetComponents(typeof(IMaterialModifier), components);
            var iter = _drawDevice.materialChunkList.GetEnumerator();
            int submesh = 0;
            while (iter.MoveNext())
            {
                var currentMat = iter.Current.material;
                for (var i = 0; i < components.Count; i++)
                    currentMat = (components[i] as IMaterialModifier).GetModifiedMaterial(currentMat);

                canvasRenderer.SetMaterial(currentMat, submesh);
                ++submesh;
            }
        }

        /// <summary>
        /// Update the renderer's mesh.
        /// </summary>
        protected override void UpdateGeometry()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            if (cachedTransform == null || cachedTransform.rect.width <= 0 || cachedTransform.rect.height <= 0)
            {
                HtEngine.Log(HtLogLevel.Error, "Invalid cachedTransform");
                return;
            }

            bool flag = false;
            if (_drawDevice.isVertDirty && _drawDevice.isColorDirty && _drawDevice.isUVDirty)
                _mesh.Clear();
            if (_drawDevice.isVertDirty && _drawDevice.verts != null)
            {
                if (_drawDevice.verts.Length >= 65000)
                    throw new ArgumentException("Mesh can not have more than 65000 verticies");
                _mesh.vertices = _drawDevice.verts;
                _mesh.subMeshCount = _drawDevice.triangles.Count;
                var iter = _drawDevice.triangles.GetEnumerator();
                int submesh = 0;
                while (iter.MoveNext())
                {
                    var indices = iter.Current;
                    _mesh.SetTriangles(indices, submesh);
                    ++submesh;
                }
                _mesh.RecalculateBounds();
                _drawDevice.isVertDirty = false;
                flag = true;
            }
            if (_drawDevice.isColorDirty && _drawDevice.colors != null)
            {
                _mesh.colors32 = _drawDevice.colors;
                _drawDevice.isColorDirty = false;
                flag = true;
            }
            if (_drawDevice.isUVDirty && _drawDevice.uvs != null)
            {
                _mesh.uv = _drawDevice.uvs;
                _drawDevice.isUVDirty = false;
                flag = true;
            }
            if (flag)
            {
                canvasRenderer.SetMesh(_mesh);
            }
        }

        /// <summary>
        /// 文本超链接处理
        /// </summary>
        void OnGUI()
        {
            // catch mouseUp to detect links
            if (Event.current.type == EventType.mouseUp && _compiler.HasLink())
            {
                // remember we have offset of html container (x,y)
                var x = Event.current.mousePosition.x - cachedTransform.position.x;
                var y = Event.current.mousePosition.y - Screen.height + cachedTransform.position.y;
                currentLink = _compiler.GetLink((int)x, (int)y);
                if (currentLink != null)
                {
                    Debug.Log("Link clicked: " + currentLink);
                }
                else
                {
                    currentLink = "no links here";
                    Debug.Log("No links");
                }
            }
        }

        /// <summary>
        /// OnDestroy
        /// </summary>
        protected override void OnDestroy()
        {
            if (_compiler != null)
            {
                _compiler.Dispose();
            }
            if (_drawDevice != null)
            {
                _drawDevice.Dispose();
            }
            if (_mesh != null)
            {
                _mesh.Clear();
                if (Application.isPlaying)
                {
                    GameObject.Destroy(_mesh);
                }
                else GameObject.DestroyImmediate(_mesh);
                _mesh = null;
            }
        }

    }
}
