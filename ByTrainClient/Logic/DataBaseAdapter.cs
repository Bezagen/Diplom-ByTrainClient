using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using ByTrainClient.Properties;

namespace ByTrainClient.Logic
{
    class DataBaseAdapter
    {
        public static SqlConnection connection = new SqlConnection($"{Settings.Default.ConnectionString}");

        public DataBaseAdapter() { }

        /// <summary>
        /// Возвращает коллекцию строк указанного столбца.
        /// </summary>
        /// <param name="query">Запрос</param>
        /// <returns>Коллекция типа ObservableCollection string</returns>
        public async Task<ObservableCollection<string>> GetCollectionAsync(string query, int columnIndex)
        {
            ObservableCollection<string> tempCollection = new ObservableCollection<string>();

            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync();

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            tempCollection.Add(reader.GetString(columnIndex));
                        }
                    }
                }
            }

            connection.Close();

            return tempCollection;
        }

        /// <summary>
        /// Выполняет запрос на получение ячейки.
        /// </summary>
        /// <returns>Возвращает первую ячейку полученной таблицы</returns>
        public async Task<object> GetCellAsync(string query)
        {
            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync();

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                try
                {
                    object result = await command.ExecuteScalarAsync();
                    connection.Close();
                    return result;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex}", "Ошибка");
                    connection.Close();
                    return -1;
                }
            }
        }

        /// <summary>
        /// Выполняет запрос.
        /// </summary>
        /// <param name="query">Запрос</param>
        /// <returns>Возвращает 1 при успешном выполнении и -1 при неудачном</returns>
        public async Task<int> ExecuteQueryAsync(string query)
        {
            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync();

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                try
                {
                    await command.ExecuteNonQueryAsync();
                    connection.Close();
                    return 1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex}", "Ошибка");
                    connection.Close();
                    return -1;
                }
            }
        }

        /// <summary>
        /// Выполняет запрос на получения таблицы.
        /// </summary>
        /// <param name="query">Запрос.</param>
        /// <returns>Возвращает таблицу в формате DataTable</returns>
        public async Task<DataTable> GetTableAsync(string query)
        {
            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync();

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    using (DataTable table = new DataTable())
                    {
                        try
                        {
                            adapter.Fill(table);
                            await command.ExecuteNonQueryAsync();
                            return table;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"{ex}", "Ошибка");
                            return null;
                        }
                    }
                }
            }
        }
    }
}
