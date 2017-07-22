using ImageProcessingServiceTest.ImageProcessingServiceReference;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageProcessingServiceTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ImageProcessingServiceClient client = new ImageProcessingServiceClient();
            client.Open();
            MessageBox.Show(client.GetData(0));
            client.Close();
        }
    }
}
