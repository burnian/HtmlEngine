using UnityEngine;
using System.Collections.Generic;


namespace HTMLEngine.Unity3D
{
    /// <summary>
    /// Provides Anime for use with HTMLEngine. Implements abstract class.
    /// </summary>
    public class Unity3DAnime : HtAnime
    {
        /// <summary>
        /// Loaded material
        /// </summary>
        private readonly Material material;
        /// <summary>
        /// Loaded Sprite Rect
        /// </summary>
        private readonly Rect rect;
        /// <summary>
        /// Loaded sprites uv
        /// </summary>
        private readonly Vector2[][] UVs;
        /// <summary>
        /// the holder
        /// </summary>
        private readonly Unity3DDevice u3dDevice;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="source">src attribute from anim tag</param>
        /// <param name="u3dDevice">holder</param>
        public Unity3DAnime(string source, Unity3DDevice device)
        {
            // 每一张纹理图片转sprite必须将mesh type设置为fullscreen，这样在atlas中每个sprite才会只有4个顶点
            u3dDevice = device;
            var index = source.LastIndexOf("#");
            var atlasPath = source.Substring(0, index);
            var spriteName = source.Substring(index + 1);
            var atlas = Resources.Load(atlasPath, typeof(UnityEngine.U2D.SpriteAtlas)) as UnityEngine.U2D.SpriteAtlas;
            if (atlas == null)
            {
                HtEngine.Log(HtLogLevel.Error, "Could not load html atlas from " + atlasPath);
                return;
            }
            var sprites = new Sprite[atlas.spriteCount];
            atlas.GetSprites(sprites);
            var frames = new Dictionary<string, Sprite>();
            var spriteNames = new List<string>();
            var iter = sprites.GetEnumerator();
            while (iter.MoveNext())
            {
                var temp = iter.Current as Sprite;
                if (temp.name.StartsWith(spriteName))
                {
                    frames.Add(temp.name, temp);
                    spriteNames.Add(temp.name);
                }
            }
            if (frames.Count <= 0)
            {
                HtEngine.Log(HtLogLevel.Error, "Could not load html anime " + spriteName + " from " + atlasPath);
                return;
            }
            spriteNames.Sort();
            int i = 0;
            UVs = new Vector2[frames.Count][];
            var it = spriteNames.GetEnumerator();
            while (it.MoveNext())
            {
                var temp = frames[it.Current];
                UVs[i] = new Vector2[temp.uv.Length];
                UVs[i][0] = temp.uv[2];
                UVs[i][1] = temp.uv[0];
                UVs[i][2] = temp.uv[1];
                UVs[i][3] = temp.uv[3];
                i++;
            }
            var sprite = frames[spriteNames[0]];
            rect = sprite.rect;
            material = u3dDevice.GetMaterial(atlasPath, sprite.texture);
            // 从atlas中取出来的是clone sprite
            iter = sprites.GetEnumerator();
            while (iter.MoveNext())
            {
                var temp = iter.Current as Sprite;
                if (Application.isPlaying)
                {
                    Object.Destroy(temp);
                }
                else Object.DestroyImmediate(temp);
            }
        }

        /// <summary>
        /// Returns width of image
        /// </summary>
        public override int Width
        {
            get
            {
                return rect == null ? 1 : (int)rect.width;
            }
        }

        /// <summary>
        /// Returns height of image
        /// </summary>
        public override int Height
        {
            get
            {
                return rect == null ? 1 : (int)rect.height;
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
        public override void Draw(string id, HtRect rect, HtColor color, string linkText, int fps, DrawDevice drawDevice)
        {
            if (this.rect == null)
            {
                return;
            }
            var chunkDrawer = OP<AnimeChunkDrawer>.Acquire();
            chunkDrawer.isAnimeChunk = true;
            chunkDrawer.drawDevice = drawDevice;
            chunkDrawer.deltaTime = 1f / fps;
            chunkDrawer.frameUVs = UVs;
            chunkDrawer.rect = rect;
            // 从左下角开始，顺时针
            chunkDrawer.position.Add(new Vector3(rect.X, -rect.Y - rect.Height, 0f));
            chunkDrawer.color.Add(new Color32(color.R, color.G, color.B, color.A));
            chunkDrawer.uv.Add(UVs[0][0]);
            chunkDrawer.position.Add(new Vector3(rect.X, -rect.Y, 0f));
            chunkDrawer.color.Add(new Color32(color.R, color.G, color.B, color.A));
            chunkDrawer.uv.Add(UVs[0][1]);
            chunkDrawer.position.Add(new Vector3(rect.X + rect.Width, -rect.Y, 0f));
            chunkDrawer.color.Add(new Color32(color.R, color.G, color.B, color.A));
            chunkDrawer.uv.Add(UVs[0][2]);
            chunkDrawer.position.Add(new Vector3(rect.X + rect.Width, -rect.Y - rect.Height, 0f));
            chunkDrawer.color.Add(new Color32(color.R, color.G, color.B, color.A));
            chunkDrawer.uv.Add(UVs[0][3]);

            drawDevice.MergeChunks(material, chunkDrawer);
        }

        public void Clear()
        {
        }
    }

}