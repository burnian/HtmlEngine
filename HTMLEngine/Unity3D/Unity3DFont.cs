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

using UnityEngine;

namespace HTMLEngine.Unity3D
{
    /// <summary>
    /// Provides font for use with HTMLEngine. Implements abstract class.
    /// </summary>
    public class Unity3DFont : HtFont
    {
        /// <summary>
        /// style to draw
        /// </summary>
        public readonly GUIStyle style = new GUIStyle();
        /// <summary>
        /// content to draw
        /// </summary>
        public readonly GUIContent content = new GUIContent();
        /// <summary>
        /// Width of whitespace
        /// </summary>
        private readonly int whiteSize;
        /// <summary>
        /// Loaded Material
        /// </summary>
        private readonly Material material;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="face">Font name</param>
        /// <param name="size">Font size</param>
        /// <param name="bold">Bold flag</param>
        /// <param name="italic">Italic flag</param>
        public Unity3DFont(string face, int size, bool bold, bool italic) : base(face, size, bold, italic)
        {
            // creating key to load from resources
            string key = string.Format("{0}{1}{2}{3}", face, size, bold ? "b" : "", italic ? "i" : "");
            this.style.font = Resources.Load("fonts/" + key, typeof(Font)) as Font;
            material = style.font.material;
            //material.shader = Shader.Find("GUI/Text Shader");
            material.shader = Shader.Find("GUI/Text Shader Custom");
            //material.name = key;

            // showing error if font not found
            if (this.style.font==null)
            {
                Debug.LogError("Could not load font: " + key);
            }

            // some tuning
            this.style.wordWrap = false;
            
            // calculating whitesize
            this.content.text = " .";
            this.whiteSize = (int) this.style.CalcSize(this.content).x;
            this.content.text = ".";
            this.whiteSize -= (int)this.style.CalcSize(this.content).x;
        }

        /// <summary>
        /// Space between text lines in pixels
        /// </summary>
        public override int LineSpacing { get { return (int) this.style.lineHeight; } }

        /// <summary>
        /// Space between words
        /// </summary>
        public override int WhiteSize { get { return this.whiteSize; } }

        /// <summary>
        /// Measuring text width and height
        /// </summary>
        /// <param name="text">text to measure</param>
        /// <returns>width and height of measured text</returns>
        public override HtSize Measure(string text)
        {
            this.content.text = text;
            var r = this.style.CalcSize(this.content);
            // add ending spaces width.
            for (int i = text.Length; i > 0; --i) {
              if (text[i - 1] == ' ') r.x += WhiteSize;
              else break;
            }
            return new HtSize((int)r.x, (int)r.y);
        }

        /// <summary>
        /// Draw method.
        /// </summary>
        /// <param name="rect">��pivotΪ(0��1)�����뵽���ڵ��pivot�ϣ���������������</param>
        /// <param name="color">Text color</param>
        /// <param name="text">Text</param>
        /// <param name="isEffect">Is effect</param>
        /// <param name="effect">Effect</param>
        /// <param name="effectColor">Effect color</param>
        /// <param name="effectAmount">Effect amount</param>
        /// <param name="linkText">Link text</param>
        /// <param name="dic">Material, UIVertices</param>
        public override void Draw(string id, HtRect rect, HtColor color, string text, bool isEffect, Core.DrawTextEffect effect, HtColor effectColor,
            int effectAmount, string linkText, DrawDevice drawDevice)
        {
            var settings = new TextGenerationSettings();
            settings.generationExtents = new Vector2(rect.Width, rect.Height);
            settings.pivot = Vector2.up;
            settings.textAnchor = TextAnchor.UpperLeft;
            settings.scaleFactor = 1f;
            settings.color = new Color(color.R, color.G, color.B, color.A);
            settings.richText = false;
            settings.font = style.font;
            settings.lineSpacing = 1;
            settings.updateBounds = false;
            settings.generateOutOfBounds = false;
            settings.resizeTextForBestFit = true;
            settings.alignByGeometry = false;
            settings.horizontalOverflow = HorizontalWrapMode.Wrap;
            settings.verticalOverflow = VerticalWrapMode.Overflow;
            if (settings.font.dynamic)
            {
                settings.fontSize = Size;
                if (!Bold && !Italic)
                {
                    settings.fontStyle = FontStyle.Normal;
                }
                else if (Bold && !Italic)
                {
                    settings.fontStyle = FontStyle.Bold;
                }
                else if (!Bold && Italic)
                {
                    settings.fontStyle = FontStyle.Italic;
                }
                else
                {
                    settings.fontStyle = FontStyle.BoldAndItalic;
                }
            }
            TextGenerator generator = new TextGenerator();
            // text����Ⱦ���Լ������һ��rectTransform���棬��rectTransform��pivotΪ(0��1)������Ϊ0��0�����뵽���ڵ��pivot�ϡ�
            // HtVertex���꣬��graphic pivotΪ����ԭ�㣬��������������
            generator.Populate(text, settings);

            var iter = generator.verts.GetEnumerator();
            var chunkDrawer = OP<ChunkDrawer>.Acquire();
            chunkDrawer.isAnimeChunk = false;
            chunkDrawer.rect = rect;
            while (iter.MoveNext())
            {
                var vert = iter.Current;
                // ����ƫ����
                chunkDrawer.position.Add(new Vector3(vert.position.x + rect.X, vert.position.y - rect.Y, 0f));
                chunkDrawer.color.Add(vert.color);
                chunkDrawer.uv.Add(vert.uv0);
            }

            drawDevice.MergeChunks(material, chunkDrawer);
        }
    }
}