using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ML
{
    /// <summary>
    /// Formularz ekranu startowego
    /// </summary>
    public partial class EkranStartowy : Form
    {
        /// <summary>
        /// Stan ekranu startowego
        /// </summary>
        private bool stan;
        public bool Stan { get => stan; set => stan = value; }
        public EkranStartowy()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Zdarzenie dla timer1 na formularzu EkranStartowy.
        /// Po osiągnięciu końca paska, zatrzymuje timer i przestawia stan formularza na false.
        /// Panel1 jest na całej długości ekranu startowego, zaś długość panelu2 jest zwiększana co 5
        /// aż do osiągnięcia długości panela1.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Start(object sender, EventArgs e)
        {
            panel2.Width += 5;
            if (panel2.Width >= 700)
            {
                timer1.Stop();
                stan = false;
                this.Close();
            }
        }
    }
}
