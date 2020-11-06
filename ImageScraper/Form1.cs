using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageScraper
{
    public partial class MainForm : Form
    {
        private List<string> ImageURLs = new List<string>();
        public MainForm()
        {
            InitializeComponent();
            UrlPattern = new Regex("<img.*src=\"(.*?\\.(jpg|jpeg|png|gif|bmp).*?)\"");
            FileExtensionPattern = new Regex("\\.(jpg|jpeg|png|gif|bmp)");
        }

        private Regex UrlPattern { get; set; }
        private Regex FileExtensionPattern { get; set; }
        private Dictionary<Task<byte[]>, string> TaskDictionary { get; set; } = new Dictionary<Task<byte[]>, string>();

        private async void buttonSearch_Click(object sender, EventArgs e)
        {
            ImageURLs.Clear();
            await DownloadHTMLAsync();
        }

        private void textBoxResults_TextChanged(object sender, EventArgs e)
        {
            var result = textBoxResults.Lines.Length == 0 ? $"No images found." :
                $"{textBoxResults.Lines.Length - 1} images found.";

            buttonSave.Enabled = textBoxResults.Lines.Length != 0;
            labelImages.Visible = true;
            labelImages.Text = result;
        }

        private async void buttonSave_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog();

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            await DownloadAndSaveImages(ImageURLs.ToArray(), dialog.SelectedPath);
        }

        private async Task DownloadHTMLAsync()
        {
            textBoxResults.Clear();

            if (string.IsNullOrWhiteSpace(textBoxSearch.Text))
            {
                return;
            }

            var url = textBoxSearch.Text;
            if (!url.Contains("http://"))
            {
                url = $"http://{url}";
            }

            using var client = new HttpClient();

            var downloadedHTMLCode = client.GetStringAsync(url);
            await downloadedHTMLCode;

            var matches = UrlPattern.Matches(downloadedHTMLCode.Result);

            foreach (Match match in matches)
            {
                var matchValue = match.Groups[1].ToString();
                if (string.IsNullOrWhiteSpace(matchValue))
                {
                    continue;
                }

                var urlWithProtocol = "";
                if (!matchValue.Contains("http"))
                {
                    urlWithProtocol = url;
                }

                textBoxResults.Text += $"{urlWithProtocol}{matchValue}{Environment.NewLine}";
                ImageURLs.Add($"{urlWithProtocol}{matchValue}");
            }
        }

        private async Task DownloadAndSaveImages(string[] imagearray, string path)
        {
            using var client = new HttpClient();

            foreach (var image in imagearray)
            {
                var match = FileExtensionPattern.Matches(image);
                TaskDictionary.Add(client.GetByteArrayAsync(image), match[0].Value);
            }

            var i = 1;
            var tasks = TaskDictionary.Keys;
            while (TaskDictionary.Count > 0)
            {
                var completedTask = await Task.WhenAny(tasks);
                var fileExtension = TaskDictionary[completedTask];
                var result = completedTask.Result;
                using var fileStream = new FileStream($"{path}\\image{i}{fileExtension}", FileMode.Create);
                await fileStream.WriteAsync(result, 0, result.Length);
                i++;
                TaskDictionary.Remove(completedTask);
            }
        }
    }
}
