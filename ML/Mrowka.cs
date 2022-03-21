using System.Drawing;

namespace ML
{
    class Mrowka
    {
        int kierunek;
        int x;
        int y;
        Color kolor;
        public int Kierunek { get => kierunek; set => kierunek = value; }
        public int X { get => x; set => x = value; }
        public int Y { get => y; set => y = value; }
        public Color Kolor { get => kolor; set => kolor = value; }

        public void NowaPozycja(int kierunek, int x, int y)
        {
            Kierunek = kierunek;
            X = x;
            Y = y;
        }
    }
}
