using System.Drawing;
using System.Windows.Forms;

namespace ML
{
    /// <summary>
    /// Zapewnia dostęp do kontrolek GUI uruchomionych w innym wątku.
    /// </summary>
    class Aktualizuj
    {
        private delegate void AktualizujMapeDelegate(PictureBox pictureBox, Bitmap bitmap);
        public static void AktualizujMape(PictureBox pictureBox, Bitmap mapa)
        {
            if (pictureBox.InvokeRequired)
            {
                pictureBox.Invoke(new AktualizujMapeDelegate(AktualizujMape), new object[] { pictureBox, mapa });
            }
            else
            {
                if (pictureBox != null && mapa != null)
                {
                    pictureBox.Image = mapa;
                }
            }
        }
    }
}
