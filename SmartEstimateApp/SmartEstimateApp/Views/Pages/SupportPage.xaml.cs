using OpenAIService.Interfaces;
using System.Windows;
using System.Windows.Controls;

namespace SmartEstimateApp.Views.Pages
{
    public partial class SupportPage : Page
    {
        private readonly IOpenAiService _openAiService;

        public SupportPage(IOpenAiService openAiService)
        {
            InitializeComponent();
            _openAiService = openAiService;
        }

        private async void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            string userInput = InputBox.Text;
            if (string.IsNullOrWhiteSpace(userInput)) return;

            MessagesPanel.Items.Add("Вы: " + userInput);
            InputBox.Text = "";

            try
            {
                string response = await _openAiService.AskAsync(userInput);
                MessagesPanel.Items.Add("AI: " + response);
            }
            catch (Exception ex)
            {
                MessagesPanel.Items.Add("Ошибка: " + ex.Message);
            }
        }
    }
}
