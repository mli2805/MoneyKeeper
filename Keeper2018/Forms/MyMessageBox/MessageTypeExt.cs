using System.Windows;

namespace Keeper2018
{
    public static class MessageTypeExt
    {
        public static string GetLocalizedString(this MessageType messageType)
        {
            switch (messageType)
            {
                case MessageType.Error: return "Ошибка";
                case MessageType.Information: return "Информация";
                case MessageType.Confirmation: return "Подтверждение";
                case MessageType.LongOperation: return "Длительная операция. Пожалуйста, подождите...";
                default: return "Сообщение";
            }
        }

        public static Visibility ShouldOkBeVisible(this MessageType messageType)
        {
            return messageType == MessageType.LongOperation ? Visibility.Collapsed : Visibility.Visible;
        }

        public static Visibility ShouldCancelBeVisible(this MessageType messageType)
        {
            return messageType == MessageType.Confirmation ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}