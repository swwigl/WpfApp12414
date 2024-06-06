using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Data;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private RealEstateDBEntities dbContext = new RealEstateDBEntities();
        public MainWindow()
        {
            InitializeComponent();
            LoadClients();
            LoadAgents();
            LoadApartments();
            LoadSupplies();
            LoadComboBoxes();
            LoadDemands();
        }


        private void LoadClients()
        {
            ClientsDataGrid.ItemsSource = dbContext.Clients.ToList();
        }

        private void LoadAgents()
        {
            AgentsDataGrid.ItemsSource = dbContext.Agents.ToList();
        }

        private void LoadApartments()
        {
            ApartmentsDataGrid.ItemsSource = dbContext.Apartments.ToList();
        }

        private void LoadDemands()
        {
            var demands = dbContext.Demands
                .Select(d => new
                {
                    d.ID,
                    d.Adress_City,
                    d.Adress_Street,
                    d.Adress_House,
                    d.Adress_Number,
                    d.Min_Price,
                    d.Max_Price,
                    d.FK_AgentID,
                    d.FK_ClientID,
                    d.MinArea,
                    d.MaxArea,
                    d.MinRooms,
                    d.MaxRooms,
                    d.MinFloor,
                    d.MaxFloor,
                    d.FK_Type_Object_ID
                }).ToList();

            DemandsDataGrid.ItemsSource = demands;
        }


        private void LoadSupplies()
        {
            var supplies = dbContext.Supplies
                .Include(s => s.Agents)
                .Include(s => s.Clients)
                .Include(s => s.Apartments)
                .Select(s => new
                {
                    s.ID,
                    s.Price,
                    AgentName = s.Agents.FirstName + " " + s.Agents.MiddleName,
                    ClientName = s.Clients.FirstName + " " + s.Clients.MiddleName,
                    ApartmentAddress = s.Apartments.Adress_Street + ", " + s.Apartments.Adress_Number
                }).ToList();

            SuppliesDataGrid.ItemsSource = supplies;
        }



        private void LoadComboBoxes()
        {
            // Очищаем предыдущие данные из комбо-боксов
            SupplyAgentComboBox.Items.Clear();
            SupplyClientComboBox.Items.Clear();
            SupplyApartmentComboBox.Items.Clear();

            // Загружаем данные для комбо-бокса агентов
            var agents = dbContext.Agents.Select(a => new { a.FirstName, a.ID }).ToList();
            SupplyAgentComboBox.ItemsSource = agents;

            // Загружаем данные для комбо-бокса клиентов
            var clients = dbContext.Clients.Select(c => new { c.FirstName, c.ID }).ToList();
            SupplyClientComboBox.ItemsSource = clients;

            // Загружаем данные для комбо-бокса квартир
            var apartments = dbContext.Apartments.Select(ap => new { ap.Adress_Street, ap.ID }).ToList();
            SupplyApartmentComboBox.ItemsSource = apartments;
        }


        private void AddUserButton_Click1(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(IDTextBox.Text) ||
                string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(LastNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(EmailTextBox.Text) ||
                string.IsNullOrWhiteSpace(PhoneTextBox.Text) ||
                string.IsNullOrWhiteSpace(MiddleNameTextBox.Text))
            {
                ShowPopup("Все поля должны быть заполнены.", false);
                return;
            }

            if (!int.TryParse(IDTextBox.Text, out int clientId))
            {
                ShowPopup("ID должен быть числом.", false);
                return;
            }

            if (clientId < 0)
            {
                ShowPopup("ID не может быть отрицательным.", false);
                return;
            }

            if (dbContext.Clients.Any(c => c.ID == clientId))
            {
                ShowPopup("Клиент с таким ID уже существует.", false);
                return;
            }

            if (!IsValidName(FirstNameTextBox.Text, 20) ||
                !IsValidName(LastNameTextBox.Text, 20))
            {
                ShowPopup("Имя и Фамилия должны содержать только буквы и быть длиной до 20 символов.", false);
                return;
            }

            if (!IsValidName(MiddleNameTextBox.Text, 35))
            {
                ShowPopup("Отчество должно содержать только буквы и быть длиной до 35 символов.", false);
                return;
            }

            if (!IsValidEmail(EmailTextBox.Text))
            {
                ShowPopup("Некорректный адрес электронной почты.", false);
                return;
            }

            if (!IsValidPhone(PhoneTextBox.Text))
            {
                ShowPopup("Номер телефона должен начинаться с +7 и содержать 11 цифр.", false);
                return;
            }

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
                dbContext.Clients.Add(newClient);
                dbContext.SaveChanges();
                LoadClients();
                ClearInputs();
                ShowPopup("Пользователь успешно добавлен!", true);
            }
            catch (Exception ex)
            {
                ShowPopup($"Ошибка при добавлении пользователя: {ex.Message}", false);
            }
        }

        private void UpdateUserButton_Click1(object sender, RoutedEventArgs e)
        {
            if (ClientsDataGrid.SelectedItem == null) return;

            var selectedClient = (Clients)ClientsDataGrid.SelectedItem;
            var client = dbContext.Clients.Find(selectedClient.ID);

            if (client != null)
            {
                client.FirstName = FirstNameTextBox.Text;
                client.MiddleName = MiddleNameTextBox.Text;
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
            if (ClientsDataGrid.SelectedItem == null) return;

            var selectedClient = (Clients)ClientsDataGrid.SelectedItem;
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
            if (ClientsDataGrid.SelectedItem == null) return;

            var selectedClient = (Clients)ClientsDataGrid.SelectedItem;
            IDTextBox.Text = selectedClient.ID.ToString();
            FirstNameTextBox.Text = selectedClient.FirstName;
            MiddleNameTextBox.Text = selectedClient.MiddleName;
            LastNameTextBox.Text = selectedClient.LastName;
            EmailTextBox.Text = selectedClient.Email;
            PhoneTextBox.Text = selectedClient.Phone;
        }

        private void AddAgentButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(IDTextBox.Text) ||
                string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
                string.IsNullOrEmpty(MiddleNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(LastNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(DealShareTextBox.Text))
            {
                MessageBox.Show("Все поля должны быть заполнены.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(IDTextBox.Text, out int agentId) || agentId < 0)
            {
                MessageBox.Show("ID должен быть положительным числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(DealShareTextBox.Text, out double dealShare))
            {
                MessageBox.Show("Доля в сделке должна быть числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dbContext.Agents.Any(a => a.ID == agentId))
            {
                MessageBox.Show("Агент с таким ID уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newAgent = new Agents
            {
                ID = agentId,
                FirstName = FirstNameTextBox.Text,
                MiddleName = MiddleNameTextBox.Text,
                LastName = LastNameTextBox.Text,
                DealShare = dealShare.ToString()
            };

            try
            {
                dbContext.Agents.Add(newAgent);
                dbContext.SaveChanges();
                LoadAgents();
                ClearAgentInputs();
                MessageBox.Show("Агент успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении агента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateAgentButton_Click(object sender, RoutedEventArgs e)
        {
            if (AgentsDataGrid.SelectedItem == null) return;

            var selectedAgent = (Agents)AgentsDataGrid.SelectedItem;
            var agent = dbContext.Agents.Find(selectedAgent.ID);

            if (agent != null)
            {
                agent.FirstName = FirstNameTextBox.Text;
                agent.MiddleName = MiddleNameTextBox.Text;
                agent.LastName = LastNameTextBox.Text;
                agent.DealShare = DealShareTextBox.Text;

                dbContext.Entry(agent).State = EntityState.Modified;
                dbContext.SaveChanges();
                LoadAgents();
                ClearAgentInputs();
                MessageBox.Show("Агент успешно обновлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteAgentButton_Click(object sender, RoutedEventArgs e)
        {
            if (AgentsDataGrid.SelectedItem == null) return;

            var selectedAgent = (Agents)AgentsDataGrid.SelectedItem;
            var agent = dbContext.Agents.Find(selectedAgent.ID);

            if (agent != null)
            {
                dbContext.Agents.Remove(agent);
                dbContext.SaveChanges();
                LoadAgents();
                ClearAgentInputs();
                MessageBox.Show("Агент успешно удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AgentsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AgentsDataGrid.SelectedItem == null) return;

            var selectedAgent = (Agents)AgentsDataGrid.SelectedItem;
            IDTextBox.Text = selectedAgent.ID.ToString();
            FirstNameTextBox.Text = selectedAgent.FirstName;
            MiddleNameTextBox.Text = selectedAgent.MiddleName;
            LastNameTextBox.Text = selectedAgent.LastName;
            DealShareTextBox.Text = selectedAgent.DealShare;
        }

        private void AddApartmentsButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ApartmentsIDTextBox.Text) ||
                string.IsNullOrWhiteSpace(ApartmentsCityTextBox.Text) ||
                string.IsNullOrWhiteSpace(ApartmentsStreetTextBox.Text) ||
                string.IsNullOrWhiteSpace(ApartmentsHouseTextBox.Text) ||
                string.IsNullOrWhiteSpace(ApartmentsNumberTextBox.Text) ||
                string.IsNullOrWhiteSpace(ApartmentsDistrictTextBox.Text) ||
                string.IsNullOrWhiteSpace(ApartmentsTotalAreaTextBox.Text) ||
                string.IsNullOrWhiteSpace(ApartmentsRoomsTextBox.Text) ||
                string.IsNullOrWhiteSpace(ApartmentsFloorTextBox.Text) ||
                string.IsNullOrWhiteSpace(ApartmentsTypeTextBox.Text))
            {
                MessageBox.Show("Все поля должны быть заполнены.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(ApartmentsIDTextBox.Text, out int apartmentsId) || apartmentsId < 0)
            {
                MessageBox.Show("ID должен быть положительным числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(ApartmentsNumberTextBox.Text, out int number))
            {
                MessageBox.Show("Номер должен быть числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(ApartmentsDistrictTextBox.Text, out int districtId))
            {
                MessageBox.Show("ID района должен быть числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(ApartmentsTotalAreaTextBox.Text, out double totalArea))
            {
                MessageBox.Show("Общая площадь должна быть числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(ApartmentsRoomsTextBox.Text, out int rooms))
            {
                MessageBox.Show("Количество комнат должно быть числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(ApartmentsFloorTextBox.Text, out int floor))
            {
                MessageBox.Show("Этаж должен быть числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(ApartmentsTypeTextBox.Text, out int typeId))
            {
                MessageBox.Show("ID типа объекта должен быть числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dbContext.Apartments.Any(a => a.ID == apartmentsId))
            {
                MessageBox.Show("Объект недвижимости с таким ID уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newApartment = new Apartments
            {
                ID = apartmentsId,
                Adress_City = ApartmentsCityTextBox.Text,
                Adress_Street = ApartmentsStreetTextBox.Text,
                Adress_House = ApartmentsHouseTextBox.Text,
                Adress_Number = number,
                FK_ID_Districts = districtId,
                TotalArea = totalArea,
                Rooms = rooms,
                Floor = floor,
                FK_Type_Object_ID = typeId
            };

            try
            {
                dbContext.Apartments.Add(newApartment);
                dbContext.SaveChanges();
                LoadApartments();
                ClearApartmentsInputs();
                MessageBox.Show("Объект недвижимости успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении объекта недвижимости: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateApartmentsButton_Click(object sender, RoutedEventArgs e)
        {
            if (ApartmentsDataGrid.SelectedItem == null) return;

            var selectedProperty = (Apartments)ApartmentsDataGrid.SelectedItem;
            var property = dbContext.Apartments.Find(selectedProperty.ID);

            if (property != null)
            {
                property.Adress_City = ApartmentsCityTextBox.Text;
                property.Adress_Street = ApartmentsStreetTextBox.Text;
                property.Adress_House = ApartmentsHouseTextBox.Text;

                if (int.TryParse(ApartmentsNumberTextBox.Text, out int number))
                {
                    property.Adress_Number = number;
                }
                else
                {
                    MessageBox.Show("Некорректное значение для номера.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (int.TryParse(ApartmentsDistrictTextBox.Text, out int districtId))
                {
                    property.FK_ID_Districts = districtId;
                }
                else
                {
                    MessageBox.Show("Некорректное значение для ID района.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (double.TryParse(ApartmentsTotalAreaTextBox.Text, out double totalArea))
                {
                    property.TotalArea = totalArea;
                }
                else
                {
                    MessageBox.Show("Некорректное значение для общей площади.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (int.TryParse(ApartmentsRoomsTextBox.Text, out int rooms))
                {
                    property.Rooms = rooms;
                }
                else
                {
                    MessageBox.Show("Некорректное значение для количества комнат.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (int.TryParse(ApartmentsFloorTextBox.Text, out int floor))
                {
                    property.Floor = floor;
                }
                else
                {
                    MessageBox.Show("Некорректное значение для этажа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (int.TryParse(ApartmentsTypeTextBox.Text, out int typeId))
                {
                    property.FK_Type_Object_ID = typeId;
                }
                else
                {
                    MessageBox.Show("Некорректное значение для ID типа объекта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                dbContext.Entry(property).State = EntityState.Modified;
                dbContext.SaveChanges();
                LoadApartments();
                ClearApartmentsInputs();
                MessageBox.Show("Объект недвижимости успешно обновлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteApartmentsButton_Click(object sender, RoutedEventArgs e)
        {
            if (ApartmentsDataGrid.SelectedItem == null) return;

            var selectedProperty = (Apartments)ApartmentsDataGrid.SelectedItem;
            var property = dbContext.Apartments.Find(selectedProperty.ID);

            if (property != null)
            {
                dbContext.Apartments.Remove(property);
                dbContext.SaveChanges();
                LoadApartments();
                ClearApartmentsInputs();
                MessageBox.Show("Объект недвижимости успешно удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ApartmentsDataGrid_SelectionChanged1(object sender, SelectionChangedEventArgs e)
        {
            if (ApartmentsDataGrid.SelectedItem == null) return;

            var selectedProperty = (Apartments)ApartmentsDataGrid.SelectedItem;
            ApartmentsIDTextBox.Text = selectedProperty.ID.ToString();
            ApartmentsCityTextBox.Text = selectedProperty.Adress_City;
            ApartmentsStreetTextBox.Text = selectedProperty.Adress_Street;
            ApartmentsHouseTextBox.Text = selectedProperty.Adress_House;
            ApartmentsNumberTextBox.Text = selectedProperty.Adress_Number.ToString();
            ApartmentsDistrictTextBox.Text = selectedProperty.FK_ID_Districts.ToString();
            ApartmentsTotalAreaTextBox.Text = selectedProperty.TotalArea.ToString();
            ApartmentsRoomsTextBox.Text = selectedProperty.Rooms.ToString();
            ApartmentsFloorTextBox.Text = selectedProperty.Floor.ToString();
            ApartmentsTypeTextBox.Text = selectedProperty.FK_Type_Object_ID.ToString();
        }

        private void AddSupplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SupplyIDTextBox.Text) ||
                string.IsNullOrWhiteSpace(SupplyPriceTextBox.Text) ||
                SupplyAgentComboBox.SelectedItem == null ||
                SupplyClientComboBox.SelectedItem == null ||
                SupplyApartmentComboBox.SelectedItem == null)
            {
                MessageBox.Show("Все поля должны быть заполнены.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(SupplyIDTextBox.Text, out int supplyId) || supplyId < 0)
            {
                MessageBox.Show("ID должен быть положительным числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(SupplyPriceTextBox.Text, out double price))
            {
                MessageBox.Show("Цена должна быть числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dbContext.Supplies.Any(s => s.ID == supplyId))
            {
                MessageBox.Show("Предложение с таким ID уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newSupply = new Supplies
            {
                ID = supplyId,
                Price = Convert.ToString(price),
                FK_AgentID = (int)SupplyAgentComboBox.SelectedValue,
                FK_ClientID = (int)SupplyClientComboBox.SelectedValue,
                FK_ApartmentsID = (int)SupplyApartmentComboBox.SelectedValue
            };

            try
            {
                dbContext.Supplies.Add(newSupply);
                dbContext.SaveChanges();
                LoadSupplies();
                ClearSupplyInputs();
                MessageBox.Show("Предложение успешно добавлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении предложения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        private void UpdateSupplyButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, выбрано ли какое-либо предложение в DataGrid
            if (SuppliesDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите предложение для обновления.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Получаем выбранное предложение из DataGrid
                dynamic selectedSupply = SuppliesDataGrid.SelectedItem;

                // Получаем ID выбранного предложения
                int supplyId = selectedSupply.ID;

                // Находим предложение в базе данных по ID
                var supply = dbContext.Supplies.Find(supplyId);

                if (supply != null)
                {
                    // Обновляем данные предложения из полей ввода
                    supply.ID = int.Parse(SupplyIDTextBox.Text);
                    supply.Price = Convert.ToString(SupplyPriceTextBox.Text);
                    supply.FK_AgentID = (int)SupplyAgentComboBox.SelectedValue;
                    supply.FK_ClientID = (int)SupplyClientComboBox.SelectedValue;
                    supply.FK_ApartmentsID = (int)SupplyApartmentComboBox.SelectedValue;

                    // Сохраняем изменения в базе данных
                    dbContext.SaveChanges();

                    // Обновляем список предложений в DataGrid
                    LoadSupplies();

                    // Очищаем поля ввода после обновления
                    ClearSupplyInputs();

                    MessageBox.Show("Предложение успешно обновлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Выбранное предложение не найдено в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении предложения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }





        private void DeleteSupplyButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, выбрано ли какое-либо предложение в DataGrid
            if (SuppliesDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите предложение для удаления.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Получаем выбранное предложение из DataGrid
                dynamic selectedSupply = SuppliesDataGrid.SelectedItem;

                // Получаем ID выбранного предложения
                int supplyId = selectedSupply.ID;

                // Находим предложение в базе данных по ID
                var supply = dbContext.Supplies.Find(supplyId);

                if (supply != null)
                {
                    // Удаляем предложение из базы данных
                    dbContext.Supplies.Remove(supply);

                    // Сохраняем изменения в базе данных
                    dbContext.SaveChanges();

                    // Обновляем список предложений в DataGrid
                    LoadSupplies();

                    // Очищаем поля ввода после удаления
                    ClearSupplyInputs();

                    MessageBox.Show("Предложение успешно удалено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Выбранное предложение не найдено в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении предложения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }





        private void SuppliesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SuppliesDataGrid.SelectedItem == null) return;

            dynamic selectedSupply = SuppliesDataGrid.SelectedItem;
            SupplyIDTextBox.Text = selectedSupply.ID.ToString();
            SupplyPriceTextBox.Text = selectedSupply.Price.ToString();

            // Находим объекты в ComboBox по имени
            SupplyAgentComboBox.SelectedItem = selectedSupply.AgentName;
            SupplyClientComboBox.SelectedItem = selectedSupply.ClientName;
            SupplyApartmentComboBox.SelectedItem = selectedSupply.ApartmentAddress;
        }








        private void ClearSupplyInputs()
        {
            SupplyIDTextBox.Text = "";
            SupplyPriceTextBox.Text = "";
            SupplyAgentComboBox.SelectedIndex = -1;
            SupplyClientComboBox.SelectedIndex = -1;
            SupplyApartmentComboBox.SelectedIndex = -1;
        }


        private void ClearInputs()
        {
            IDTextBox.Text = string.Empty;
            FirstNameTextBox.Text = string.Empty;
            LastNameTextBox.Text = string.Empty;
            EmailTextBox.Text = string.Empty;
            PhoneTextBox.Text = string.Empty;
            MiddleNameTextBox.Text = string.Empty;
        }

        private void ClearAgentInputs()
        {
            IDTextBox.Text = string.Empty;
            FirstNameTextBox.Text = string.Empty;
            MiddleNameTextBox .Text = string.Empty;
            LastNameTextBox.Text = string.Empty;
            DealShareTextBox.Text = string.Empty;
        }

        private void ClearApartmentsInputs()
        {
            ApartmentsIDTextBox.Text = string.Empty;
            ApartmentsCityTextBox.Text = string.Empty;
            ApartmentsStreetTextBox.Text = string.Empty;
            ApartmentsHouseTextBox.Text = string.Empty;
            ApartmentsNumberTextBox.Text = string.Empty;
            ApartmentsDistrictTextBox.Text = string.Empty;
            ApartmentsTotalAreaTextBox.Text = string.Empty;
            ApartmentsRoomsTextBox.Text = string.Empty;
            ApartmentsFloorTextBox.Text = string.Empty;
            ApartmentsTypeTextBox.Text = string.Empty;
        }

        private void ShowPopup(string message, bool isSuccess)
        {
            MessageBox.Show(message, isSuccess ? "Успех" : "Ошибка", MessageBoxButton.OK, isSuccess ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }

        private bool IsValidName(string name, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length > maxLength) return false;
            return name.All(c => char.IsLetter(c) || c == '-');
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
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
        private void ClosePopup_Click(object sender, RoutedEventArgs e)
        {
            SuccessPopup.IsOpen = false;
        }

        private void AddDemandsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Demands demand = new Demands()
                {
                    ID = int.Parse(DemandsIDTextBox.Text),
                    Adress_City = DemandsCityTextBox.Text,
                    Adress_Street = DemandsStreetTextBox.Text,
                    Adress_House = DemandsHouseTextBox.Text,
                    Adress_Number = Convert.ToInt32(DemandsNumberTextBox.Text),
                    Min_Price = int.Parse(DemandsMinPriceTextBox.Text),
                    Max_Price = int.Parse(DemandsMaxPriceTextBox.Text),
                    FK_AgentID = int.Parse(DemandsAgentIDTextBox.Text),
                    FK_ClientID = int.Parse(DemandsClientIDTextBox.Text),
                    MinArea = int.Parse(DemandsMinAreaTextBox.Text),
                    MaxArea = int.Parse(DemandsMaxAreaTextBox.Text),
                    MinRooms = int.Parse(DemandsMinRoomsTextBox.Text),
                    MaxRooms = int.Parse(DemandsMaxRoomsTextBox.Text),
                    MinFloor = int.Parse(DemandsMinFloorTextBox.Text),
                    MaxFloor = int.Parse(DemandsMaxFloorTextBox.Text),
                    FK_Type_Object_ID = int.Parse(DemandsTypeIDTextBox.Text)
                };

                // Добавление новой потребности в базу данных и обновление DataGrid
                dbContext.Demands.Add(demand);
                dbContext.SaveChanges();
                LoadDemands();
                SuccessPopupText.Text = "Потребность успешно добавлена!";
                SuccessPopup.IsOpen = true;
            }
            catch (Exception ex)
            {
                UnSuccessPopupText.Text = $"Ошибка при добавлении потребности: {ex.Message}";
                UnSuccessPopup.IsOpen = true;
            }
        }

        private void UpdateDemandsButton_Click(object sender, RoutedEventArgs e)
        {
            if (DemandsDataGrid.SelectedItem == null) return;

            try
            {
                Demands selectedDemand = (Demands)DemandsDataGrid.SelectedItem;
                Demands demand = dbContext.Demands.Find(selectedDemand.ID);

                demand.Adress_City = DemandsCityTextBox.Text;
                demand.Adress_Street = DemandsStreetTextBox.Text;
                demand.Adress_House = DemandsHouseTextBox.Text;
                demand.Adress_Number = Convert.ToInt32(DemandsNumberTextBox.Text);
                demand.Min_Price = int.Parse(DemandsMinPriceTextBox.Text);
                demand.Max_Price = int.Parse(DemandsMaxPriceTextBox.Text);
                demand.FK_AgentID = int.Parse(DemandsAgentIDTextBox.Text);
                demand.FK_ClientID = int.Parse(DemandsClientIDTextBox.Text);
                demand.MinArea = int.Parse(DemandsMinAreaTextBox.Text);
                demand.MaxArea = int.Parse(DemandsMaxAreaTextBox.Text);
                demand.MinRooms = int.Parse(DemandsMinRoomsTextBox.Text);
                demand.MaxRooms = int.Parse(DemandsMaxRoomsTextBox.Text);
                demand.MinFloor = int.Parse(DemandsMinFloorTextBox.Text);
                demand.MaxFloor = int.Parse(DemandsMaxFloorTextBox.Text);
                demand.FK_Type_Object_ID = int.Parse(DemandsTypeIDTextBox.Text);

                // Сохранение изменений в базе данных и обновление DataGrid
                dbContext.SaveChanges();
                LoadDemands();
                SuccessPopupText.Text = "Потребность успешно обновлена!";
                SuccessPopup.IsOpen = true;
            }
            catch (Exception ex)
            {
                UnSuccessPopupText.Text = $"Ошибка при обновлении потребности: {ex.Message}";
                UnSuccessPopup.IsOpen = true;
            }
        }

        private void DeleteDemandsButton_Click(object sender, RoutedEventArgs e)
        {
            if (DemandsDataGrid.SelectedItem == null) return;

            try
            {
                Demands selectedDemand = (Demands)DemandsDataGrid.SelectedItem;
                Demands demand = dbContext.Demands.Find(selectedDemand.ID);

                // Удаление потребности из базы данных и обновление DataGrid
                dbContext.Demands.Remove(demand);
                dbContext.SaveChanges();
                LoadDemands();
                SuccessPopupText.Text = "Потребность успешно удалена!";
                SuccessPopup.IsOpen = true;
            }
            catch (Exception ex)
            {
                UnSuccessPopupText.Text = $"Ошибка при удалении потребности: {ex.Message}";
                UnSuccessPopup.IsOpen = true;
            }
        }

    }
}
