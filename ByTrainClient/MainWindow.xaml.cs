using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ByTrainClient.Logic;
using ByTrainClient.View;

namespace ByTrainClient
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MoveRectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MinimazeButton_Click_1(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private async void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            Authorization authorization = new Authorization();
             bool result = await authorization.LogIn(LoginTextBox.Text, PasswordTextBox.Password);

            if (result == true)
            {
                ClientWindow clientWindow = new ClientWindow();
                clientWindow.Show();
                this.Hide();
            }
            else
                MessageBox.Show("Неверные данные авторизации", "Ошибка");
        }
    }
}
