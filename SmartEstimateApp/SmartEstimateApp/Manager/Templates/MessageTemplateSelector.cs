using SmartEstimateApp.Models;
using System.Windows;
using System.Windows.Controls;

namespace SmartEstimateApp.Manager.Templates;

public class MessageTemplateSelector : DataTemplateSelector
{
    public DataTemplate UserTemplate { get; set; }
    public DataTemplate AssistantTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is ChatMessage message)
        {
            return message.IsFromUser ? UserTemplate : AssistantTemplate;
        }

        return base.SelectTemplate(item, container);
    }
}
