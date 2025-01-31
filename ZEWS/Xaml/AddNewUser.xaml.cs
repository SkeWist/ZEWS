﻿using Newtonsoft.Json;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Text;
using System.Windows;
using System;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Data;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using ZEWS.Class;
using System.Windows.Input;
using System.Reflection;

namespace ZEWS
{
    /// <summary>
    /// Логика взаимодействия для AddNewUser.xaml
    /// </summary>
    public partial class AddNewUser : Page
    {
        private string accessToken = Properties.Settings.Default.Token;

        private MainWindow mainWindow;

        public AddNewUser(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            mainWindow.UpdateWindowTitle("Добавление пользователя");
            mainWindow.Height = 450;
            mainWindow.Width = 1100;
            roleComboBox.ItemsSource = new List<Role>() {
                new Role("admin", "Администратор"),
                new Role("manager", "Менеджер"),
                new Role("user", "Пользователь")
            };
            roleComboBox.DisplayMemberPath = "Name";
            roleComboBox.SelectedIndex = 0;
            sexComboBox.ItemsSource = new List<Sex>()
            {
                new Sex("male", "Мужчина"),
                new Sex("female", "Женщина")
            };
            sexComboBox.DisplayMemberPath = "Name";
            sexComboBox.SelectedIndex = 0;
            name.PreviewTextInput += TextBox_PreviewTextInput;
            surname.PreviewTextInput += TextBox_PreviewTextInput;
            patronymic.PreviewTextInput += TextBox_PreviewTextInput;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Проверяем, что вводимый символ является буквой
            if (!Regex.IsMatch(e.Text, @"^[а-яА-Яa-zA-Z]+$"))
            {
                // Если вводимый символ не является буквой, отменяем его
                e.Handled = true;
            }
        }
        private void Phone_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Проверяем, что вводимый символ является цифрой или символом '_'
            if (!char.IsDigit(e.Text, 0) && e.Text != "_")
            {
                // Если вводимый символ не является цифрой или символом '_', отменяем его
                e.Handled = true;
            }
        }
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка длины пароля
            if (password.Text.Length < 8)
            {
                MessageBox.Show("Пароль должен содержать не менее 8 символов!");
                return;
            }

            // Проверка совпадения пароля и его повтора
            if (password.Text != passwordRepeat.Text)
            {
                MessageBox.Show("Пароль и его повторения пароля должны совпадать!");
                return;
            }

            // Проверка на заполнение обязательных полей
            if (string.IsNullOrEmpty(name.Text) || string.IsNullOrEmpty(surname.Text))
            {
                MessageBox.Show("Пожалуйста, заполните имя и фамилию.");
                return;
            }

            // Проверка на заполнение номера паспорта и кода подразделения
            if (string.IsNullOrEmpty(pass_number.Text))
            {
                MessageBox.Show("Серия и номер паспорта обязательны!");
                return;
            }
            if (string.IsNullOrEmpty(pass_authority_code.Text))
            {
                MessageBox.Show("Код подразделения обязателен!");
                return;
            }

            // Проверка на заполнение места выдачи паспорта
            if (string.IsNullOrEmpty(pass_authority_name.Text) || string.IsNullOrEmpty(pass_birth_address.Text))
            {
                MessageBox.Show("Кем выдан паспорт и Где прописаны обязательны!");
                return;
            }

            // Проверка на заполнение даты рождения и даты выдачи паспорта
            if (!birthday.SelectedDate.HasValue || !pass_issue_date.SelectedDate.HasValue)
            {
                MessageBox.Show("Дата рождения и Дата выдачи обязательны!");
                return;
            }

            string gender = sexComboBox.SelectedItem.ToString();
            bool sexValue = gender == "male" ? true : false;
            DateTime Birthday = birthday.SelectedDate ?? new DateTime();
            DateTime PassIssueDate = pass_issue_date.SelectedDate ?? new DateTime();

            var newUser = new
            {
                phone = Convert.ToInt64(Regex.Replace(phone.Text, @"[^\d]", "")),
                password = password.Text,
                passwordRepeat = passwordRepeat.Text,
                role = roleComboBox.SelectedItem.ToString(),
                birthday = Birthday.ToString("yyyy-MM-dd"),
                surname = surname.Text,
                name = name.Text,
                patronymic = patronymic.Text ?? "",
                pass_number = Convert.ToInt64(Regex.Replace(pass_number.Text, @"[^\d]", "")),
                pass_authority_code = Convert.ToInt64(Regex.Replace(pass_authority_code.Text, @"[^\d]", "")),
                pass_authority_name = pass_authority_name.Text,
                pass_birth_address = pass_birth_address.Text,
                pass_issue_date = PassIssueDate.ToString("yyyy-MM-dd"),
                sex = sexValue,
            };

            string json = JsonConvert.SerializeObject(newUser);
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(APIconfig.APIurl + "/users", content);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Пользователь успешно создан!");
                        FrameManager.MainFrame.Navigate(new ListUsers(mainWindow));
                    }
                    else
                    {
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Ошибка при создании пользователя: {response.StatusCode} - {errorMessage}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выполнении запроса: {ex.Message}");
            }
            
        }
        private void BackButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            FrameManager.MainFrame.Navigate(new ListUsers(mainWindow));
        }
    }
}
   
