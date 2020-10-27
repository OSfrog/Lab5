using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageScraper
{
    public partial class mainForm : Form
    {
        public mainForm()
        {
            InitializeComponent();
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            if (!textBoxSearch.Text.Contains("http://"))
            {
                var client = new HttpClient();
                Task<string> downloadHTML = client.GetStringAsync($"http://{textBoxSearch.Text}");
            }
        }
    }
}
