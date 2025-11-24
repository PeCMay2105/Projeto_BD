using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace patrimonioDB.Features.Home
{
    public sealed partial class HomeView : Page
    {
        public HomeView()
        {
            this.InitializeComponent();
        }

        private void CadastrarButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(patrimonioDB.Features.CadastrarAdm.CadastroAdmView));
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame?.Navigate(typeof(patrimonioDB.Features.Login.LoginView));
        }

        private void SairButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }
    }
}
