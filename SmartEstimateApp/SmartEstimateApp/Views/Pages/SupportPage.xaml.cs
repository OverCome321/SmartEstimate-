using AutoMapper;
using Bl.Interfaces;
using OpenAIService.Interfaces;
using SmartEstimateApp.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace SmartEstimateApp.Views.Pages
{
    public partial class SupportPage : Page
    {
        private SupportPageViewModel ViewModel { get; set; }

        public SupportPage(IOpenAiService openAiService, IMapper mapper, HomeWindowViewModel homeWindowViewModel, IChatBL chatBL)
        {
            InitializeComponent();
            ViewModel = new SupportPageViewModel(chatBL, openAiService, mapper, homeWindowViewModel);
            this.DataContext = ViewModel;

            if (ViewModel != null)
            {
                ViewModel.Messages.CollectionChanged += Messages_CollectionChanged;
            }
        }

        private void Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                MessageScrollViewer.ScrollToEnd();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                e.Handled = true;
                if (ViewModel?.SendMessageCommand.CanExecute(null) == true)
                {
                    ViewModel.SendMessageCommand.Execute(null);
                }
            }
        }
    }
}
