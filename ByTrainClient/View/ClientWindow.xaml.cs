
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ByTrainClient.View.Pages;

namespace ByTrainClient.View
{
    public partial class ClientWindow : Window
    {
        public List<Page> pages;
        public ClientWindow()
        {
            InitializeComponent();

            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;

            pages = new List<Page>();

            pages.Add(new MainClientPage());

            FrameField.Content = pages[0];
        }

        private void MinimazeButton_Click(object sender, RoutedEventArgs e)
        { WindowState = WindowState.Minimized; }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        { Application.Current.Shutdown(); }

        private void MoveRectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (MenuGrid.Visibility == Visibility.Visible)
                MenuGrid.Visibility = Visibility.Collapsed;
            else
                MenuGrid.Visibility = Visibility.Visible;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuGrid.Visibility = Visibility.Collapsed;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ReturnToMainPage_Click(object sender, RoutedEventArgs e)
        {
            FrameField.Content = pages[0];
        }

        private void CheckPurchase_History_Click(object sender, RoutedEventArgs e)
        {
            FrameField.Content = new PurchaseHistoryPage();
        }

        private void CheckRoutePageButton_Click(object sender, RoutedEventArgs e)
        {
            FrameField.Content = new RoutesPage();
        }

        private void OpenEditUserPageButton_Click(object sender, RoutedEventArgs e)
        {
            FrameField.Content = new UserInfoPage();
        }
    }
}
