/// The modified version of this software is Copyright (C) 2013 ZHing.
/// The original version's copyright as below.

/* Copyright (C) 2012 Ruslan A. Abdrashitov

Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
and associated documentation files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions 
of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
DEALINGS IN THE SOFTWARE. */

using System.Collections.Generic;
using UnityEngine;

namespace HTMLEngine.Unity3D
{
    /// <summary>
    /// Provides gate between HTMLEngine and Unity3D. Implements abstract class.
    /// </summary>
    public class Unity3DDevice : HtDevice
    {
        /// <summary>
        /// Fonts cache (to do not load every time from resouces)
        /// </summary>
        private readonly Dictionary<string, Unity3DFont> fonts = new Dictionary<string, Unity3DFont>();
        /// <summary>
        /// Image cache (same thing)
        /// </summary>
        private readonly Dictionary<string, Unity3DImage> images = new Dictionary<string, Unity3DImage>();
        /// <summary>
        /// Anime cache (same thing)
        /// </summary>
        private readonly Dictionary<string, Unity3DAnime> animes = new Dictionary<string, Unity3DAnime>();
        /// <summary>
        /// Material cache (same thing)
        /// </summary>
        private readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();

        /// <summary>
        /// White texture (for FillRect method)
        /// </summary>
        private static Material whiteMaterial;

        /// <summary>
        /// get the material singleton
        /// </summary>
        /// <param name="src">src attribute from img tag</param>
        /// <param name="texture">src attribute from img tag</param>
        /// <returns>Loaded Material</returns>
        public Material GetMaterial(string src, Texture2D texture)
        {
            Material material = null;
            if (!materials.TryGetValue(src, out material))
            {
                material = new Material(Shader.Find("UI/Default"));
                material.mainTexture = texture;
                material.name = src;
                materials.Add(src, material);
            }
            return material;
        }

        /// <summary>
        /// Load font
        /// </summary>
        /// <param name="face">Font name</param>
        /// <param name="size">Font size</param>
        /// <param name="bold">Bold flag</param>
        /// <param name="italic">Italic flag</param>
        /// <returns>Loaded font</returns>
        public override HtFont LoadFont(string face, int size, bool bold, bool italic)
        {
            // try get from cache
            string key = string.Format("{0}{1}{2}{3}", face, size, bold ? "b" : "", italic ? "i" : "");
            Unity3DFont ret;
            if (fonts.TryGetValue(key, out ret)) return ret;
            // fail with cache, so create new and store into cache
            ret = new Unity3DFont(face, size, bold, italic);
            fonts[key] = ret;
            return ret;
        }

        /// <summary>
        /// Load image
        /// </summary>
        /// <param name="src">src attribute from img tag</param>
        /// <param name="inAtlas">src texture is in atlas or not</param>
        /// <returns>Loaded image</returns>
        public override HtImage LoadImage(string src)
        {
            // try get from cache
            Unity3DImage ret;
            if (!images.TryGetValue(src, out ret))
            {
                // fail with cache, so create new and store into cache
                ret = new Unity3DImage(src, this);
                images[src] = ret;
            }
            return ret;
        }

        /// <summary>
        /// Load Anime
        /// </summary>
        /// <param name="src">src attribute from anim tag</param>
        /// <returns>Loaded image</returns>
        public override HtAnime LoadAnime(string src)
        {
            // try get from cache
            Unity3DAnime ret;
            if (!animes.TryGetValue(src, out ret))
            {
                ret = new Unity3DAnime(src, this);
                animes[src] = ret;
            }
            return ret;
        }

        /// <summary>
        /// FillRect implementation
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="color"></param>
        public override void FillRect(HtRect rect, HtColor color, DrawDevice drawDevice)
        {
            // create white texture if need
            if (whiteMaterial == null)
            {
                Texture2D whiteTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                whiteTex.SetPixel(0, 0, Color.white);
                whiteTex.Apply(false, true);
                whiteMaterial = new Material(Shader.Find("UI/Default"));
                whiteMaterial.mainTexture = whiteTex;
            }
            var chunkDrawer = OP<ChunkDrawer>.Acquire();
            chunkDrawer.isAnimeChunk = false;
            chunkDrawer.rect = rect;
            chunkDrawer.position.Add(new Vector3(rect.X, -rect.Y - rect.Height, 0f));
            chunkDrawer.color.Add(new Color32(color.R, color.G, color.B, color.A));
            chunkDrawer.uv.Add(new Vector2(0f, 0f));
            chunkDrawer.position.Add(new Vector3(rect.X, -rect.Y, 0f));
            chunkDrawer.color.Add(new Color32(color.R, color.G, color.B, color.A));
            chunkDrawer.uv.Add(new Vector2(0f, 1f));
            chunkDrawer.position.Add(new Vector3(rect.X + rect.Width, -rect.Y, 0f));
            chunkDrawer.color.Add(new Color32(color.R, color.G, color.B, color.A));
            chunkDrawer.uv.Add(new Vector2(1f, 1f));
            chunkDrawer.position.Add(new Vector3(rect.X + rect.Width, -rect.Y - rect.Height, 0f));
            chunkDrawer.color.Add(new Color32(color.R, color.G, color.B, color.A));
            chunkDrawer.uv.Add(new Vector2(1f, 0f));

            drawDevice.MergeChunks(whiteMaterial, chunkDrawer);
        }

        /// <summary>
        /// On device is released.
        /// </summary>
        public override void OnRelease()
        {
            if (fonts != null)
            {
                // font的材质由Resources管理
                fonts.Clear();
            }
            if (images != null)
            {
                var iter = images.GetEnumerator();
                while (iter.MoveNext())
                {
                    var temp = iter.Current.Value;
                    temp.Clear();
                }
                images.Clear();
            }
            if (animes != null)
            {
                var iter = animes.GetEnumerator();
                while (iter.MoveNext())
                {
                    var temp = iter.Current.Value;
                    temp.Clear();
                }
                animes.Clear();
            }
            if (materials != null)
            {
                var iter = materials.GetEnumerator();
                while (iter.MoveNext())
                {
                    var temp = iter.Current.Value;
                    if (Application.isPlaying)
                    {
                        Object.Destroy(temp);
                    }
                    else Object.DestroyImmediate(temp);
                }
                materials.Clear();
            }
            if (Application.isPlaying)
            {
                Object.Destroy(whiteMaterial);
            }
            else Object.DestroyImmediate(whiteMaterial);
        }
    }
}
