using System.Windows;
using SpotDot.Services;

namespace SpotDot.Views;

public partial class AboutWindow : Wpf.Ui.Controls.FluentWindow
{
    public AboutWindow(Localizer localizer)
    {
        InitializeComponent();
        TaglineText.Text = localizer["AppSubtitle"];
        VersionText.Text = $"{localizer["Version"]}: 1.1.13";
        AuthorText.Text = $"{localizer["Author"]}: Joshua Ezenwa";
        CloseButton.Content = localizer["Close"];
    }

    private void CloseClicked(object sender, RoutedEventArgs e) => Close();
}
