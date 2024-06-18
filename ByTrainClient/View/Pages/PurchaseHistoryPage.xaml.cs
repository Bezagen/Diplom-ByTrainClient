using System.Data;
using System.Windows;
using System.Windows.Controls;
using ByTrainClient.Logic;
using ByTrainClient.Model;

namespace ByTrainClient.View.Pages
{
    public partial class PurchaseHistoryPage : Page
    {
        DataBaseAdapter dataBaseAdapter = new DataBaseAdapter();
        public PurchaseHistoryPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DataTable historyTable = await dataBaseAdapter.GetTableAsync($"SELECT Purchase_History.DateAndime AS [Дата покупки], Ticket.Carriage_Number as [Номер вагона], Ticket.Seat_Number as [Номер места], DepartPoint.Name as [Город отправления], DestinPoint.Name as [Город прибытия] FROM Purchase_History JOIN Ticket ON Purchase_History.ID_Ticket = Ticket.ID JOIN Schedule ON Ticket.ID_Schedule = Schedule.ID JOIN Route ON Schedule.ID_Route = Route.ID JOIN City AS DepartPoint ON Route.ID_City_Of_Departure = DepartPoint.ID JOIN City AS DestinPoint ON Route.ID_Destination_City = DestinPoint.ID WHERE Ticket.ID_User = {EnteredUser.ID}");
            PurchaseHistoryDataGrid.ItemsSource = historyTable.DefaultView;
        }
    }
}
