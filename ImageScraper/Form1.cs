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
using System.Text.RegularExpressions;

namespace ImageScraper
{
    public partial class mainForm : Form
    {
        public mainForm()
        {
            InitializeComponent();
            Pattern = new Regex("(?<=<img\\s[^>]*?src=\")[^\"]*");
        }

        public Regex Pattern { get; set; }
        public List<string> ImageURLs { get; set; }
        private void buttonSearch_Click(object sender, EventArgs e)
        {
            if (!textBoxSearch.Text.Contains("http://") &&
                !string.IsNullOrWhiteSpace(textBoxSearch.Text))
            {
                var client = new HttpClient();
                Task<string> downloadHTML = client.GetStringAsync($"http://{textBoxSearch.Text}");
                downloadHTML.Wait();

                var matches = Pattern.Matches(downloadHTML.Result);

                foreach (var match in matches)
                {
                    textBoxResults.Text += match.ToString();
                }

                
            }
        }
    }
}
