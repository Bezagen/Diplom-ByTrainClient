using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ByTrainClient.Model;

namespace ByTrainClient.Logic
{
    public class Authorization
    {
        DataBaseAdapter dataBaseAdapter = new DataBaseAdapter();
        public Authorization() { }

        public async Task<bool> LogIn(string login, string password)
        {
            SecurityService security = new SecurityService();

            string encryptedLogin = security.GetHashedText(login),
                   encryptedPassword = security.GetHashedText(password);


            object UserType = await dataBaseAdapter.GetCellAsync($"SELECT ID_User_Type From [User] Where Login = '{encryptedLogin}' AND Password = '{encryptedPassword}'");
            int GetUserType = Convert.ToInt32(UserType);

            if (GetUserType == 1)
            {
                DataTable dataTable = await dataBaseAdapter.GetTableAsync($"SELECT [User].ID, [User].Name, [User].Surname, User_Type.Name AS Type_Name From [User] JOIN User_Type ON User_Type.ID = [User].ID_User_Type WHERE [User].[Login] = '{encryptedLogin}' AND [User].[Password] = '{encryptedPassword}'");
                DataRow[] rows = dataTable.Select();

                for (int i = 0; i < rows.Length; i++)
                {
                    EnteredUser.ID = Convert.ToInt32(rows[i]["ID"]);
                    EnteredUser.Name = rows[i]["Name"].ToString();
                    EnteredUser.Surname = rows[i]["Surname"].ToString();
                    EnteredUser.Type = rows[i]["Type_Name"].ToString();
                }
            }

            EnteredUser.DateTimeEntry = DateTime.UtcNow.ToString();

            if (GetUserType == 1)
                return true;
            return false;
        }
    }
}
