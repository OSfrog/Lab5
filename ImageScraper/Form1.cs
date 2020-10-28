using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageScraper
{
    public enum FileTypes
    {
        png = 1,
        jpg,
        jpeg,

    }
    public partial class mainForm : Form
    {
        public List<string> ImageURLs = new List<string>();
        public mainForm()
        {
            InitializeComponent();
            Pattern = new Regex("(?<=<img\\s[^>]*?src=\")[^\"]*");
        }

        public Regex Pattern { get; set; }
        public List<Task<byte[]>> TaskList { get; set; } = new List<Task<byte[]>>();

        private async void buttonSearch_Click(object sender, EventArgs e)
        {
            var downloadTask = DownloadHTML();
        }

        private void textBoxResults_TextChanged(object sender, EventArgs e)
        {
            var result = textBoxResults.Lines.Length == 0 ? $"No images found." :
                $"{textBoxResults.Lines.Length} images found.";

            if (textBoxResults.Lines.Length != 0)
            {
                buttonSave.Enabled = true;
            }
            else
            {
                buttonSave.Enabled = false;
            }
            labelImages.Visible = true;
            labelImages.Text = result;
        }

        private async void buttonSave_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                await DownloadImagesAsync(ImageURLs.ToArray());
                await SaveImages(dialog.SelectedPath);
            }
        }

        private async Task DownloadHTML()
        {
            textBoxResults.Clear();

            if (!textBoxSearch.Text.Contains("http://") &&
                !string.IsNullOrWhiteSpace(textBoxSearch.Text))
            {
                var client = new HttpClient();
                Task<string> downloadHTML = null;

                downloadHTML = client.GetStringAsync($"http://{textBoxSearch.Text}");
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
        }

        private async Task DownloadImagesAsync(string[] imagearray)
        {
            var client = new HttpClient();

            foreach (var image in imagearray)
            {
                 TaskList.Add(client.GetByteArrayAsync(image));
            }

        }

        private async Task SaveImages(string path)
        {
            var i = 1;
            while (TaskList.Count > 0)
            {
                var completedTask =  await Task.WhenAny(TaskList);
                var result = await completedTask;
                var fileStream = new FileStream($"{path}\\image{i}", FileMode.Create);
                await fileStream.WriteAsync(result, 0, result.Length);
                i++;
                TaskList.Remove(completedTask);
            }
        }
    }
}
