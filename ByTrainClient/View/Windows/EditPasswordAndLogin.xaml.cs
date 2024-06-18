
using System.Windows;
using System.Windows.Input;
using ByTrainClient.Logic;
using ByTrainClient.Model;

namespace ByTrainClient.View.Windows
{
    public partial class EditPasswordAndLogin : Window
    {
        DataBaseAdapter dataBaseAdapter = new DataBaseAdapter();
        public EditPasswordAndLogin()
        {
            InitializeComponent();
        }

        private void MoveRectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (LoginTextBox.Text == "")
            {
                MessageBox.Show("Введите новый логин", "Внимание");
                return;
            }

            if (PasswordTextBox.Text == "")
            {
                MessageBox.Show("Введите новый пароль", "Внимание");
                return;
            }

            MessageBoxResult result = MessageBox.Show("Вы действительно хотите изменить логин и пароль?", "Внимание", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.No)
                return;

            SecurityService securityService = new SecurityService();

            string encryptedLogin = securityService.GetHashedText(LoginTextBox.Text);
            string encryptedPassword = securityService.GetHashedText(PasswordTextBox.Text);

            if (await dataBaseAdapter.GetCellAsync($"SELECT * FROM [User] WHERE [User].[Login] = '{encryptedLogin}' AND [User].[ID] = {EnteredUser.ID}") == null)
                if (await dataBaseAdapter.GetCellAsync($"SELECT * FROM [User] WHERE [User].[Login] = '{encryptedLogin}'") != null)
                {
                    MessageBox.Show("Такой логин уже существует, попробуйте другой", "Внимание");
                    return;
                }

            _ = await dataBaseAdapter.ExecuteQueryAsync($"UPDATE [User] SET [Login] = '{encryptedLogin}', [Password] = '{encryptedPassword}' WHERE [User].ID = {EnteredUser.ID}");

            MessageBox.Show("Сохранено", "Внимание");

            this.Close();
        }
    }
}
