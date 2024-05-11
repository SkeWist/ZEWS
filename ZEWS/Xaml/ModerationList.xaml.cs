using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
using ZEWS.Class;

namespace ZEWS.Xaml
{
    /// <summary>
    /// Логика взаимодействия для ModerationList.xaml
    /// </summary>
    public partial class ModerationList : Page
    {
        private MainWindow mainWindow;
        private string accessToken = Properties.Settings.Default.Token;
        private readonly HttpClient client = new HttpClient();

        public ModerationList(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            mainWindow.Height = 550;
            mainWindow.Width = 800;
            LoadReviewsAsync();
        }
        private async void LoadReviewsAsync()
        {
            try
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                HttpResponseMessage response = await client.GetAsync(APIconfig.APIurl + "/reviews");

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseBody); // Выводим JSON-ответ на консоль

                    JObject jsonResponse = JObject.Parse(responseBody);

                    // Проверяем тип JSON-объекта
                    if (jsonResponse["reviews"] is JArray reviewArray)
                    {
                        // Если это массив отзывов, десериализуем его в список
                        List<Review> reviews = reviewArray.ToObject<List<Review>>();

                        if (reviews != null && reviews.Any())
                        {
                            // Устанавливаем список отзывов в качестве источника данных для ListBox
                            reviewsListBox.ItemsSource = reviews;
                        }
                        else
                        {
                            MessageBox.Show("Отзывы не найдены");
                        }
                    }
                    else if (jsonResponse["reviews"] is JObject singleReview)
                    {
                        // Если это одиночный отзыв, десериализуем его в одиночный объект отзыва
                        Review review = singleReview.ToObject<Review>();

                        // Создаем список с одним отзывом
                        List<Review> reviews = new List<Review> { review };

                        // Устанавливаем список отзывов в качестве источника данных для ListBox
                        reviewsListBox.ItemsSource = reviews;
                    }
                    else
                    {
                        MessageBox.Show("Отзывы не найдены");
                    }
                }
                else
                {
                    MessageBox.Show("Ошибка при получении данных: " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем ListBoxItem, содержащий нажатую кнопку
            var item = reviewsListBox.ItemContainerGenerator.ContainerFromItem((sender as FrameworkElement).DataContext) as ListBoxItem;
            if (item != null)
            {
                // Получаем отзыв из контекста элемента ListBoxItem
                Review review = item.DataContext as Review;
                if (review != null)
                {
                    // Удаляем отзыв из списка
                    (reviewsListBox.ItemsSource as List<Review>).Remove(review);

                    // Удаляем отзыв из API
                    bool success = await DeleteReviewAsync(review);
                    if (success)
                    {
                        // Успешно удалено, обновляем страницу
                        LoadReviewsAsync();
                    }
                }
            }
        }
        private async void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем ListBoxItem, содержащий нажатую кнопку
            var item = reviewsListBox.ItemContainerGenerator.ContainerFromItem((sender as FrameworkElement).DataContext) as ListBoxItem;
            if (item != null)
            {
                // Получаем отзыв из контекста элемента ListBoxItem
                Review review = item.DataContext as Review;
                if (review != null)
                {
                    // Устанавливаем значение moderated равным 1
                    review.Moderated = 1;

                    // Отправляем запрос на обновление отзыва на сервер
                    bool success = await UpdateReviewAsync(review);
                    if (success)
                    {
                        // Успешно обновлено
                        // Можно добавить обновление интерфейса или какое-либо уведомление об успешном изменении
                        MessageBox.Show("Отзыв успешно принят.");
                    }
                    else
                    {
                        // Обработка ошибки
                        // Возможно, показать сообщение об ошибке или выполнить другие действия
                        MessageBox.Show("Ошибка при обновлении отзыва.");
                    }
                }
            }
        }
        private async Task<bool> UpdateReviewAsync(Review review)
        {
            try
            {
                // Создаем объект, который будет содержать обновленные данные отзыва
                var updatedReview = new
                {
                    Moderated = review.Moderated
                };

                // Преобразуем объект в JSON
                string json = JsonConvert.SerializeObject(updatedReview);

                // Создаем контент запроса
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Отправляем PATCH-запрос на сервер для обновления отзыва
                HttpResponseMessage response = await client.PostAsync($"{APIconfig.APIurl}/reviews/{review.Id}", content);

                // Проверяем успешность запроса
                if (response.IsSuccessStatusCode)
                {
                    // Отзыв успешно обновлен
                    return true;
                }
                else
                {
                    // Обработка ошибки
                    string errorMessage = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Ошибка при обновлении отзыва: {errorMessage}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении отзыва: {ex.Message}");
                return false;
            }
        }
        private async Task<bool> DeleteReviewAsync(Review review)
        {
            try
            {
                // Отправляем DELETE-запрос на сервер для удаления отзыва
                HttpResponseMessage response = await client.DeleteAsync($"{APIconfig.APIurl}/reviews/{review.Id}");

                // Проверяем успешность запроса
                if (response.IsSuccessStatusCode)
                {
                    // Отзыв успешно удален
                    return true;
                }
                else
                {
                    // Обработка ошибки
                    string errorMessage = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Ошибка при удалении отзыва: {errorMessage}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении отзыва: {ex.Message}");
                return false;
            }
        }
        private void BackButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            FrameManager.MainFrame.GoBack();
            mainWindow.Height = 550;
            mainWindow.Width = 800;
        }

    }
}
