using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Management.Instrumentation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ML
{
    public partial class Form1 : Form
    {
        CancellationTokenSource _tokenSource = null;

        int sekunda = 1;
        /// <summary>
        /// Czas uśpienia wątku.
        /// </summary>
        int czasSynchronizacji;

        /// <summary>
        /// Dostęp do czasu uśpienia wątku.
        /// </summary>
        public int CzasSynchronizacji { get => czasSynchronizacji; set => czasSynchronizacji = value; }

        /// <summary>
        /// Priorytet wątku.
        /// </summary>
        int priorytet;

        /// <summary>
        /// Dostęp do priorytetu wątku.
        /// </summary>
        public int Priorytet { get => priorytet; set => priorytet = value; }

        /// <summary>
        /// Zawiera kolor startowy planszy.
        /// </summary>
        Color planszaKolorStartowy;

        /// <summary>
        /// Dostep do startowego koloru planszy.
        /// </summary>
        public Color PlanszaKolorStartowy { get => planszaKolorStartowy; set => planszaKolorStartowy = value; }

        /// <summary>
        /// Zawiera kolor startowy mrówki
        /// </summary>
        Color mrowkaKolorStartowy;

        /// <summary>
        /// Dostęp do startowego koloru mrówki.
        /// </summary>
        public Color MrowkaKolorStartowy { get => mrowkaKolorStartowy; set => mrowkaKolorStartowy = value; }

        Color sciezkaKolorStartowy;

        /// <summary>
        /// Dostęp do startowego koloru mrówki.
        /// </summary>
        public Color SciezkaKolorStartowy { get => sciezkaKolorStartowy; set => sciezkaKolorStartowy = value; }

        /// <summary>
        /// Plansza
        /// </summary>
        private Plansza plansza;

        /// <summary>
        /// Dostęp do planszy.
        /// </summary>
        internal Plansza Plansza { get => plansza; set => plansza = value; }

        /// <summary>
        /// Mapa
        /// </summary>
        private Bitmap mapa;

        /// <summary>
        /// Dostęp do mapy
        /// </summary>
        public Bitmap Mapa { get => mapa; set => mapa = value; }

        /// <summary>
        /// Słownik zawierający rozmiar mapy w pikselach (x,y).
        /// </summary>
        Dictionary<int, int> rozmiarMapy = new Dictionary<int, int>();

        /// <summary>
        /// Wątek mrówki.
        /// </summary>
        private Thread mrowkaStartThread;

        /// <summary>
        /// Status wątku.
        /// </summary>
        private volatile bool Stop = false;

        /// <summary>
        /// Dostęp do zmiennej wątku mrówki.
        /// </summary>
        public Thread MrowkaStartThread { get => mrowkaStartThread; set => mrowkaStartThread = value; }

        /// <summary>
        /// Formularz główny.
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Ustawienia podczas ładowania formularza głównego.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            // Button Stop
            Image ikonaClose = ResizeImage(Properties.Resources.close, new Size(16, 16));
            button3.Image = ikonaClose;
            button3.Enabled = false;

            // Rozmiar mapy
            comboBox1.SelectedIndex = 2;
            toolTip1RozmiarMapy.SetToolTip(comboBox1, "Ustawia rozmiar mapy po której porusza się mrówka.\n" +
                "Im większy rozmiar mapy tym większe zapotrzebowanie na pamięć.");

            // Czas uśpienia wątku
            comboBox2.SelectedIndex = 16;
            CzasSynchronizacji = int.Parse(comboBox2.SelectedItem.ToString());
            toolTip1Sleep.SetToolTip(comboBox2, "Wstrzymuje wątek odpowiedzialny za obliczenia na określony czas [ms].\n " +
                "W zależności od parametrów sprzętu, zbyt niska wartość może spowodować błąd podczas działania programu.");

            // Priorytet wątku
            // https://docs.microsoft.com/pl-pl/dotnet/api/system.threading.threadpriority?view=net-5.0
            comboBox3.SelectedIndex = 0;
            Priorytet = comboBox3.SelectedIndex;
            toolTip1Priorytet.SetToolTip(comboBox3, "Ustawia priorytet wątku odpowiedzialnego za obliczenia.\n" +
                "W zależności od parametrów sprzętu, zbyt niska wartość może spowodować błąd podczas działania programu.");

            // Kolor mapy
            label4.Text = Color.White.ToString();
            PlanszaKolorStartowy = Color.White;
            UstawParametryMapy(comboBox1.SelectedItem.ToString(), PlanszaKolorStartowy);

            //Kolor ścieżki
            label6.Text = label4.Text;
            SciezkaKolorStartowy = Color.White;
            button5.Enabled = false;

            // Kolor mrówki
            label5.Text = Color.Black.ToString();
            MrowkaKolorStartowy = Color.Black;
            toolStripStatusLabel3.Text = "Mrówka gotowa";

            // Timer
            //timer1.Enabled = false;
            //toolStripStatusLabel4.Text = "";
            //toolStripStatusLabel4.Visible = false;
            //toolTip1Sekundnik.SetToolTip(checkBox2, "Pokazuje ile upłynęło czasu [s] od uruchomienia mrówki.\n" +
            //    "Ma wpływ na wydajność.");
        }

        /// <summary>
        /// ComboBox-Zmiana rozmiaru mapy.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UstawParametryMapy(comboBox1.SelectedItem.ToString(), PlanszaKolorStartowy);

        }

        /// <summary>
        /// Combobox-ustawienie czasu blokady wątku.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            CzasSynchronizacji = int.Parse(comboBox2.SelectedItem.ToString());
        }

        /// <summary>
        /// Combobox-ustawienie priorytetu wątku.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            Priorytet = comboBox3.SelectedIndex;
        }

        /// <summary>
        /// CheckBox-sekundnik
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
            {
                //timer1.Enabled = true;
                toolStripStatusLabel4.Visible = true;
                toolStripStatusLabel4.Text = "Minęło: ";
                sekunda = 1;
            }
            else
            {
                timer1.Enabled = false;
                toolStripStatusLabel4.Visible = false;
                sekunda = 1;
            }
        }

        /// <summary>
        /// Klawisz wyboru koloru mapy.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            colorDialog1.AllowFullOpen = false;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                if (checkBox3.Checked == true)
                {
                    PlanszaKolorStartowy = colorDialog1.Color;
                    SciezkaKolorStartowy = colorDialog1.Color;
                    label4.Text = colorDialog1.Color.ToString();
                    label6.Text = colorDialog1.Color.ToString();
                    UstawParametryMapy(comboBox1.SelectedItem.ToString(), PlanszaKolorStartowy);
                }
                else
                {
                    PlanszaKolorStartowy = colorDialog1.Color;
                    label4.Text = colorDialog1.Color.ToString();
                    UstawParametryMapy(comboBox1.SelectedItem.ToString(), PlanszaKolorStartowy);
                }

            }
        }

        /// <summary>
        /// Klawisz wyboru koloru mrówki.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            colorDialog1.AllowFullOpen = false;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                MrowkaKolorStartowy = colorDialog1.Color;
                label5.Text = colorDialog1.Color.ToString();
            }
        }

        /// <summary>
        /// Klawisz uruchamiający mrówkę.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            _tokenSource = new CancellationTokenSource();
            var token = _tokenSource.Token;

            try
            {
                if (checkBox1.Checked == true)
                {
                    Task taskMrowkaStartTask = new Task(delegate { MrowkaStartTask(token); });

                    //Console.WriteLine(taskMrowkaStartTask.Status.ToString());
                    checkBox1.Enabled = false;
                    checkBox3.Enabled = false;
                    comboBox1.Enabled = false;
                    comboBox2.Enabled = false;
                    comboBox3.Enabled = false;
                    button1.Enabled = false;
                    button2.Enabled = false;
                    button3.Enabled = true;
                    button4.Enabled = false;
                    button5.Enabled = false;
                    pomocMenu.Enabled = false;

                    taskMrowkaStartTask.Start();
                }
                else
                {
                    Task taskMrowkaStartTeleportTask = new Task(delegate { MrowkaStartTeleportTask(token); });

                    //Console.WriteLine(taskMrowkaStartTask.Status.ToString());
                    checkBox1.Enabled = false;
                    checkBox3.Enabled = false;
                    comboBox1.Enabled = false;
                    comboBox2.Enabled = false;
                    comboBox3.Enabled = false;
                    button1.Enabled = false;
                    button2.Enabled = false;
                    button3.Enabled = true;
                    button4.Enabled = false;
                    button5.Enabled = false;
                    pomocMenu.Enabled = false;

                    taskMrowkaStartTeleportTask.Start();
                }
            }
            catch (OperationCanceledException exception)
            {
                Console.WriteLine(exception);
                throw;
            }
            finally
            {
                //_tokenSource.Dispose();
            }




            //try
            //{
            //    if (checkBox1.Checked == true)
            //    {
            //        Stop = false;
            //        checkBox1.Enabled = false;
            //        //checkBox2.Enabled = false;
            //        comboBox1.Enabled = false;
            //        comboBox2.Enabled = false;
            //        comboBox3.Enabled = false;
            //        button2.Enabled = false;
            //        button4.Enabled = false;
            //        pomocMenu.Enabled = false;
            //        Image ikonaClose = ResizeImage(Properties.Resources.close, new Size(16, 16));
            //        button1.Image = ikonaClose;

            //        if (MrowkaStartThread == null)
            //        {
            //            mrowkaStartThread = new Thread(MrowkaStart);
            //            priorytet = Priorytet;
            //            switch (priorytet)
            //            {
            //                case 0:
            //                    mrowkaStartThread.Priority = ThreadPriority.Lowest;
            //                    break;
            //                case 1:
            //                    mrowkaStartThread.Priority = ThreadPriority.BelowNormal;
            //                    break;
            //                case 2:
            //                    mrowkaStartThread.Priority = ThreadPriority.Normal;
            //                    break;
            //                case 3:
            //                    mrowkaStartThread.Priority = ThreadPriority.AboveNormal;
            //                    break;
            //                case 4:
            //                    mrowkaStartThread.Priority = ThreadPriority.Highest;
            //                    break;
            //                default:
            //                    mrowkaStartThread.Priority = ThreadPriority.Lowest;
            //                    break;
            //            }

            //            mrowkaStartThread.Start();
            //            button1.Text = "Anuluj";
            //            //Console.WriteLine(MrowkaStartThread.Priority.ToString());
            //            //Console.WriteLine(Priorytet.ToString());
            //            //Console.WriteLine(Thread.CurrentThread.Priority.ToString());
            //        }
            //        else
            //        {
            //            Stop = true;
            //            mrowkaStartThread = null;
            //            button1.Text = "Start";
            //            checkBox1.Enabled = true;
            //            //checkBox2.Enabled = true;
            //            comboBox1.Enabled = true;
            //            comboBox2.Enabled = true;
            //            comboBox3.Enabled = true;
            //            button2.Enabled = true;
            //            button4.Enabled = true;
            //            pomocMenu.Enabled = true;
            //            button1.Image = Properties.Resources.play_1_;
            //            //if (checkBox2.Checked == true)
            //            //{
            //            //    timer1.Stop();
            //            //}
            //        }
            //    }
            //    else
            //    {
            //        Stop = false;
            //        checkBox1.Enabled = false;
            //        //checkBox2.Enabled = false;
            //        comboBox1.Enabled = false;
            //        comboBox2.Enabled = false;
            //        comboBox3.Enabled = false;
            //        button2.Enabled = false;
            //        button4.Enabled = false;
            //        pomocMenu.Enabled = false;
            //        Image ikonaClose = ResizeImage(Properties.Resources.close, new Size(16, 16));
            //        button1.Image = ikonaClose;

            //        if (MrowkaStartThread == null)
            //        {
            //            //Bitmap mapa = new Bitmap(pictureBox1.Image);
            //            //Bitmap klon = new Bitmap(mapa);
            //            //Thread mrowkaStartThread = new Thread(() => MrowkaStartTeleport());
            //             mrowkaStartThread = new Thread(MrowkaStartTeleport);
            //            priorytet = Priorytet;
            //            switch (priorytet)
            //            {
            //                case 0:
            //                    mrowkaStartThread.Priority = ThreadPriority.Lowest;
            //                    break;
            //                case 1:
            //                    mrowkaStartThread.Priority = ThreadPriority.BelowNormal;
            //                    break;
            //                case 2:
            //                    mrowkaStartThread.Priority = ThreadPriority.Normal;
            //                    break;
            //                case 3:
            //                    mrowkaStartThread.Priority = ThreadPriority.AboveNormal;
            //                    break;
            //                case 4:
            //                    mrowkaStartThread.Priority = ThreadPriority.Highest;
            //                    break;
            //                default:
            //                    mrowkaStartThread.Priority = ThreadPriority.Lowest;
            //                    break;
            //            }

            //            mrowkaStartThread.Start();
            //            button1.Text = "Anuluj";
            //            //Console.WriteLine(MrowkaStartThread.Priority.ToString());
            //            //Console.WriteLine(Priorytet.ToString());
            //            //Console.WriteLine(Thread.CurrentThread.Priority.ToString());
            //        }
            //        else
            //        {
            //            Stop = true;
            //            mrowkaStartThread = null;
            //            button1.Text = "Start";
            //            checkBox1.Enabled = true;
            //            //checkBox2.Enabled = true;
            //            comboBox1.Enabled = true;
            //            comboBox2.Enabled = true;
            //            comboBox3.Enabled = true;
            //            button2.Enabled = true;
            //            button4.Enabled = true;
            //            pomocMenu.Enabled = true;
            //            button1.Image = Properties.Resources.play_1_;
            //            //if (checkBox2.Checked == true)
            //            //{
            //            //    timer1.Stop();
            //            //}
            //        }
            //    }
            //}
            //catch (Exception error)
            //{
            //    MessageBox.Show(error.Message.ToString(), "Halo tu mrówka !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
        }

        /// <summary>
        /// Ustawia parametry mapy.
        /// </summary>
        /// <param name="wartoscComboBox">Rozmiar w pikselach pochodzący z comboBox w formacie XXxYY</param>
        /// <param name="kolor">Kolor.</param>
        private void UstawParametryMapy(String wartoscComboBox, Color kolor)
        {
            // Rozbija string "po x" pochodzący z comboboxa 
            string[] maxXmaxY = wartoscComboBox.Split('x');
            // Czyści słoenik ze starego spisu.
            rozmiarMapy.Clear();
            // Dodaje aktualne parametry do słownika
            rozmiarMapy.Add(int.Parse(maxXmaxY[0]), int.Parse(maxXmaxY[1]));
            //foreach (var item in rozmiarMapy)
            //{
            //    Console.WriteLine(item);
            //}

            //Console.WriteLine(rozmiarMapy.ElementAt(0).Key);
            //Console.WriteLine(rozmiarMapy.ElementAt(0).Value);

            // Tworzy obiekt klasy plansza o zadanych parametrach
            plansza = new Plansza(rozmiarMapy.ElementAt(0).Key, 300, 25, rozmiarMapy.ElementAt(0).Value, 300, 25, kolor);
            plansza.Kolor = kolor;
            pictureBox1.Width = plansza.Width;
            pictureBox1.Height = plansza.Height;


            try
            {
                // Ustawia mapkę na środku panela
                pictureBox1.Location = new Point((panel1.Width - pictureBox1.Width) / 2, (panel1.Height - pictureBox1.Height) / 2);
                Bitmap img = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                for (int x = 0; x < pictureBox1.Width; x++)
                {
                    for (int y = 0; y < pictureBox1.Height; y++)
                    {
                        img.SetPixel(x, y, plansza.Kolor);
                    }
                }
                pictureBox1.Image = img;
                //pictureBox1.Refresh();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message.ToString(), "Halo tu mrówka !", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        /// <summary>
        /// Uruchamia mrówkę (do graniecy mapy).
        /// </summary>
        private void MrowkaStart()
        {
            // Tworzy obiekt klasy mrówka. Mrówka skierowana jest na:
            // 0-północ *-domyślnie
            // 1-wschód
            // 2-południe
            // 3-zachód
            Mrowka mrowka = new Mrowka();
            mrowka.Kierunek = 0;
            mrowka.Kolor = MrowkaKolorStartowy;
            mrowka.X = plansza.Width / 2;
            mrowka.Y = plansza.Height / 2;
            try
            {
                if (mrowka.X < plansza.MaxX && mrowka.X > 0 && mrowka.Y < plansza.MaxY && mrowka.Y > 0)
                {
                    int i = 0;
                    do
                    {
                        Bitmap mapa = new Bitmap(pictureBox1.Image);
                        Bitmap klon = new Bitmap(mapa);
                        if (klon.GetPixel(mrowka.X, mrowka.Y).ToArgb() == Plansza.Kolor.ToArgb())
                        {
                            switch (mrowka.Kierunek)
                            {
                                case 0:
                                    mrowka.Kierunek = 3;
                                    klon.SetPixel(mrowka.X, mrowka.Y, mrowka.Kolor);
                                    mrowka.X--;
                                    break;
                                case 1:
                                    mrowka.Kierunek = 0;
                                    klon.SetPixel(mrowka.X, mrowka.Y, mrowka.Kolor);
                                    mrowka.Y--;
                                    break;
                                case 2:
                                    mrowka.Kierunek = 1;
                                    klon.SetPixel(mrowka.X, mrowka.Y, mrowka.Kolor);
                                    mrowka.X++;
                                    break;
                                case 3:
                                    mrowka.Kierunek = 2;
                                    klon.SetPixel(mrowka.X, mrowka.Y, mrowka.Kolor);
                                    mrowka.Y++;
                                    break;
                            }
                        }
                        else if (klon.GetPixel(mrowka.X, mrowka.Y).ToArgb() == mrowka.Kolor.ToArgb())
                        {
                            switch (mrowka.Kierunek)
                            {
                                case 0:
                                    mrowka.Kierunek = 1;
                                    klon.SetPixel(mrowka.X, mrowka.Y, Plansza.Kolor);
                                    mrowka.X++;
                                    break;
                                case 1:
                                    mrowka.Kierunek = 2;
                                    klon.SetPixel(mrowka.X, mrowka.Y, Plansza.Kolor);
                                    mrowka.Y++;
                                    break;
                                case 2:
                                    mrowka.Kierunek = 3;
                                    klon.SetPixel(mrowka.X, mrowka.Y, Plansza.Kolor);
                                    mrowka.X--;
                                    break;
                                case 3:
                                    mrowka.Kierunek = 0;
                                    klon.SetPixel(mrowka.X, mrowka.Y, Plansza.Kolor);
                                    mrowka.Y--;
                                    break;
                            }
                        }
                        i++;
                        Aktualizuj.AktualizujMape(pictureBox1, klon);
                        if (InvokeRequired)
                        {
                            Invoke(new SetToolStripDelegate(SetToolStripLabel), "Kroki mrówki: " + i.ToString());
                        }
                        else
                        {
                            toolStripStatusLabel3.Text = "Kroki mrówki: " + i.ToString();
                        }
                        //this.BeginInvoke(new MethodInvoker(delegate
                        //{

                        //}));

                        Thread.Sleep(CzasSynchronizacji);


                    } while (Stop == false);
                    //this.BeginInvoke(new MethodInvoker(delegate
                    //{
                    //    if (checkBox2.Checked == true)
                    //    {
                    //        timer1.Stop();
                    //    }
                    //}));
                    this.BeginInvoke(new MethodInvoker(delegate
                    {
                        button1.Enabled = false;
                        MessageBox.Show("Anulowano", "Halo tu mrówka !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        mrowka = null;
                        pictureBox1.Image = null;
                        UstawParametryMapy(comboBox1.SelectedItem.ToString(), PlanszaKolorStartowy);
                        button1.Enabled = true;
                    }));

                }
            }
            catch (Exception)
            {
                this.BeginInvoke(new MethodInvoker(delegate
                {
                    //if (checkBox2.Checked == true)
                    //{
                    //    timer1.Stop();
                    //}
                    button1.Enabled = false;
                    MessageBox.Show("Dotarłam do końca mapy i nie wiem gdzie dalej iść :-(.", "Halo tu mrówka !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    button1.Enabled = true;
                    button1.Image = Properties.Resources.play_1_;
                    Stop = true;
                    mrowkaStartThread = null;
                    UstawParametryMapy(comboBox1.SelectedItem.ToString(), PlanszaKolorStartowy);
                    button1.Text = "Start";
                    checkBox1.Enabled = true;
                    //checkBox2.Enabled = true;
                    comboBox1.Enabled = true;
                    comboBox2.Enabled = true;
                    comboBox3.Enabled = true;
                    button2.Enabled = true;
                    button4.Enabled = true;
                    pomocMenu.Enabled = true;
                }));
            }
            //finally
            //{
            //    this.BeginInvoke(new MethodInvoker(delegate
            //    {
            //        UstawParametryMapy(comboBox1.SelectedItem.ToString(), PlanszaKolorStartowy);
            //    }));
            //}
        }

        private void MrowkaStartTeleport()
        {
            // Tworzy obiekt klasy mrówka. Mrówka skierowana jest na:
            // 0-północ *-domyślnie
            // 1-wschód
            // 2-południe
            // 3-zachód
            Mrowka mrowka = new Mrowka();
            mrowka.Kierunek = 0;
            mrowka.Kolor = MrowkaKolorStartowy;
            mrowka.X = plansza.Width / 2;
            mrowka.Y = plansza.Height / 2;
            try
            {
                if (mrowka.X < plansza.MaxX && mrowka.X > 0 && mrowka.Y < plansza.MaxY && mrowka.Y > 0)
                {
                    int i = 0;
                    do
                    {
                        Bitmap mapa = new Bitmap(pictureBox1.Image);
                        Bitmap klon = new Bitmap(mapa);
                        if (klon.GetPixel(mrowka.X, mrowka.Y).ToArgb() == Plansza.Kolor.ToArgb())
                        {
                            switch (mrowka.Kierunek)
                            {
                                case 0:
                                    mrowka.Kierunek = 3;
                                    klon.SetPixel(mrowka.X, mrowka.Y, mrowka.Kolor);
                                    if (mrowka.X - 1 < 0)
                                    {
                                        mrowka.X = plansza.Width - 1;
                                    }
                                    else
                                    {
                                        mrowka.X--;
                                    }
                                    break;
                                case 1:
                                    mrowka.Kierunek = 0;
                                    klon.SetPixel(mrowka.X, mrowka.Y, mrowka.Kolor);
                                    if (mrowka.Y - 1 < 0)
                                    {
                                        mrowka.Y = plansza.Height - 1;
                                    }
                                    else
                                    {
                                        mrowka.Y--;
                                    }
                                    break;
                                case 2:
                                    mrowka.Kierunek = 1;
                                    klon.SetPixel(mrowka.X, mrowka.Y, mrowka.Kolor);
                                    if (mrowka.X + 1 > plansza.Width - 1)
                                    {
                                        mrowka.X = 0;
                                    }
                                    else
                                    {
                                        mrowka.X++;
                                    }
                                    break;
                                case 3:
                                    mrowka.Kierunek = 2;
                                    klon.SetPixel(mrowka.X, mrowka.Y, mrowka.Kolor);
                                    if (mrowka.Y + 1 > plansza.Height - 1)
                                    {
                                        mrowka.Y = 0;
                                    }
                                    else
                                    {
                                        mrowka.Y++;
                                    }
                                    break;
                            }
                        }
                        else if (klon.GetPixel(mrowka.X, mrowka.Y).ToArgb() == mrowka.Kolor.ToArgb())
                        {
                            switch (mrowka.Kierunek)
                            {
                                case 0:
                                    mrowka.Kierunek = 1;
                                    klon.SetPixel(mrowka.X, mrowka.Y, Plansza.Kolor);
                                    if (mrowka.X + 1 > plansza.Width - 1)
                                    {
                                        mrowka.X = 0;
                                    }
                                    else
                                    {
                                        mrowka.X++;
                                    }
                                    break;
                                case 1:
                                    mrowka.Kierunek = 2;
                                    klon.SetPixel(mrowka.X, mrowka.Y, Plansza.Kolor);
                                    if (mrowka.Y + 1 > plansza.Height - 1)
                                    {
                                        mrowka.Y = 0;
                                    }
                                    else
                                    {
                                        mrowka.Y++;
                                    }
                                    break;
                                case 2:
                                    mrowka.Kierunek = 3;
                                    klon.SetPixel(mrowka.X, mrowka.Y, Plansza.Kolor);
                                    if (mrowka.X - 1 < 0)
                                    {
                                        mrowka.X = plansza.Width - 1;
                                    }
                                    else
                                    {
                                        mrowka.X--;
                                    }
                                    break;
                                case 3:
                                    mrowka.Kierunek = 0;
                                    klon.SetPixel(mrowka.X, mrowka.Y, Plansza.Kolor);
                                    if (mrowka.Y - 1 < 0)
                                    {
                                        mrowka.Y = plansza.Height - 1;
                                    }
                                    else
                                    {
                                        mrowka.Y--;
                                    }
                                    break;
                            }
                        }
                        i++;
                        Aktualizuj.AktualizujMape(pictureBox1, klon);
                        if (InvokeRequired)
                        {
                            Invoke(new SetToolStripDelegate(SetToolStripLabel), "Kroki mrówki: " + i.ToString());
                        }
                        else
                        {
                            toolStripStatusLabel3.Text = "Kroki mrówki: " + i.ToString();
                        }
                        Thread.Sleep(CzasSynchronizacji);

                    } while (Stop == false);
                    //this.BeginInvoke(new MethodInvoker(delegate
                    //{
                    //    if (checkBox2.Checked == true)
                    //    {
                    //        timer1.Stop();
                    //    }
                    //}));
                    this.BeginInvoke(new MethodInvoker(delegate
                    {
                        button1.Enabled = false;
                        MessageBox.Show("Anulowano", "Halo tu mrówka !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        mrowka = null;
                        pictureBox1.Image = null;
                        UstawParametryMapy(comboBox1.SelectedItem.ToString(), PlanszaKolorStartowy);
                        button1.Enabled = true;
                    }));

                }
            }
            catch (Exception)
            {
                this.BeginInvoke(new MethodInvoker(delegate
                {
                    //if (checkBox2.Checked == true)
                    //{
                    //    timer1.Stop();
                    //}
                    button1.Enabled = false;
                    MessageBox.Show("Dotarłam do końca mapy i nie wiem gdzie dalej iść :-(.", "Halo tu mrówka !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    button1.Enabled = true;
                    button1.Image = Properties.Resources.play_1_;
                    Stop = true;
                    mrowkaStartThread = null;
                    UstawParametryMapy(comboBox1.SelectedItem.ToString(), PlanszaKolorStartowy);
                    button1.Text = "Start";
                    checkBox1.Enabled = true;
                    //checkBox2.Enabled = true;
                    comboBox1.Enabled = true;
                    comboBox2.Enabled = true;
                    comboBox3.Enabled = true;
                    button2.Enabled = true;
                    button4.Enabled = true;
                    pomocMenu.Enabled = true;
                }));
            }
        }

        /// <summary>
        /// Uruchamia mrówkę (do graniecy mapy) na tasku.
        /// </summary>
        /// <param name="token"></param>
        private void MrowkaStartTask(CancellationToken token)
        {
            // Console.WriteLine(Thread.CurrentThread.Priority.ToString());

            // Tworzy obiekt klasy mrówka. Mrówka skierowana jest na:
            // 0-północ *-domyślnie
            // 1-wschód
            // 2-południe
            // 3-zachód
            Mrowka mrowka = new Mrowka();
            mrowka.Kierunek = 0;
            mrowka.Kolor = MrowkaKolorStartowy;
            mrowka.X = plansza.Width / 2;
            mrowka.Y = plansza.Height / 2;

            // Mapka do oznaczenia gdzie była mrówka
            Mapa mapa = new Mapa(plansza.Width, plansza.Height);
            // Klon
            Bitmap klon = new Bitmap(plansza.Width, plansza.Height);
            // Grafika
            Graphics gr = Graphics.FromImage(klon);
            // Pędzle 
            SolidBrush mrowkaKolor = new SolidBrush(mrowka.Kolor);
            SolidBrush planszaKolor = new SolidBrush(plansza.Kolor);
            SolidBrush sciezkaKolor = new SolidBrush(SciezkaKolorStartowy);
            // Wypełnia pictureBox kolorem planszy
            for (int x = 0; x < pictureBox1.Width; x++)
            {
                for (int y = 0; y < pictureBox1.Height; y++)
                {
                    gr.FillRectangle(planszaKolor, x, y, 1, 1);
                }
            }

            try
            {

                switch (Priorytet)
                {
                    case 0:
                        Thread.CurrentThread.Priority = ThreadPriority.Lowest;
                        break;
                    case 1:
                        Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
                        break;
                    case 2:
                        Thread.CurrentThread.Priority = ThreadPriority.Normal;
                        break;
                    case 3:
                        Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
                        break;
                    case 4:
                        Thread.CurrentThread.Priority = ThreadPriority.Highest;
                        break;
                    default:
                        mrowkaStartThread.Priority = ThreadPriority.Lowest;
                        break;
                }
                // Console.WriteLine(Thread.CurrentThread.Priority.ToString());
                int i = 0;
                do
                {
                    if (mapa.Koordynaty[mrowka.X, mrowka.Y] == 0)
                    {
                        switch (mrowka.Kierunek)
                        {
                            case 0:
                                mrowka.Kierunek = 3;
                                mapa.Koordynaty[mrowka.X, mrowka.Y] = 1;
                                gr.FillRectangle(mrowkaKolor, mrowka.X, mrowka.Y, 1, 1);
                                mrowka.X--;
                                break;
                            case 1:
                                mrowka.Kierunek = 0;
                                mapa.Koordynaty[mrowka.X, mrowka.Y] = 1;
                                gr.FillRectangle(mrowkaKolor, mrowka.X, mrowka.Y, 1, 1);
                                mrowka.Y--;
                                break;
                            case 2:
                                mrowka.Kierunek = 1;
                                mapa.Koordynaty[mrowka.X, mrowka.Y] = 1;
                                gr.FillRectangle(mrowkaKolor, mrowka.X, mrowka.Y, 1, 1);
                                mrowka.X++;
                                break;
                            case 3:
                                mrowka.Kierunek = 2;
                                mapa.Koordynaty[mrowka.X, mrowka.Y] = 1;
                                gr.FillRectangle(mrowkaKolor, mrowka.X, mrowka.Y, 1, 1);
                                mrowka.Y++;
                                break;
                        }
                    }
                    else if (mapa.Koordynaty[mrowka.X, mrowka.Y] == 1)
                    {
                        switch (mrowka.Kierunek)
                        {
                            case 0:
                                mrowka.Kierunek = 1;
                                mapa.Koordynaty[mrowka.X, mrowka.Y] = 0;
                                gr.FillRectangle(sciezkaKolor, mrowka.X, mrowka.Y, 1, 1);
                                mrowka.X++;
                                break;
                            case 1:
                                mrowka.Kierunek = 2;
                                mapa.Koordynaty[mrowka.X, mrowka.Y] = 0;
                                gr.FillRectangle(sciezkaKolor, mrowka.X, mrowka.Y, 1, 1);
                                mrowka.Y++;
                                break;
                            case 2:
                                mrowka.Kierunek = 3;
                                mapa.Koordynaty[mrowka.X, mrowka.Y] = 0;
                                gr.FillRectangle(sciezkaKolor, mrowka.X, mrowka.Y, 1, 1);
                                mrowka.X--;
                                break;
                            case 3:
                                mrowka.Kierunek = 0;
                                mapa.Koordynaty[mrowka.X, mrowka.Y] = 0;
                                gr.FillRectangle(sciezkaKolor, mrowka.X, mrowka.Y, 1, 1);
                                mrowka.Y--;
                                break;
                        }
                    }

                    i++;
                    Aktualizuj.AktualizujMape(pictureBox1, klon);
                    toolStripStatusLabel3.Text = "Kroki mrówki: " + i.ToString();
                    pictureBox1.Image = klon;
                    Thread.Sleep(CzasSynchronizacji);

                    if (token.IsCancellationRequested)
                    {
                        this.BeginInvoke(new MethodInvoker(delegate
                        {

                            checkBox1.Enabled = true;
                            //checkBox2.Enabled = false;
                            comboBox1.Enabled = true;
                            comboBox2.Enabled = true;
                            comboBox3.Enabled = true;
                            button1.Enabled = true;
                            button3.Enabled = false;
                            button2.Enabled = true;
                            button4.Enabled = true;
                            if (checkBox3.Enabled == true)
                            {
                                button5.Enabled = false;
                            }
                            else
                            {
                                button5.Enabled = true;
                            }
                            pomocMenu.Enabled = true;
                        }));

                        token.ThrowIfCancellationRequested();
                    }

                } while (true);

            }
            catch (Exception)
            {
                MessageBox.Show("Dotarłam do końca mapy i nie wiem gdzie dalej iść :-(.", "Halo tu mrówka !",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.BeginInvoke(new MethodInvoker(delegate
                {
                    checkBox1.Enabled = true;
                    checkBox3.Enabled = true;
                    comboBox1.Enabled = true;
                    comboBox2.Enabled = true;
                    comboBox3.Enabled = true;
                    button1.Enabled = true;
                    button3.Enabled = false;
                    button2.Enabled = true;
                    button4.Enabled = true;
                    if (checkBox3.Checked == true)
                    {
                        button5.Enabled = false;
                    }
                    else
                    {
                        button5.Enabled = true;
                    }
                    pomocMenu.Enabled = true;
                }));
            }
            finally
            {
                Thread.CurrentThread.Priority = ThreadPriority.Normal;
                // Console.WriteLine(Thread.CurrentThread.Priority.ToString());
            }
        }

        /// <summary>
        /// Uruchamia mrówkę na tasku.
        /// </summary>
        /// <param name="token"></param>
        private void MrowkaStartTeleportTask(CancellationToken token)
        {

            // Tworzy obiekt klasy mrówka. Mrówka skierowana jest na:
            // 0-północ *-domyślnie
            // 1-wschód
            // 2-południe
            // 3-zachód
            Mrowka mrowka = new Mrowka();
            mrowka.Kierunek = 0;
            mrowka.Kolor = MrowkaKolorStartowy;
            mrowka.X = plansza.Width / 2;
            mrowka.Y = plansza.Height / 2;

            // Mapka do oznaczenia gdzie była mrówka
            Mapa mapa = new Mapa(plansza.Width, plansza.Height);
            // Klon
            Bitmap klon = new Bitmap(plansza.Width, plansza.Height);
            // Grafika
            Graphics gr = Graphics.FromImage(klon);
            // Pędzle 
            SolidBrush mrowkaKolor = new SolidBrush(mrowka.Kolor);
            SolidBrush planszaKolor = new SolidBrush(plansza.Kolor);
            SolidBrush sciezkaKolor = new SolidBrush(SciezkaKolorStartowy);
            // Wypełnia pictureBox kolorem planszy
            for (int x = 0; x < pictureBox1.Width; x++)
            {
                for (int y = 0; y < pictureBox1.Height; y++)
                {
                    gr.FillRectangle(planszaKolor, x, y, 1, 1);
                }
            }

            try
            {
                int i = 0;
                do
                {
                    if (mapa.Koordynaty[mrowka.X, mrowka.Y] == 0)
                    {
                        switch (mrowka.Kierunek)
                        {
                            case 0:
                                mrowka.Kierunek = 3;
                                mapa.Koordynaty[mrowka.X, mrowka.Y] = 1;
                                gr.FillRectangle(mrowkaKolor, mrowka.X, mrowka.Y, 1, 1);
                                if (mrowka.X - 1 < 0)
                                {
                                    mrowka.X = plansza.Width - 1;
                                }
                                else
                                {
                                    mrowka.X--;
                                }
                                break;
                            case 1:
                                mrowka.Kierunek = 0;
                                mapa.Koordynaty[mrowka.X, mrowka.Y] = 1;
                                gr.FillRectangle(mrowkaKolor, mrowka.X, mrowka.Y, 1, 1);
                                if (mrowka.Y - 1 < 0)
                                {
                                    mrowka.Y = plansza.Height - 1;
                                }
                                else
                                {
                                    mrowka.Y--;
                                }
                                break;
                            case 2:
                                mrowka.Kierunek = 1;
                                mapa.Koordynaty[mrowka.X, mrowka.Y] = 1;
                                gr.FillRectangle(mrowkaKolor, mrowka.X, mrowka.Y, 1, 1);
                                if (mrowka.X + 1 > plansza.Width - 1)
                                {
                                    mrowka.X = 0;
                                }
                                else
                                {
                                    mrowka.X++;
                                }
                                break;
                            case 3:
                                mrowka.Kierunek = 2;
                                mapa.Koordynaty[mrowka.X, mrowka.Y] = 1;
                                gr.FillRectangle(mrowkaKolor, mrowka.X, mrowka.Y, 1, 1);
                                if (mrowka.Y + 1 > plansza.Height - 1)
                                {
                                    mrowka.Y = 0;
                                }
                                else
                                {
                                    mrowka.Y++;
                                }
                                break;
                        }
                    }
                    else if (mapa.Koordynaty[mrowka.X, mrowka.Y] == 1)
                    {
                        switch (mrowka.Kierunek)
                        {
                            case 0:
                                mrowka.Kierunek = 1;
                                mapa.Koordynaty[mrowka.X, mrowka.Y] = 0;
                                gr.FillRectangle(sciezkaKolor, mrowka.X, mrowka.Y, 1, 1);
                                if (mrowka.X + 1 > plansza.Width - 1)
                                {
                                    mrowka.X = 0;
                                }
                                else
                                {
                                    mrowka.X++;
                                }
                                break;
                            case 1:
                                mrowka.Kierunek = 2;
                                mapa.Koordynaty[mrowka.X, mrowka.Y] = 0;
                                gr.FillRectangle(sciezkaKolor, mrowka.X, mrowka.Y, 1, 1);
                                if (mrowka.Y + 1 > plansza.Height - 1)
                                {
                                    mrowka.Y = 0;
                                }
                                else
                                {
                                    mrowka.Y++;
                                }
                                break;
                            case 2:
                                mrowka.Kierunek = 3;
                                mapa.Koordynaty[mrowka.X, mrowka.Y] = 0;
                                gr.FillRectangle(sciezkaKolor, mrowka.X, mrowka.Y, 1, 1);
                                if (mrowka.X - 1 < 0)
                                {
                                    mrowka.X = plansza.Width - 1;
                                }
                                else
                                {
                                    mrowka.X--;
                                }
                                break;
                            case 3:
                                mrowka.Kierunek = 0;
                                mapa.Koordynaty[mrowka.X, mrowka.Y] = 0;
                                gr.FillRectangle(sciezkaKolor, mrowka.X, mrowka.Y, 1, 1);
                                if (mrowka.Y - 1 < 0)
                                {
                                    mrowka.Y = plansza.Height - 1;
                                }
                                else
                                {
                                    mrowka.Y--;
                                }
                                break;
                        }
                    }

                    i++;
                    Aktualizuj.AktualizujMape(pictureBox1, klon);
                    toolStripStatusLabel3.Text = "Kroki mrówki: " + i.ToString();
                    pictureBox1.Image = klon;
                    Thread.Sleep(CzasSynchronizacji);

                    if (token.IsCancellationRequested)
                    {
                        this.BeginInvoke(new MethodInvoker(delegate
                        {

                            checkBox1.Enabled = true;
                            checkBox3.Enabled = true;
                            comboBox1.Enabled = true;
                            comboBox2.Enabled = true;
                            comboBox3.Enabled = true;
                            button1.Enabled = true;
                            button3.Enabled = false;
                            button2.Enabled = true;
                            button4.Enabled = true;
                            if (checkBox3.Checked == true)
                            {
                                button5.Enabled = false;
                            }
                            else
                            {
                                button5.Enabled = true;
                            }
                            pomocMenu.Enabled = true;
                        }));

                        token.ThrowIfCancellationRequested();
                    }


                } while (true);
            }
            catch (Exception error)
            {
                MessageBox.Show("Zabłądziłam :-(( " + error.Message.ToString(), "Halo tu mrówka !",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.BeginInvoke(new MethodInvoker(delegate
                {

                    checkBox1.Enabled = true;
                    checkBox3.Enabled = true;
                    comboBox1.Enabled = true;
                    comboBox2.Enabled = true;
                    comboBox3.Enabled = true;
                    button1.Enabled = true;
                    button3.Enabled = false;
                    button2.Enabled = true;
                    button4.Enabled = true;
                    if (checkBox3.Checked == true)
                    {
                        button5.Enabled = false;
                    }
                    else
                    {
                        button5.Enabled = true;
                    }
                    pomocMenu.Enabled = true;
                }));
            }
            finally
            {
                Thread.CurrentThread.Priority = ThreadPriority.Normal;
                // Console.WriteLine(Thread.CurrentThread.Priority.ToString());
            }
        }

        /// <summary>
        /// Sekundnik
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new SetToolStripDelegate(SetToolStripLabel), toolStripStatusLabel4.Text = "Minęło: " + sekunda++.ToString() + " s.");
            }
            else
            {
                toolStripStatusLabel4.Text = "Minęło: " + sekunda++.ToString() + " s.";
            }

        }

        /// <summary>
        /// Zmienia rozmiar bitmapy
        /// </summary>
        /// <param name="imgToResize">Zdjęcie</param>
        /// <param name="size">Rozmiar</param>
        /// <returns></returns>
        public static Image ResizeImage(Image imgToResize, Size size)
        {
            return (Image)(new Bitmap(imgToResize, size));
        }

        /// <summary>
        /// Okno o programie.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void oProgramieToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            About oProgramie = new About();
            oProgramie.Text = "O programie ML";
            oProgramie.ShowDialog();
        }

        /// <summary>
        /// Delegat do aktualizacji toolStripa np. z krokami
        /// </summary>
        /// <param name="text">Tekst.</param>
        private delegate void SetToolStripDelegate(string text);

        /// <summary>
        /// Dotyczy delegata SetToolStripDelegate
        /// </summary>
        /// <param name="text"></param>
        private void SetToolStripLabel(string text)
        {
            toolStripStatusLabel3.Text = text;
        }

        /// <summary>
        /// Anulowanie taska
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            _tokenSource.Cancel();
        }

        /// <summary>
        /// Kolor ścieżki mrówki
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            colorDialog1.AllowFullOpen = false;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                SciezkaKolorStartowy = colorDialog1.Color;
                label6.Text = colorDialog1.Color.ToString();
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked == true)
            {
                label6.Text = label4.Text;
                SciezkaKolorStartowy = planszaKolorStartowy;
                button5.Enabled = false;
            }
            else
            {
                SciezkaKolorStartowy = planszaKolorStartowy;
                button5.Enabled = true;
            }
        }
    }
}
