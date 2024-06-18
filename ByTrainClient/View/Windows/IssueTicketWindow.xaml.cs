using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ByTrainClient.Model;
using ByTrainClient.Logic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text.RegularExpressions;
using UglyToad.PdfPig.Writer;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Content;
using System.IO;

namespace ByTrainClient.View.Windows
{
    public partial class IssueTicketWindow : Window
    {
        private ScheduleRoute Route { get; set; }
        DataBaseAdapter dataBaseAdapter = new DataBaseAdapter();
        private ObservableCollection<Carriage> carriages = new ObservableCollection<Carriage>();

        public IssueTicketWindow(ScheduleRoute route)
        {
            InitializeComponent();

            Route = route;

            BirthCertificateTextBox.IsEnabled = false;
        }
        

        private void MinimazeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MoveRectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DeparturePointTextBlock.Text = Route.DepartureCity;
            DestinationPointTextBlock.Text = Route.DestinationCity;
            DepartureDateTextBlock.Text = Route.DateAndTimeOfDeparture;
            DestinationDateTextBlock.Text = Route.DateAndTimeOfArrival;
            TrainIDTextBlock.Text = Route.Train_ID.ToString();

            BirthdayPicker.DisplayDateEnd = DateTime.Now;

            try
            {
                TicketRateTypeBox.ItemsSource = await dataBaseAdapter.GetCollectionAsync("SELECT * FROM Ticket_Rate", 1);

                DataTable tempTable = await dataBaseAdapter.GetTableAsync($"SELECT Carriage.ID, Type_Carriage.Name FROM Carriage JOIN Type_Carriage ON Carriage.ID_Type_Carriage = Type_Carriage.ID JOIN Train ON Carriage.ID_Train = Train.ID WHERE Train.ID = {Route.Train_ID}");
                DataRow[] rows = tempTable.Select();

                for (int i = 0; i < rows.Length; i++)
                {
                    Carriage carriage = new Carriage();
                    carriage.CarriageID = (int)rows[i]["ID"];
                    carriage.Carriage_Type = (string)rows[i]["Name"];
                    carriages.Add(carriage);
                }

                CarriageBox.ItemsSource = carriages;
            }
            catch
            {
                MessageBox.Show("Не удалось получить данные из базы данных или что-то другое", "Ошибка");
            }
        }

        private void PassprotDataMaskedTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PassprotDataMaskedTextBox.Select(0, 0);
        }

        private async void CarriageBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                await UpdateSeatsComboBox();
            }
            catch (Exception)
            {
                MessageBox.Show("ОЙ");
            }
        }

        private async Task UpdateSeatsComboBox()
        {
            SeatsComboBox.Items.Clear();
            int index = CarriageBox.SelectedIndex;

            DataTable tempTable = await dataBaseAdapter.GetTableAsync($"SELECT Seat_Number FROM Seat Where ID_Carriage = {carriages[index].CarriageID} AND Status = 0");
            DataRow[] rows = tempTable.Select();

            for (int i = 0; i < rows.Length; i++)
            {
                SeatsComboBox.Items.Add(rows[i]["Seat_Number"]);
            }
        }

        private async void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TicketPrice.Text = Convert.ToString(await CalculatePrice());
            }
            catch
            {
                MessageBox.Show("Что-то пошло не так");
            }
        }

        private double bedLinelPrice = 0,
                    carriageTypePrice = 0,
                    ticketRatePrice = 0,
                    routePrice = 0;

        private async Task<double> CalculatePrice()
        {
            bedLinelPrice = 0;
            carriageTypePrice = 0;
            ticketRatePrice = 0;
            routePrice = 0;

            if (TicketRateTypeBox.SelectedItem == null)
            {
                MessageBox.Show("Нужно выбрать все параметры для подсчета цены", "Внимание");
                return 0;
            }
            else
                if (TicketRateTypeBox.SelectedValue.ToString() == "Безденежный")
                return 0;

            try
            {
                if (CarriageBox.SelectedItem != null && TicketRateTypeBox != null)
                {
                    if (BedLinenCheckBox.IsChecked == true)
                        bedLinelPrice += Convert.ToDouble(await dataBaseAdapter.GetCellAsync("SELECT Price FROM Additional_Services WHERE Name = 'Постельное белье'"));

                    carriageTypePrice += Convert.ToDouble(await dataBaseAdapter.GetCellAsync($"SELECT [Price] FROM Type_Carriage WHERE Name = '{carriages[CarriageBox.SelectedIndex].Carriage_Type}'"));
                    ticketRatePrice += Convert.ToDouble(await dataBaseAdapter.GetCellAsync($"SELECT [Price] FROM Ticket_Rate WHERE Name = '{TicketRateTypeBox.SelectedValue.ToString()}'"));
                    routePrice += Convert.ToDouble(await dataBaseAdapter.GetCellAsync($"SELECT Route.Price FROM Route JOIN City AS Depart ON Depart.ID = Route.ID_City_Of_Departure JOIN City AS Destin ON Destin.ID = Route.ID_Destination_City WHERE Depart.Name = '{Route.DepartureCity}' AND Destin.Name = '{Route.DestinationCity}'"));
                }
                else
                    MessageBox.Show("Нужно выбрать все параметры для подсчета цены", "Внимание");
            }
            catch
            {
                MessageBox.Show("Не получилось рассчитать цену", "Внимание");
            }

            return bedLinelPrice + carriageTypePrice + ticketRatePrice + routePrice;
        }

        private async void IssueTicket_Click(object sender, RoutedEventArgs e)
        {
            if (NameTextBox.Text == "")
            {
                MessageBox.Show($"Заполните поле имени", "Внимание");
                return;
            }

            if (SurnameTextBox.Text == "")
            {
                MessageBox.Show("Заполните поле фамилии", "Внимание");
                return;
            }

            if (BirthdayPicker.Text == "")
            {
                MessageBox.Show("Укажите дату рождения", "Внимание");
            }

            if (CarriageBox.SelectedItem != null && TicketRateTypeBox != null && SeatsComboBox.SelectedItem != null)
            {
                if (TicketRateTypeBox.SelectedValue.ToString() != "Безденежный" && Regex.IsMatch(PassprotDataMaskedTextBox.Text, @"_") && TicketRateTypeBox.SelectedValue.ToString() != "Детский")
                {
                    MessageBox.Show("Заполните поле паспортных данных", "Внимание");
                    return;
                }

                if (TicketRateTypeBox.SelectedValue.ToString() == "Безденежный" || TicketRateTypeBox.SelectedValue.ToString() == "Детский")
                {
                    if (Regex.IsMatch(BirthCertificateTextBox.Text, @"_"))
                    {
                        MessageBox.Show("Заполните поле свидетельства о рождении", "Внимание");
                        return;
                    }
                }

                try
                {
                    if (Convert.ToDouble(TicketPrice.Text) >= 0)
                        TicketPrice.Text = Convert.ToString(await CalculatePrice());

                    int seatStatus = Convert.ToInt32(await dataBaseAdapter.GetCellAsync($"SELECT Status FROM Seat WHERE Seat_Number = {Convert.ToInt32(SeatsComboBox.SelectedValue)} AND ID_Carriage = {carriages[CarriageBox.SelectedIndex].CarriageID}"));

                    switch (seatStatus)
                    {
                        case 0:
                            await dataBaseAdapter.ExecuteQueryAsync($"UPDATE Seat SET Status = 1 WHERE Seat_Number = {Convert.ToInt32(SeatsComboBox.SelectedValue)} AND ID_Carriage = {carriages[CarriageBox.SelectedIndex].CarriageID}");
                            break;
                        case 1:
                            await UpdateSeatsComboBox();
                            MessageBox.Show("Это место уже занято", "Ошибка");
                            return;
                        default:
                            return;
                    }
                    MessageBoxResult result = MessageBox.Show("Ожидает оплаты\nНажимите Да если оплата была произведена\nНет если оплата не произведена", "", MessageBoxButton.YesNo);

                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            break;
                        default:
                            await dataBaseAdapter.ExecuteQueryAsync($"UPDATE Seat SET Status = 0 WHERE Seat_Number = {Convert.ToInt32(SeatsComboBox.SelectedValue)} AND ID_Carriage = {carriages[CarriageBox.SelectedIndex].CarriageID}");
                            MessageBox.Show("Операция отменена", "Внимание");
                            return;
                    }

                    int passengerID = 0;

                    if (await dataBaseAdapter.GetCellAsync($"SELECT PassportData FROM Passenger WHERE PassportData = '{PassprotDataMaskedTextBox.Text}'") == null && TicketRateTypeBox.SelectedValue.ToString() != "Безденежный" && TicketRateTypeBox.SelectedValue.ToString() != "Детский")
                    {
                        await dataBaseAdapter.ExecuteQueryAsync($"INSERT INTO Passenger (Name, Surname, Patronymic, DateOfBirth, PassportData) VALUES ('{NameTextBox.Text}', '{SurnameTextBox.Text}', '{PatronymicTextBox.Text}', '{BirthdayPicker}', '{PassprotDataMaskedTextBox.Text}')");
                    }

                    if (await dataBaseAdapter.GetCellAsync($"SELECT BirthCertificateData FROM Passenger WHERE BirthCertificateData = '{BirthCertificateTextBox.Text}'") == null && (TicketRateTypeBox.SelectedValue.ToString() == "Безденежный" || TicketRateTypeBox.SelectedValue.ToString() == "Детский"))
                    {
                        await dataBaseAdapter.ExecuteQueryAsync($"INSERT INTO Passenger (Name, Surname, Patronymic, DateOfBirth, BirthCertificateData) VALUES ('{NameTextBox.Text}', '{SurnameTextBox.Text}', '{PatronymicTextBox.Text}', '{BirthdayPicker}', '{BirthCertificateTextBox.Text}')");
                    }

                    if (PassprotDataMaskedTextBox.IsEnabled == true)
                        passengerID = Convert.ToInt32(await dataBaseAdapter.GetCellAsync($"SELECT ID FROM Passenger WHERE PassportData = '{PassprotDataMaskedTextBox.Text}'"));

                    if (BirthCertificateTextBox.IsEnabled == true)
                        passengerID = Convert.ToInt32(await dataBaseAdapter.GetCellAsync($"SELECT ID FROM Passenger WHERE BirthCertificateData = '{BirthCertificateTextBox.Text}'"));

                    int scheduleID = Convert.ToInt32(await dataBaseAdapter.GetCellAsync($"SELECT Schedule.ID FROM Schedule WHERE ID_Train = {Route.Train_ID}"));

                    int ticketRateID = Convert.ToInt32(await dataBaseAdapter.GetCellAsync($"SELECT ID FROM Ticket_Rate WHERE Name = '{TicketRateTypeBox.SelectedValue.ToString()}'"));

                    await dataBaseAdapter.ExecuteQueryAsync($"INSERT INTO [Ticket] (ID_Schedule, ID_User, ID_Passenger, ID_Ticket_Rate, Carriage_Number, Seat_Number) VALUES ({scheduleID}, {EnteredUser.ID}, {passengerID}, {ticketRateID}, {carriages[CarriageBox.SelectedIndex].CarriageID}, {SeatsComboBox.SelectedValue})");

                    int ticketID = Convert.ToInt32(await dataBaseAdapter.GetCellAsync($"SELECT MAX(ID) FROM Ticket"));

                    switch (BedLinenCheckBox.IsChecked)
                    {
                        case true:
                            await dataBaseAdapter.ExecuteQueryAsync($"INSERT INTO [Connected_Services] (Ticket_ID, Additional_Services_ID) VALUES ({ticketID}, 1)");
                            break;
                        default:
                            break;
                    }

                    CreatePDFDoucment();

                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex}", "Ошибка");
                }
            }
        }

        private void CreatePDFDoucment()
        {
            PdfDocumentBuilder builder = new PdfDocumentBuilder();
            PdfPageBuilder page = builder.AddPage(PageSize.A4);

            string bytesPath = @"C:\Windows\Fonts\consola.ttf";
            byte[] fontBytes = File.ReadAllBytes(bytesPath);

            PdfDocumentBuilder.AddedFont font = builder.AddTrueTypeFont(fontBytes);

            page.AddText($"КОНТРОЛЬНЫЙ КУПОН", 25, new PdfPoint(25, 800), font);
            page.DrawLine(new PdfPoint(20, 795), new PdfPoint(400, 795));
            page.AddText($"Поезд: {Route.Train_ID}  Вагон: {carriages[CarriageBox.SelectedIndex].CarriageID}  Место: {SeatsComboBox.SelectedValue.ToString()}", 14, new PdfPoint(25, 770), font);
            page.AddText($"{NameTextBox.Text} {SurnameTextBox.Text} {PatronymicTextBox.Text}", 14, new PdfPoint(25, 740), font);

            if (PassprotDataMaskedTextBox.IsEnabled == true)
                page.AddText($"{PassprotDataMaskedTextBox.Text}", 12, new PdfPoint(25, 720), font);

            if (BirthCertificateTextBox.IsEnabled == true)
                page.AddText($"{BirthCertificateTextBox.Text}", 12, new PdfPoint(25, 720), font);

            page.AddText($"Место отправления: {Route.DepartureCity}", 14, new PdfPoint(25, 690), font);
            page.AddText($"Дата и время отправления: {Route.DateAndTimeOfDeparture}", 14, new PdfPoint(25, 670), font);
            page.AddText($"Место прибытия: {Route.DestinationCity}", 14, new PdfPoint(25, 640), font);
            page.AddText($"Дата и время прибытия: {Route.DateAndTimeOfArrival}", 14, new PdfPoint(25, 620), font);
            page.AddText($"МСК часовой пояс", 12, new PdfPoint(25, 660), font);
            page.AddText($"Тариф билета: {TicketRateTypeBox.SelectedValue}  ", 14, new PdfPoint(25, 580), font);
            page.AddText($"Класс вагона: {carriages[CarriageBox.SelectedIndex].Carriage_Type}", 14, new PdfPoint(25, 560), font);

            string bedLinen = "Нет";
            switch (BedLinenCheckBox.IsChecked)
            {
                case true:
                    bedLinen = "Есть";
                    break;
                default:
                    break;
            }

            page.AddText($"Постельное бельё: {bedLinen}", 14, new PdfPoint(25, 550), font);
            page.DrawLine(new PdfPoint(20, 500), new PdfPoint(400, 500));
            page.AddText($"Оформлен: {DateTime.Now}", 14, new PdfPoint(25, 480), font);
            page.AddText($"Цена состоит из", 14, new PdfPoint(25, 460), font);
            page.AddText($"Постельное бельё: {bedLinelPrice}", 14, new PdfPoint(25, 440), font);
            page.AddText($"Класс вагона: {carriageTypePrice}", 14, new PdfPoint(25, 420), font);
            page.AddText($"Тип билета: {ticketRatePrice}", 14, new PdfPoint(25, 400), font);
            page.AddText($"Стоимость маршрута: {routePrice}", 14, new PdfPoint(25, 380), font);
            page.AddText($"Информация о перевозчике (пример)", 12, new PdfPoint(300, 380), font);


            byte[] documentBytes = builder.Build();
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            DateTime date = DateTime.Now;

            File.WriteAllBytes($@"{path}\Ticket-{Route.DepartureCity}-{Route.DestinationCity}-{date.ToString("dd-MM-yyyy-hh-mm")}.pdf", documentBytes);
        }

        private void TicketRateTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TicketRateTypeBox.SelectedItem != null)
                if (TicketRateTypeBox.SelectedValue.ToString() == "Безденежный" || TicketRateTypeBox.SelectedValue.ToString() == "Детский")
                {
                    PassprotDataMaskedTextBox.IsEnabled = false;
                    BirthCertificateTextBox.IsEnabled = true;
                }
                else
                {
                    PassprotDataMaskedTextBox.IsEnabled = true;
                    BirthCertificateTextBox.IsEnabled = false;
                }
        }
    }
}
