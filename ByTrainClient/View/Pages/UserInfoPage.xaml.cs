using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ByTrainClient.Logic;
using ByTrainClient.Model;
using ByTrainClient.View.Windows;

namespace ByTrainClient.View.Pages
{
    public partial class UserInfoPage : Page
    {
        DataBaseAdapter dataBaseAdapter = new DataBaseAdapter();
        public UserInfoPage()
        {
            InitializeComponent();
        }

        private void BirtDatePicker_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!DateTime.TryParse(e.Text, out _))
            {
                e.Handled = true;
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DataTable userTable = await dataBaseAdapter.GetTableAsync($"SELECT [User].Name, [User].[Surname], [User].[Patronymic], [User].[DateOfBirth] FROM [User] WHERE [User].[ID] = {EnteredUser.ID}");
            DataRow[] rows = userTable.Select();

            UserNameTextBox.Text = rows[0]["Name"].ToString();
            UserSurnameTextBox.Text = rows[0]["Surname"].ToString();
            UserPtronymicTextBox.Text = rows[0]["Patronymic"].ToString();
            BirtDatePicker.Text = rows[0]["DateOfBirth"].ToString();

            BirtDatePicker.DisplayDateEnd = DateTime.Now;
        }

        private async void EditUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserNameTextBox.Text == "")
            {
                MessageBox.Show("Введите имя", "Внимание");
                return;
            }

            if (UserSurnameTextBox.Text == "")
            {
                MessageBox.Show("Введите фамилию", "Внимание");
                return;
            }

            if (BirtDatePicker.Text == "")
            {
                MessageBox.Show("Введите дату рождения", "Внимание");
                return;
            }

            MessageBoxResult result = MessageBox.Show("Вы уверены что хотите изменить данные?", "Внимание", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.No)
                return;

            _ = await dataBaseAdapter.ExecuteQueryAsync($"UPDATE [User] Set [Name] = '{UserNameTextBox.Text}', [Surname] = '{UserSurnameTextBox.Text}', [Patronymic] = '{UserPtronymicTextBox.Text}', [DateOfBirth] = '{BirtDatePicker.Text}' WHERE [User].ID = {EnteredUser.ID}");

            EnteredUser.Name = UserNameTextBox.Text;
            EnteredUser.Surname = UserSurnameTextBox.Text;

            MessageBox.Show("Сохранено", "Внимание");
        }

        private void EditPasswordAndLoginButton_Click(object sender, RoutedEventArgs e)
        {
            EditPasswordAndLogin editPasswordAndLogin = new EditPasswordAndLogin();
            editPasswordAndLogin.ShowDialog();
        }
    }
}
