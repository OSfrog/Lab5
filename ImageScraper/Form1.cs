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
        public List<string> ImageURLs = new List<string>();
        public mainForm()
        {
            InitializeComponent();
            UrlPattern = new Regex("<img.*src=\"(.*\\.(jpg|jpeg|png|gif|bmp).*)\"\\s");
            FileExtensionPattern = new Regex("\\.(jpg|jpeg|png|gif|bmp)");
        }

        public Regex UrlPattern { get; set; }
        public Regex FileExtensionPattern { get; set; }
        public Dictionary<Task<byte[]>, string> TaskDictionary { get; set; } = new Dictionary<Task<byte[]>, string>();

        private async void buttonSearch_Click(object sender, EventArgs e)
        {
            var downloadTask = DownloadHTML();
        }

        private void textBoxResults_TextChanged(object sender, EventArgs e)
        {
            var result = textBoxResults.Lines.Length == 0 ? $"No images found." :
                $"{textBoxResults.Lines.Length} images found.";

            buttonSave.Enabled = textBoxResults.Lines.Length != 0;
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
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromMinutes(1);

                var downloadedHTMLCode = client.GetStringAsync($"http://{textBoxSearch.Text}");
                await downloadedHTMLCode;

                var matches = UrlPattern.Matches(downloadedHTMLCode.Result);


                foreach (Match match in matches)
                {
                    if (!match.Groups[1].ToString().Contains("http") &&
                        !string.IsNullOrWhiteSpace(match.ToString()))
                    {
                        var result = $"http://{textBoxSearch.Text}{match.Groups[1]}{Environment.NewLine}";
                        textBoxResults.Text += result;

                        ImageURLs.Add($"http://{textBoxSearch.Text}{match.Groups[1]}");
                    }
                    else if (!string.IsNullOrWhiteSpace(match.Groups[1].ToString()))
                    {
                        textBoxResults.Text += $"{match.Groups[1]} {Environment.NewLine}";

                        ImageURLs.Add($"{match.Groups[1]}");
                    }
                }
            }
        }

        private async Task DownloadImagesAsync(string[] imagearray)
        {
            using var client = new HttpClient();

            foreach (var image in imagearray)
            {
                var match = FileExtensionPattern.Matches(image);
                TaskDictionary.Add(client.GetByteArrayAsync(image), match[0].Value);
            }
        }

        private async Task SaveImages(string path)
        {
            var tasks = TaskDictionary.Keys;
            var i = 1;
            while (TaskDictionary.Count > 0)
            {
                var completedTask = await Task.WhenAny(tasks);
                var fileExtension = TaskDictionary[completedTask];
                var result = await completedTask;
                using var fileStream = new FileStream($"{path}\\image{i}{fileExtension}", FileMode.Create);
                await fileStream.WriteAsync(result, 0, result.Length);
                i++;
                TaskDictionary.Remove(completedTask);
            }
        }
    }
}
