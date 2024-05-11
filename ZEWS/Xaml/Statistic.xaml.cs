using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ZEWS.Xaml
{
    /// <summary>
    /// Логика взаимодействия для Statistic.xaml
    /// </summary>
    public partial class Statistic : Window
    {
        public MainWindow mainWindow;
        private string accessToken = Properties.Settings.Default.Token;
        private readonly HttpClient client;
        public Statistic(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();
            LoadRoomTypes();
        }
        private async void LoadRoomTypes()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync($"{APIconfig.APIurl}/room/types");
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    // Десериализация ответа
                    var roomTypes = JsonConvert.DeserializeObject<string[]>(responseBody);
                    // Добавление элементов в ComboBox
                    foreach (var type in roomTypes)
                    {
                        roomTypeComboBox.Items.Add(type);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке типов номеров: {ex.Message}");
            }
        }

        private async void RoomTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (roomTypeComboBox.SelectedItem != null)
            {
                string selectedRoomType = roomTypeComboBox.SelectedItem.ToString();
                await LoadSalesStatistics(selectedRoomType);
            }
        }

        private async Task LoadSalesStatistics(string roomType)
        {
            try
            {
                DateTime today = DateTime.Today;

                // Запрос на статистику продаж за сегодня
                HttpResponseMessage responseDay = await client.GetAsync($"{APIconfig.APIurl}/stats?type={roomType}&date={today.ToShortDateString()}");
                responseDay.EnsureSuccessStatusCode();
                string responseBodyDay = await responseDay.Content.ReadAsStringAsync();
                // Десериализация ответа
                var statisticDay = JsonConvert.DeserializeObject<Statistic>(responseBodyDay);
                // Обновление текстового блока для продаж за сегодня
                salesDayTextBlock.Text = statisticDay.ToString();

                // Вычитаем один день из текущей даты
                DateTime yesterday = today.AddDays(-1);
                // Запрос на статистику продаж за вчера
                HttpResponseMessage responseYesterday = await client.GetAsync($"{APIconfig.APIurl}/stats?type={roomType}&date={yesterday.ToShortDateString()}");
                responseYesterday.EnsureSuccessStatusCode();
                string responseBodyYesterday = await responseYesterday.Content.ReadAsStringAsync();

                // Вычитаем один месяц из текущей даты
                DateTime lastMonth = today.AddMonths(-1);
                // Запрос на статистику продаж за месяц назад
                HttpResponseMessage responseMonth = await client.GetAsync($"{APIconfig.APIurl}/stats?type={roomType}&date={lastMonth.ToShortDateString()}");
                responseMonth.EnsureSuccessStatusCode();
                string responseBodyMonth = await responseMonth.Content.ReadAsStringAsync();
                // Десериализация ответа
                var statisticMonth = JsonConvert.DeserializeObject<Statistic>(responseBodyMonth);
                // Обновление текстового блока для продаж за месяц назад
                salesMonthTextBlock.Text = statisticMonth.ToString();

                // Вычитаем один год из текущей даты
                DateTime lastYear = today.AddYears(-1);
                // Запрос на статистику продаж за год назад
                HttpResponseMessage responseYear = await client.GetAsync($"{APIconfig.APIurl}/stats?type={roomType}&date={lastYear.ToShortDateString()}");
                responseYear.EnsureSuccessStatusCode();
                string responseBodyYear = await responseYear.Content.ReadAsStringAsync();
                // Десериализация ответа
                var statisticYear = JsonConvert.DeserializeObject<Statistic>(responseBodyYear);
                // Обновление текстового блока для продаж за год назад
                salesYearTextBlock.Text = statisticYear.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке статистики продаж: {ex.Message}");
            }
        }
    }
}
