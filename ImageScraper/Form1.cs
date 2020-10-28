using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        public List<string> ImageURLs = new List<string>();
        private async void buttonSearch_Click(object sender, EventArgs e)
        {
            var downloadTask = DownloadHTML();
            await downloadTask;
        }

        private void textBoxResults_TextChanged(object sender, EventArgs e)
        {
            var result = textBoxResults.Lines.Length == 0 ? $"No images found." :
                $"{textBoxResults.Lines.Length} images found.";

            labelImages.Visible = true;
            labelImages.Text = result;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                //Spara filerna till SelectedPath
            }
        }

        private async Task DownloadHTML()
        {
            textBoxResults.Clear();

            if (!textBoxSearch.Text.Contains("http://") &&
                !string.IsNullOrWhiteSpace(textBoxSearch.Text))
            {
                var client = new HttpClient();
                Task<string> downloadHTML = client.GetStringAsync($"http://{textBoxSearch.Text}");
                await downloadHTML;

                var matches = Pattern.Matches(downloadHTML.Result);

                foreach (var match in matches)
                {
                    if (!match.ToString().Contains("http") &&
                        !string.IsNullOrWhiteSpace(match.ToString()))
                    {
                        var result = $"http://{textBoxSearch.Text}{match}{Environment.NewLine}";
                        textBoxResults.Text += result;

                        ImageURLs.Add($"http://{textBoxSearch.Text}{match}");
                    }
                    else if (!string.IsNullOrWhiteSpace(match.ToString()))
                    {
                        textBoxResults.Text += $"{match} {Environment.NewLine}";

                        ImageURLs.Add($"{match}");
                    }
                }
            }

            var imageURLArray = ImageURLs.ToArray();
        }

        private async Task DownloadImagesAsync(string[] imagearray, string path)
        {
            var client = new HttpClient();
            var fileStream = new FileStream(path, FileMode.Create);
            Task<Task[]> downloadedImage;

            foreach (var image in imagearray)
            {
               client.GetByteArrayAsync(image);
            }
        }
    }
}
