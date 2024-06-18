using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ByTrainClient.Model;
using ByTrainClient.View.Windows;

namespace ByTrainClient.View.Pages
{
    public partial class SchedulePage : Page
    {
        ObservableCollection<ScheduleRoute> scheduleRoutesCollection = new ObservableCollection<ScheduleRoute>();
        public SchedulePage(ObservableCollection<ScheduleRoute> scheduleRoutes)
        {
            InitializeComponent();

            scheduleRoutesCollection = scheduleRoutes;
            ScheduleRoutesListBox.ItemsSource = scheduleRoutesCollection;
        }

        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            object tag = (sender as FrameworkElement).Tag;
            int index = ScheduleRoutesListBox.Items.IndexOf(tag);

            IssueTicketWindow routeWindow = new IssueTicketWindow(scheduleRoutesCollection[index]);
            routeWindow.ShowDialog();

            NavigationService.GoBack();
        }
    }
}
