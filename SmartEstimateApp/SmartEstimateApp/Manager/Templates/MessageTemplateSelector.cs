using SmartEstimateApp.Models;
using System.Windows;
using System.Windows.Controls;

namespace SmartEstimateApp.Manager.Templates;

public class MessageTemplateSelector : DataTemplateSelector
{
    public DataTemplate UserTemplate { get; set; }
    public DataTemplate AssistantTemplate { get; set; }
    private const long BotUserId = 1;

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is Message msg)
        {
            return msg.SenderUserId == BotUserId
                ? AssistantTemplate
                : UserTemplate;
        }
        return base.SelectTemplate(item, container);
    }
}
