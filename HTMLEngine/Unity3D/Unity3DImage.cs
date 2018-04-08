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

using System;
using UnityEngine;

namespace HTMLEngine.Unity3D
{
    /// <summary>
    /// Provides image for use with HTMLEngine. Implements abstract class.
    /// </summary>
    public class Unity3DImage : HtImage
    {
        /// <summary>
        /// Loaded Material
        /// </summary>
        private readonly Material material;
        /// <summary>
        /// Loaded Sprite or single texture uv
        /// </summary>
        private readonly Vector2[] uv;
        /// <summary>
        /// Sprite or single texture width
        /// </summary>
        private readonly int width;
        /// <summary>
        /// Sprite or single texture height
        /// </summary>
        private readonly int height;
        /// <summary>
        /// Is special kind of image? (shows time)
        /// </summary>
        private readonly bool isTime;
        /// <summary>
        /// the holder
        /// </summary>
        private readonly Unity3DDevice u3dDevice;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="source">src attribute from img tag</param>
        /// <param name="device">holder</param>
        public Unity3DImage(string source, Unity3DDevice device)
        {
            u3dDevice = device;
            if ("#time".Equals(source, StringComparison.InvariantCultureIgnoreCase))
            {
                isTime = true;
                //timeStyle = new GUIStyle();
                //timeStyle.font = Resources.Load("fonts/code") as Font;
                //timeStyle.fontSize = 16;
                //timeStyle.fontStyle = FontStyle.Normal;
                //timeStyle.normal.textColor = Color.white;
                //timeStyle.alignment = TextAnchor.MiddleCenter;
            }
            else
            {
                string src = "";
                Texture2D tex = null;
                var index = source.LastIndexOf("#");
                if (index >= 0)
                {
                    var atlasPath = source.Substring(0, index);
                    var spriteName = source.Substring(index + 1);
                    var atlas = Resources.Load(atlasPath, typeof(UnityEngine.U2D.SpriteAtlas)) as UnityEngine.U2D.SpriteAtlas;
                    if (atlas == null)
                    {
                        HtEngine.Log(HtLogLevel.Error, "Could not load html atlas from " + atlasPath);
                        return;
                    }
                    var sprite = atlas.GetSprite(spriteName);
                    if (sprite == null)
                    {
                        HtEngine.Log(HtLogLevel.Error, "Could not load html sprite " + spriteName + " from " + atlasPath);
                        return;
                    }
                    src = atlasPath;
                    tex = sprite.texture;
                    width = (int)sprite.rect.width;
                    height = (int)sprite.rect.height;
                    uv = new Vector2[4] { sprite.uv[2], sprite.uv[0], sprite.uv[1], sprite.uv[3] };
                    if (Application.isPlaying)
                    {
                        UnityEngine.Object.Destroy(sprite);
                    }
                    else UnityEngine.Object.DestroyImmediate(sprite);
                }
                else
                {
                    src = source;
                    tex = Resources.Load(source, typeof(Texture2D)) as Texture2D;
                    if (tex == null)
                    {
                        HtEngine.Log(HtLogLevel.Error, "Could not load html texture from " + source);
                        return;
                    }
                    width = tex.width;
                    height = tex.height;
                    uv = new Vector2[4] { new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0f) };
                }
                material = u3dDevice.GetMaterial(src, tex);
            }
        }

        /// <summary>
        /// Returns width of image
        /// </summary>
        public override int Width
        {
            get
            {
                if (isTime) return 120;
                return width > 0 ? width : 1;
            }
        }

        /// <summary>
        /// Returns height of image
        /// </summary>
        public override int Height
        {
            get
            {
                if (isTime) return 20;
                return height > 0 ? height : 1;
            }
        }


        /// <summary>
        /// Draw method
        /// </summary>
        /// <param name="id">Chunk id</param>
        /// <param name="rect">Where to draw</param>
        /// <param name="color">Color to use (ignored for now)</param>
        /// <param name="linkText">Link text</param>
        /// <param name="drawDevice">Draw manager</param>
        public override void Draw(string id, HtRect rect, HtColor color, string linkText, DrawDevice drawDevice)
        {
            if (isTime)
            {
                var now = DateTime.Now;
                //timeStyle.Draw(new Rect(rect.X, rect.Y, rect.Width, rect.Height),
                //string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D3}", now.Hour, now.Minute, now.Second, now.Millisecond),
                //               false, false, false, false);
            }
            else if (uv != null)
            {
                var chunkDrawer = OP<ChunkDrawer>.Acquire();
                chunkDrawer.isAnimeChunk = false;
                chunkDrawer.rect = rect;
                chunkDrawer.position.Add(new Vector3(rect.X, -rect.Y - rect.Height, 0f));
                chunkDrawer.color.Add(new Color32(color.R, color.G, color.B, color.A));
                chunkDrawer.uv.Add(uv[0]);
                chunkDrawer.position.Add(new Vector3(rect.X, -rect.Y, 0f));
                chunkDrawer.color.Add(new Color32(color.R, color.G, color.B, color.A));
                chunkDrawer.uv.Add(uv[1]);
                chunkDrawer.position.Add(new Vector3(rect.X + rect.Width, -rect.Y, 0f));
                chunkDrawer.color.Add(new Color32(color.R, color.G, color.B, color.A));
                chunkDrawer.uv.Add(uv[2]);
                chunkDrawer.position.Add(new Vector3(rect.X + rect.Width, -rect.Y - rect.Height, 0f));
                chunkDrawer.color.Add(new Color32(color.R, color.G, color.B, color.A));
                chunkDrawer.uv.Add(uv[3]);

                drawDevice.MergeChunks(material, chunkDrawer);
            }
        }

        public void Clear()
        {
        }
    }

}