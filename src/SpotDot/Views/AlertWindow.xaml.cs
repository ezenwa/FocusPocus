using System.Windows;
using SpotDot.Services;

namespace SpotDot.Views;

public partial class AlertWindow : Wpf.Ui.Controls.FluentWindow
{
    public AlertWindow(string message, Localizer localizer)
    {
        InitializeComponent();
        MessageText.Text = message;
        AcceptButton.Content = localizer["Accept"];
    }

    private void AcceptClicked(object sender, RoutedEventArgs e) => Close();
}
