using System.Drawing;

namespace ML
{
    /// <summary>
    /// Reprezentuje planszę/mapę.
    /// </summary>
    class Plansza
    {
        /// <summary>
        /// Wysokość planszy w pikselach.
        /// </summary>
        int width, maxX, minX;

        /// <summary>
        /// Szerokość planszy w pikselach.
        /// </summary>
        int height, maxY, minY;

        /// <summary>
        /// Kolor planszy.
        /// </summary>
        Color kolor;

        public int Width { get => width; set => width = value; }
        public int MaxX { get => maxX; set => maxX = value; }
        public int MinX { get => minX; set => minX = value; }
        public int Height { get => height; set => height = value; }
        public int MaxY { get => maxY; set => maxY = value; }
        public int MinY { get => minY; set => minY = value; }
        public Color Kolor { get => kolor; set => kolor = value; }

        public Plansza(int width, int maxX, int minX, int height, int maxY, int minY, Color kolor)
        {
            this.width = width;
            this.maxX = maxX;
            this.minX = minX;
            this.height = height;
            this.maxY = maxY;
            this.minY = minY;
            this.kolor = kolor;
        }
    }
}
