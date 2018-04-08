

namespace HTMLEngine
{
    public abstract class HtAnime
    {
        public abstract int Width { get; }
        public abstract int Height { get; }
        public abstract void Draw(string id, HtRect rect, HtColor color, string linkText, int fps, DrawDevice drawDevice);
    }
}