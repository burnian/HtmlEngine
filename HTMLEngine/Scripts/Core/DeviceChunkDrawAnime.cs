using UnityEngine;

namespace HTMLEngine.Core
{
    internal class DeviceChunkDrawAnime : DeviceChunk
    {
        public HtAnime Anime;

        public HtColor Color = HtColor.white; // TODO: implement something to draw image with given color for shader

        public string Id;

        public int fps;

        public override void Draw(float deltaTime, string linkText, DrawDevice drawDevice)
        {
            Anime.Draw(Id, Rect, Color, linkText, fps, drawDevice);
        }
        public override void MeasureSize()
        {
            Debug.Assert(Anime != null, "Anime is not assigned");

            Rect.Width = Anime.Width;
            Rect.Height = Anime.Height;
        }
    }
}