using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ML
{
    /// <summary>
    /// Mapka z oznaczonymi polami (czy mrówka była w danym punkcie).
    /// </summary>
    class Mapa
    {
        public int[,] Koordynaty;

        public int Width;
        public int Height;

        public Mapa(int width, int height)
        {
            Width = width;
            Height = height;
            Koordynaty = new int[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Koordynaty[x, y] = 0;
                }
            }
        }
    }
}
