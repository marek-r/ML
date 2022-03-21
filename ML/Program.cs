using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ML
{
    static class Program
    {
        /// <summary>
        /// Reprezentuje status ekranu startowego
        /// </summary>
        private static bool statusEkranStartowy = true;
        public static bool StatusEkranStartowy { get => statusEkranStartowy; set => statusEkranStartowy = value; }

        /// <summary>
        /// Główny punkt wejścia dla aplikacji.
        /// </summary>
        [STAThread]

        static void Main()
        {
            /*
             Ta metoda umożliwia stosowanie stylów wizualnych dla aplikacji.
             Style wizualizacji to kolory, czcionki i inne elementy wizualne, które tworzą motyw systemu operacyjnego.
             Kontrolki będą rysowane za pomocą stylów wizualnych, jeśli kontrolka i system operacyjny ją obsługują.
             Aby to zrobić, element musi zostać wywołany przed utworzeniem jakichkolwiek kontrolek w aplikacji.
             Zazwyczaj jest to EnableVisualStyles() EnableVisualStyles() pierwszy wiersz w Main funkcji.
             */
            Application.EnableVisualStyles();
            // Application.SetCompatibleTextRenderingDefault(false);
            Run();
        }

        static void Run()
        {
            // Ekran startowy w wątku.
            Thread threadEkranStartowy = new Thread(new ThreadStart(StartForm));
#pragma warning disable CS0618 // Type or member is obsolete
            threadEkranStartowy.ApartmentState = ApartmentState.STA;
#pragma warning restore CS0618 // Type or member is obsolete
            threadEkranStartowy.Start();

            if (StatusEkranStartowy == false)
            {
                threadEkranStartowy.Abort();
            }
        }

        /// <summary>
        /// Uruchamia formularz
        /// </summary>
        static void StartForm()
        {
            try
            {
                EkranStartowy formEkranStartowy = new EkranStartowy();
                Application.Run(formEkranStartowy);
                StatusEkranStartowy = formEkranStartowy.Stan != false;

                if (StatusEkranStartowy == false)
                {
                    Application.Run(new Form1());
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }
    }
}
