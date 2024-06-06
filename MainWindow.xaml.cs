using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private RealEstateDBEntities dbContext = new RealEstateDBEntities();

        public MainWindow()
        {
            InitializeComponent();
            LoadClients();
        }

        private void LoadClients()
        {
            UsersDataGrid.ItemsSource = dbContext.Clients.ToList();
        }

        private void AddUserButton_Click1(object sender, RoutedEventArgs e)
        {
            // Проверка, что все поля заполнены
            if (string.IsNullOrWhiteSpace(IDTextBox.Text) ||
                string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(LastNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(EmailTextBox.Text) ||
                string.IsNullOrWhiteSpace(PhoneTextBox.Text) ||
                string.IsNullOrWhiteSpace(MiddleNameTextBox.Text))
            {
                MessageBox.Show("Все поля должны быть заполнены.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(IDTextBox.Text, out int clientId))
            {
                MessageBox.Show("ID должен быть числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (clientId < 0)
            {
                MessageBox.Show("ID не может быть отрицательным.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            // Проверка уникальности ID
            if (dbContext.Clients.Any(c => c.ID == clientId))
            {
                MessageBox.Show("Клиент с таким ID уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка имени и фамилии
            if (!IsValidName(FirstNameTextBox.Text, 20) ||
                !IsValidName(LastNameTextBox.Text, 20))
            {
                MessageBox.Show("Имя и Фамилия должны содержать только буквы и быть длиной до 20 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка отчества
            if (!IsValidName(MiddleNameTextBox.Text, 35))
            {
                MessageBox.Show("Отчество должно содержать только буквы и быть длиной до 35 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка почты
            if (!IsValidEmail(EmailTextBox.Text))
            {
                MessageBox.Show("Некорректный адрес электронной почты.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка номера телефона
            if (!IsValidPhone(PhoneTextBox.Text))
            {
                MessageBox.Show("Номер телефона должен начинаться с +7 и содержать 11 цифр.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Создание нового клиента
            var newClient = new Clients
            {
                ID = clientId,
                FirstName = FirstNameTextBox.Text,
                LastName = LastNameTextBox.Text,
                Email = EmailTextBox.Text,
                Phone = PhoneTextBox.Text,
                MiddleName = MiddleNameTextBox.Text
            };

            try
            {
                // Добавление нового клиента в контекст и сохранение изменений
                dbContext.Clients.Add(newClient);
                dbContext.SaveChanges();

                // Обновление списка клиентов в DataGrid и очистка полей ввода
                LoadClients();
                ClearInputs();

                MessageBox.Show("Пользователь успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private bool IsValidName(string name, int maxLength)
        {
            return name.All(char.IsLetter) && name.Length <= maxLength;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email &&
                       (email.EndsWith("mail.ru") || email.EndsWith("gmail.com") || email.EndsWith("yandex.ru"));
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhone(string phone)
        {
            return phone.StartsWith("+7") && phone.Length == 12 && phone.Skip(1).All(char.IsDigit);
        }



        private void UpdateUserButton_Click1(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem == null) return;

            var selectedClient = (Clients)UsersDataGrid.SelectedItem;
            var client = dbContext.Clients.Find(selectedClient.ID);

            if (client != null)
            {
                client.FirstName = FirstNameTextBox.Text;
                client.LastName = LastNameTextBox.Text;
                client.Email = EmailTextBox.Text;
                client.Phone = PhoneTextBox.Text;
                client.MiddleName = MiddleNameTextBox.Text;

                dbContext.Entry(client).State = EntityState.Modified;
                dbContext.SaveChanges();
                LoadClients();
                ClearInputs();
                MessageBox.Show("Пользователь успешно обновлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteUserButton_Click1(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem == null) return;

            var selectedClient = (Clients)UsersDataGrid.SelectedItem;
            var client = dbContext.Clients.Find(selectedClient.ID);

            if (client != null)
            {
                dbContext.Clients.Remove(client);
                dbContext.SaveChanges();
                LoadClients();
                ClearInputs();
                MessageBox.Show("Пользователь успешно удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }




        private void UsersDataGrid_SelectionChanged1(object sender, SelectionChangedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem == null) return;

            var selectedClient = (Clients)UsersDataGrid.SelectedItem;
            IDTextBox.Text = selectedClient.ID.ToString();
            FirstNameTextBox.Text = selectedClient.FirstName;
            MiddleNameTextBox.Text = selectedClient.MiddleName;
            LastNameTextBox.Text = selectedClient.LastName;
            EmailTextBox.Text = selectedClient.Email;
            PhoneTextBox.Text = selectedClient.Phone;
        }

        private void ClearInputs()
        {
            IDTextBox.Clear();
            FirstNameTextBox.Clear();
            MiddleNameTextBox.Clear();
            LastNameTextBox.Clear();
            EmailTextBox.Clear();
            PhoneTextBox.Clear();
        }

        
    }
}
