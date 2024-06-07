using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Data;
using System.Data.Entity.Infrastructure;
using WpfApp1;
using System.Timers;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private RealEstateDBEntities dbContext = new RealEstateDBEntities();
        private Timer timer;
        private bool isShowingSuccessPopup;
        private bool isShowingUnSuccessPopup;
        public MainWindow()
        {
            InitializeComponent();
            LoadClients();
            LoadAgents();
            LoadApartments();
            LoadSupplies();
            LoadComboBoxes();
            LoadDemands();
            LoadDeals();
            timer = new Timer();
            timer.AutoReset = false;
            timer.Elapsed += TimerElapsed;
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
            var apartments = dbContext.Apartments.ToList();
            ApartmentsDataGrid.ItemsSource = apartments;
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

        private void LoadDeals()
        {
            try
            {
                // Загрузка сделок из базы данных
                var deals = dbContext.Deals
                    .Select(d => new
                    {
                        ID = d.ID,
                        Demant_ID = d.Demant_ID,
                        Supply_ID = d.Supply_ID
                    }).ToList();

                // Привязка загруженных сделок к DataGrid
                DealsDataGrid.ItemsSource = deals;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке сделок: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            if (ClientsDataGrid.SelectedItem == null)
            {
                ShowPopup("Выберите пользователя для обновления.", false);
                return;
            }

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

                try
                {
                    dbContext.Entry(client).State = EntityState.Modified;
                    dbContext.SaveChanges();
                    LoadClients();
                    ClearInputs();
                    ShowPopup("Пользователь успешно обновлен!", true);
                }
                catch (Exception ex)
                {
                    ShowPopup($"Ошибка при обновлении пользователя: {ex.Message}", false);
                }
            }
        }


        private void DeleteUserButton_Click1(object sender, RoutedEventArgs e)
        {
            if (ClientsDataGrid.SelectedItem == null) return;

            var selectedClient = (Clients)ClientsDataGrid.SelectedItem;
            var client = dbContext.Clients.Find(selectedClient.ID);

            if (client != null)
            {
                try
                {
                    dbContext.Clients.Remove(client);
                    dbContext.SaveChanges();
                    LoadClients();
                    ClearInputs();
                    ShowPopup("Пользователь успешно удален!", true);
                }
                catch (Exception ex)
                {
                    ShowPopup($"Ошибка при удалении пользователя: {ex.Message}", false);
                }
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
                ShowPopup("Все поля должны быть заполнены.", false);
                return;
            }

            if (!int.TryParse(IDTextBox.Text, out int agentId) || agentId < 0)
            {
                ShowPopup("ID должен быть положительным числом.", false);
                return;
            }

            if (!double.TryParse(DealShareTextBox.Text, out double dealShare))
            {
                ShowPopup("Доля в сделке должна быть числом.", false);
                return;
            }

            if (dbContext.Agents.Any(a => a.ID == agentId))
            {
                ShowPopup("Агент с таким ID уже существует.", false);
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
                ShowPopup("Агент успешно добавлен!", true);
            }
            catch (Exception ex)
            {
                ShowPopup($"Ошибка при добавлении агента: {ex.Message}", false);
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

                try
                {
                    dbContext.Entry(agent).State = EntityState.Modified;
                    dbContext.SaveChanges();
                    LoadAgents();
                    ClearAgentInputs();
                    ShowPopup("Агент успешно обновлен!", true);
                }
                catch (Exception ex)
                {
                    ShowPopup($"Ошибка при обновлении агента: {ex.Message}", false);
                }
            }
        }

        private void DeleteAgentButton_Click(object sender, RoutedEventArgs e)
        {
            if (AgentsDataGrid.SelectedItem == null) return;

            var selectedAgent = (Agents)AgentsDataGrid.SelectedItem;
            var agent = dbContext.Agents.Find(selectedAgent.ID);

            if (agent != null)
            {
                try
                {
                    dbContext.Agents.Remove(agent);
                    dbContext.SaveChanges();
                    LoadAgents();
                    ClearAgentInputs();
                    ShowPopup("Агент успешно удален!", true);
                }
                catch (Exception ex)
                {
                    ShowPopup($"Ошибка при удалении агента: {ex.Message}", false);
                }
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
                ShowPopup("Все поля должны быть заполнены.", false);
                return;
            }

            if (!int.TryParse(ApartmentsIDTextBox.Text, out int apartmentsId) || apartmentsId < 0)
            {
                ShowPopup("ID должен быть положительным числом.", false);
                return;
            }

            if (!int.TryParse(ApartmentsNumberTextBox.Text, out int number))
            {
                ShowPopup("Номер должен быть числом.", false);
                return;
            }

            if (!int.TryParse(ApartmentsDistrictTextBox.Text, out int districtId))
            {
                ShowPopup("ID района должен быть числом.", false);
                return;
            }

            if (!double.TryParse(ApartmentsTotalAreaTextBox.Text, out double totalArea))
            {
                ShowPopup("Общая площадь должна быть числом.", false);
                return;
            }

            if (!int.TryParse(ApartmentsRoomsTextBox.Text, out int rooms))
            {
                ShowPopup("Количество комнат должно быть числом.", false);
                return;
            }

            if (!int.TryParse(ApartmentsFloorTextBox.Text, out int floor))
            {
                ShowPopup("Этаж должен быть числом.", false);
                return;
            }

            if (!int.TryParse(ApartmentsTypeTextBox.Text, out int typeId))
            {
                ShowPopup("ID типа объекта должен быть числом.", false);
                return;
            }

            if (dbContext.Apartments.Any(a => a.ID == apartmentsId))
            {
                ShowPopup("Объект недвижимости с таким ID уже существует.", false);
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
                ShowPopup("Объект недвижимости успешно добавлен!", true);
            }
            catch (Exception ex)
            {
                ShowPopup($"Ошибка при добавлении объекта недвижимости: {ex.Message}", false);
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
                    ShowPopup("Некорректное значение для номера.", false);
                    return;
                }

                if (int.TryParse(ApartmentsDistrictTextBox.Text, out int districtId))
                {
                    property.FK_ID_Districts = districtId;
                }
                else
                {
                    ShowPopup("Некорректное значение для ID района.", false);
                    return;
                }

                if (double.TryParse(ApartmentsTotalAreaTextBox.Text, out double totalArea))
                {
                    property.TotalArea = totalArea;
                }
                else
                {
                    ShowPopup("Некорректное значение для общей площади.", false);
                    return;
                }

                if (int.TryParse(ApartmentsRoomsTextBox.Text, out int rooms))
                {
                    property.Rooms = rooms;
                }
                else
                {
                    ShowPopup("Некорректное значение для количества комнат.", false);
                    return;
                }

                if (int.TryParse(ApartmentsFloorTextBox.Text, out int floor))
                {
                    property.Floor = floor;
                }
                else
                {
                    ShowPopup("Некорректное значение для этажа.", false);
                    return;
                }

                if (int.TryParse(ApartmentsTypeTextBox.Text, out int typeId))
                {
                    property.FK_Type_Object_ID = typeId;
                }
                else
                {
                    ShowPopup("Некорректное значение для ID типа объекта.", false);
                    return;
                }

                try
                {
                    dbContext.Entry(property).State = EntityState.Modified;
                    dbContext.SaveChanges();
                    LoadApartments();
                    ClearApartmentsInputs();
                    ShowPopup("Объект недвижимости успешно обновлен!", true);
                }
                catch (Exception ex)
                {
                    ShowPopup($"Ошибка при обновлении объекта недвижимости: {ex.Message}", false);
                }
            }
        }

        private void DeleteApartmentsButton_Click(object sender, RoutedEventArgs e)
        {
            if (ApartmentsDataGrid.SelectedItem == null) return;

            var selectedProperty = (Apartments)ApartmentsDataGrid.SelectedItem;
            var property = dbContext.Apartments.Find(selectedProperty.ID);

            if (property != null)
            {
                try
                {
                    dbContext.Apartments.Remove(property);
                    dbContext.SaveChanges();
                    LoadApartments();
                    ClearApartmentsInputs();
                    ShowPopup("Объект недвижимости успешно удален!", true);
                }
                catch (Exception ex)
                {
                    ShowPopup($"Ошибка при удалении объекта недвижимости: {ex.Message}", false);
                }
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
                ShowPopup("Все поля должны быть заполнены.", false);
                return;
            }

            if (!int.TryParse(SupplyIDTextBox.Text, out int supplyId) || supplyId < 0)
            {
                ShowPopup("ID должен быть положительным числом.", false);
                return;
            }

            if (!double.TryParse(SupplyPriceTextBox.Text, out double price))
            {
                ShowPopup("Цена должна быть числом.", false);
                return;
            }

            if (dbContext.Supplies.Any(s => s.ID == supplyId))
            {
                ShowPopup("Предложение с таким ID уже существует.", false);
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
                ShowPopup("Предложение успешно добавлено!", true);
            }
            catch (Exception ex)
            {
                ShowPopup($"Ошибка при добавлении предложения: {ex.Message}", false);
            }
        }

        private void UpdateSupplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (SuppliesDataGrid.SelectedItem == null)
            {
                ShowPopup("Выберите предложение для обновления.", false);
                return;
            }

            try
            {
                dynamic selectedSupply = SuppliesDataGrid.SelectedItem;
                int supplyId = selectedSupply.ID;
                var supply = dbContext.Supplies.Find(supplyId);

                if (supply != null)
                {
                    supply.ID = int.Parse(SupplyIDTextBox.Text);
                    supply.Price = Convert.ToString(SupplyPriceTextBox.Text);
                    supply.FK_AgentID = (int)SupplyAgentComboBox.SelectedValue;
                    supply.FK_ClientID = (int)SupplyClientComboBox.SelectedValue;
                    supply.FK_ApartmentsID = (int)SupplyApartmentComboBox.SelectedValue;

                    dbContext.SaveChanges();
                    LoadSupplies();
                    ClearSupplyInputs();
                    ShowPopup("Предложение успешно обновлено!", true);
                }
                else
                {
                    ShowPopup("Выбранное предложение не найдено в базе данных.", false);
                }
            }
            catch (Exception ex)
            {
                ShowPopup($"Ошибка при обновлении предложения: {ex.Message}", false);
            }
        }

        private void DeleteSupplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (SuppliesDataGrid.SelectedItem == null)
            {
                ShowPopup("Выберите предложение для удаления.", false);
                return;
            }

            try
            {
                dynamic selectedSupply = SuppliesDataGrid.SelectedItem;
                int supplyId = selectedSupply.ID;
                var supply = dbContext.Supplies.Find(supplyId);

                if (supply != null)
                {
                    dbContext.Supplies.Remove(supply);
                    dbContext.SaveChanges();
                    LoadSupplies();
                    ClearSupplyInputs();
                    ShowPopup("Предложение успешно удалено!", true);
                }
                else
                {
                    ShowPopup("Выбранное предложение не найдено в базе данных.", false);
                }
            }
            catch (Exception ex)
            {
                ShowPopup($"Ошибка при удалении предложения: {ex.Message}", false);
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
            UnSuccessPopup.IsOpen = false;
        }

        private void AddDemandsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateDemandsInputs())
            {
                ShowPopup("Все поля должны быть заполнены корректно.", false);
                return;
            }

            if (!int.TryParse(DemandsIDTextBox.Text, out int demandId) || demandId < 0)
            {
                ShowPopup("ID должен быть положительным числом.", false);
                return;
            }

            if (!double.TryParse(DemandsMinPriceTextBox.Text, out double minPrice) ||
                !double.TryParse(DemandsMaxPriceTextBox.Text, out double maxPrice))
            {
                ShowPopup("Цены должны быть числом.", false);
                return;
            }

            if (!int.TryParse(DemandsAgentIDTextBox.Text, out int agentId) ||
                !int.TryParse(DemandsClientIDTextBox.Text, out int clientId) ||
                !int.TryParse(DemandsTypeIDTextBox.Text, out int typeId))
            {
                ShowPopup("Агент, клиент и тип объекта должны быть числом.", false);
                return;
            }

            if (!dbContext.Agents.Any(a => a.ID == agentId))
            {
                ShowPopup("Агент с таким ID не найден.", false);
                return;
            }

            if (!dbContext.Clients.Any(c => c.ID == clientId))
            {
                ShowPopup("Клиент с таким ID не найден.", false);
                return;
            }

            if (!dbContext.Type_Object.Any(t => t.ID == typeId))
            {
                ShowPopup("Тип объекта с таким ID не найден.", false);
                return;
            }

            Demands newDemand = new Demands
            {
                ID = demandId,
                Adress_City = DemandsCityTextBox.Text,
                Adress_Street = DemandsStreetTextBox.Text,
                Adress_House = DemandsHouseTextBox.Text,
                Adress_Number = Convert.ToInt32(DemandsNumberTextBox.Text),
                Min_Price = minPrice,
                Max_Price = maxPrice,
                FK_AgentID = agentId,
                FK_ClientID = clientId,
                FK_Type_Object_ID = typeId,
                MinArea = Convert.ToDouble(DemandsMinAreaTextBox.Text),
                MaxArea = Convert.ToDouble(DemandsMaxAreaTextBox.Text),
                MinRooms = Convert.ToInt32(DemandsMinRoomsTextBox.Text),
                MaxRooms = Convert.ToInt32(DemandsMaxRoomsTextBox.Text),
                MinFloor = Convert.ToInt32(DemandsMinFloorTextBox.Text),
                MaxFloor = Convert.ToInt32(DemandsMaxFloorTextBox.Text)
            };

            try
            {
                dbContext.Demands.Add(newDemand);
                dbContext.SaveChanges();
                LoadDemands();
                ClearDemandInputs();
                ShowPopup("Потребность успешно добавлена!", true);
            }
            catch (Exception ex)
            {
                ShowPopup($"Ошибка при добавлении потребности: {ex.Message}", false);
            }
        }

        private void DemandsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DemandsDataGrid.SelectedItem != null)
            {
                dynamic selectedDemand = DemandsDataGrid.SelectedItem;

                // Проверяем, что свойства объекта не равны null перед приведением их к строке
                DemandsIDTextBox.Text = selectedDemand.ID?.ToString();
                DemandsCityTextBox.Text = selectedDemand.Adress_City;
                DemandsStreetTextBox.Text = selectedDemand.Adress_Street;
                DemandsHouseTextBox.Text = selectedDemand.Adress_House;
                DemandsNumberTextBox.Text = selectedDemand.Adress_Number?.ToString();
                DemandsMinPriceTextBox.Text = selectedDemand.Min_Price?.ToString();
                DemandsMaxPriceTextBox.Text = selectedDemand.Max_Price?.ToString();
                DemandsAgentIDTextBox.Text = selectedDemand.FK_AgentID?.ToString();
                DemandsClientIDTextBox.Text = selectedDemand.FK_ClientID?.ToString();
                DemandsMinAreaTextBox.Text = selectedDemand.MinArea?.ToString();
                DemandsMaxAreaTextBox.Text = selectedDemand.MaxArea?.ToString();
                DemandsMinRoomsTextBox.Text = selectedDemand.MinRooms?.ToString();
                DemandsMaxRoomsTextBox.Text = selectedDemand.MaxRooms?.ToString();
                DemandsMinFloorTextBox.Text = selectedDemand.MinFloor?.ToString();
                DemandsMaxFloorTextBox.Text = selectedDemand.MaxFloor?.ToString();
                DemandsTypeIDTextBox.Text = selectedDemand.FK_Type_Object_ID?.ToString();
            }
        }





        private void ClearDemandInputs()
        {
            DemandsIDTextBox.Text = "";
            DemandsCityTextBox.Text = "";
            DemandsStreetTextBox.Text = "";
            DemandsHouseTextBox.Text = "";
            DemandsNumberTextBox.Text = "";
            DemandsMinPriceTextBox.Text = "";
            DemandsMaxPriceTextBox.Text = "";
            DemandsAgentIDTextBox.Text = "";
            DemandsClientIDTextBox.Text = "";
            DemandsMinAreaTextBox.Text = "";
            DemandsMaxAreaTextBox.Text = "";
            DemandsMinRoomsTextBox.Text = "";
            DemandsMaxRoomsTextBox.Text = "";
            DemandsMinFloorTextBox.Text = "";
            DemandsMaxFloorTextBox.Text = "";
            DemandsTypeIDTextBox.Text = "";
        }


        // Метод для проверки корректности введенных данных перед добавлением
        private bool ValidateDemandsInputs()
        {
            return !string.IsNullOrWhiteSpace(DemandsIDTextBox.Text) &&
                   !string.IsNullOrWhiteSpace(DemandsCityTextBox.Text) &&
                   !string.IsNullOrWhiteSpace(DemandsStreetTextBox.Text) &&
                   !string.IsNullOrWhiteSpace(DemandsHouseTextBox.Text) &&
                   !string.IsNullOrWhiteSpace(DemandsNumberTextBox.Text) &&
                   !string.IsNullOrWhiteSpace(DemandsMinPriceTextBox.Text) &&
                   !string.IsNullOrWhiteSpace(DemandsMaxPriceTextBox.Text) &&
                   !string.IsNullOrWhiteSpace(DemandsAgentIDTextBox.Text) &&
                   !string.IsNullOrWhiteSpace(DemandsClientIDTextBox.Text) &&
                   !string.IsNullOrWhiteSpace(DemandsMinAreaTextBox.Text) &&
                   !string.IsNullOrWhiteSpace(DemandsMaxAreaTextBox.Text) &&
                   !string.IsNullOrWhiteSpace(DemandsMinRoomsTextBox.Text) &&
                   !string.IsNullOrWhiteSpace(DemandsMaxRoomsTextBox.Text) &&
                   !string.IsNullOrWhiteSpace(DemandsMinFloorTextBox.Text) &&
                   !string.IsNullOrWhiteSpace(DemandsMaxFloorTextBox.Text) &&
                   !string.IsNullOrWhiteSpace(DemandsTypeIDTextBox.Text);
        }



        private void UpdateDemandsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(DemandsIDTextBox.Text, out int demandId) || demandId < 0)
            {
                ShowPopup("ID должен быть положительным числом.", false);
                return;
            }

            var existingDemand = dbContext.Demands.FirstOrDefault(d => d.ID == demandId);
            if (existingDemand == null)
            {
                ShowPopup("Потребность с таким ID не найдена.", false);
                return;
            }

            if (!double.TryParse(DemandsMinPriceTextBox.Text, out double minPrice) ||
                !double.TryParse(DemandsMaxPriceTextBox.Text, out double maxPrice))
            {
                ShowPopup("Цены должны быть числом.", false);
                return;
            }

            if (!int.TryParse(DemandsAgentIDTextBox.Text, out int agentId) ||
                !int.TryParse(DemandsClientIDTextBox.Text, out int clientId) ||
                !int.TryParse(DemandsTypeIDTextBox.Text, out int typeId) ||
                !double.TryParse(DemandsMinAreaTextBox.Text, out double minArea) ||
                !double.TryParse(DemandsMaxAreaTextBox.Text, out double maxArea) ||
                !int.TryParse(DemandsMinRoomsTextBox.Text, out int minRooms) ||
                !int.TryParse(DemandsMaxRoomsTextBox.Text, out int maxRooms) ||
                !int.TryParse(DemandsMinFloorTextBox.Text, out int minFloor) ||
                !int.TryParse(DemandsMaxFloorTextBox.Text, out int maxFloor))
            {
                ShowPopup("Некоторые поля содержат некорректные данные.", false);
                return;
            }

            existingDemand.Adress_City = DemandsCityTextBox.Text;
            existingDemand.Adress_Street = DemandsStreetTextBox.Text;
            existingDemand.Adress_House = DemandsHouseTextBox.Text;
            existingDemand.Adress_Number = Convert.ToInt32(DemandsNumberTextBox.Text);
            existingDemand.Min_Price = Convert.ToDouble(minPrice);
            existingDemand.Max_Price = Convert.ToDouble(maxPrice);
            existingDemand.FK_AgentID = agentId;
            existingDemand.FK_ClientID = clientId;
            existingDemand.MinArea = Convert.ToDouble(minArea);
            existingDemand.MaxArea = Convert.ToDouble(maxArea);
            existingDemand.MinRooms = minRooms;
            existingDemand.MaxRooms = maxRooms;
            existingDemand.MinFloor = minFloor;
            existingDemand.MaxFloor = maxFloor;
            existingDemand.FK_Type_Object_ID = typeId;

            try
            {
                dbContext.SaveChanges();
                LoadDemands();
                ClearDemandInputs();
                ShowPopup("Потребность успешно обновлена!", true);
            }
            catch (Exception ex)
            {
                ShowPopup($"Ошибка при обновлении потребности: {ex.Message}", false);
            }
        }

        private void DeleteDemandsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(DemandsIDTextBox.Text, out int demandId) || demandId < 0)
            {
                ShowPopup("ID должен быть положительным числом.", false);
                return;
            }

            var demandToDelete = dbContext.Demands.FirstOrDefault(d => d.ID == demandId);
            if (demandToDelete == null)
            {
                ShowPopup("Потребность с таким ID не найдена.", false);
                return;
            }

            try
            {
                dbContext.Demands.Remove(demandToDelete);
                dbContext.SaveChanges();
                LoadDemands();
                ClearDemandInputs();
                ShowPopup("Потребность успешно удалена!", true);
            }
            catch (Exception ex)
            {
                ShowPopup($"Ошибка при удалении потребности: {ex.Message}", false);
            }
        }

        private void AddDealButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!int.TryParse(IDDealsTextBox.Text, out int id))
                {
                    ShowPopup("Поле ID должно содержать целое число.", false);
                    return;
                }

                if (id < 0)
                {
                    ShowPopup("ID должен быть положительным числом.", false);
                    return;
                }

                if (!int.TryParse(DemandssIDTextBox.Text, out int demandId))
                {
                    ShowPopup("Поле ID Потребности должно содержать целое число.", false);
                    return;
                }

                if (demandId < 0)
                {
                    ShowPopup("ID Потребности должен быть положительным числом.", false);
                    return;
                }

                if (!int.TryParse(SuppliesIDTextBox.Text, out int supplyId))
                {
                    ShowPopup("Поле ID Предложения должно содержать целое число.", false);
                    return;
                }

                if (supplyId < 0)
                {
                    ShowPopup("ID Предложения должен быть положительным числом.", false);
                    return;
                }

                var demandExists = dbContext.Demands.Any(d => d.ID == demandId);

                if (!demandExists)
                {
                    ShowPopup("Потребность с указанным ID не найдена.", false);
                    return;
                }

                var supplyExists = dbContext.Supplies.Any(s => s.ID == supplyId);

                if (!supplyExists)
                {
                    ShowPopup("Предложение с указанным ID не найдено.", false);
                    return;
                }

                dbContext.Deals.Add(new Deals { ID = id, Demant_ID = demandId, Supply_ID = supplyId });
                dbContext.SaveChanges();

                LoadDeals();

                ShowPopup("Сделка успешно добавлена!", true);
            }
            catch (Exception ex)
            {
                ShowPopup($"Ошибка при добавлении сделки: {ex.Message}", false);
            }
        }








        private void UpdateDealButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(IDDealsTextBox.Text, out int id) || id < 0)
            {
                ShowPopup("ID должен быть положительным числом.", false);
                return;
            }

            var existingDeal = dbContext.Deals.FirstOrDefault(d => d.ID == id);
            if (existingDeal == null)
            {
                ShowPopup("Сделка с таким ID не найдена.", false);
                return;
            }

            if (!int.TryParse(DemandssIDTextBox.Text, out int demandId))
            {
                ShowPopup("Поле ID Потребности должно содержать целое число.", false);
                return;
            }

            if (demandId < 0)
            {
                ShowPopup("ID Потребности должен быть положительным числом.", false);
                return;
            }

            if (!int.TryParse(SuppliesIDTextBox.Text, out int supplyId))
            {
                ShowPopup("Поле ID Предложения должно содержать целое число.", false);
                return;
            }

            if (supplyId < 0)
            {
                ShowPopup("ID Предложения должен быть положительным числом.", false);
                return;
            }

            var demandExists = dbContext.Demands.Any(d => d.ID == demandId);
            if (!demandExists)
            {
                ShowPopup("Потребность с указанным ID не найдена.", false);
                return;
            }

            var supplyExists = dbContext.Supplies.Any(s => s.ID == supplyId);
            if (!supplyExists)
            {
                ShowPopup("Предложение с указанным ID не найдено.", false);
                return;
            }

            existingDeal.Demant_ID = demandId;
            existingDeal.Supply_ID = supplyId;

            try
            {
                dbContext.SaveChanges();
                LoadDeals();
                ClearDealInputs();
                ShowPopup("Сделка успешно обновлена!", true);
            }
            catch (Exception ex)
            {
                ShowPopup($"Ошибка при обновлении сделки: {ex.Message}", false);
            }
        }


        private void ClearDealInputs()
        {
            IDTextBox.Text = string.Empty;
            DemandssIDTextBox.Text = string.Empty;
            SuppliesIDTextBox.Text = string.Empty;
        }



        private void DeleteDealButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(IDDealsTextBox.Text, out int dealId) || dealId < 0)
            {
                ShowPopup("ID должен быть положительным числом.", false);
                return;
            }

            var dealToDelete = dbContext.Deals.FirstOrDefault(d => d.ID == dealId);
            if (dealToDelete == null)
            {
                ShowPopup("Сделка с таким ID не найдена.", false);
                return;
            }

            try
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить выбранную сделку?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                {
                    return;
                }

                dbContext.Deals.Remove(dealToDelete);
                dbContext.SaveChanges();
                LoadDeals();
                ClearDealInputs();
                ShowPopup("Сделка успешно удалена!", true);
            }
            catch (Exception ex)
            {
                ShowPopup($"Ошибка при удалении сделки: {ex.Message}", false);
            }
        }


        private void DealsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (DealsDataGrid.SelectedItem != null)
                {
                    dynamic selectedDeal = DealsDataGrid.SelectedItem;
                    IDDealsTextBox.Text = selectedDeal.ID.ToString();
                    DemandssIDTextBox.Text = selectedDeal.Demant_ID.ToString();
                    SuppliesIDTextBox.Text = selectedDeal.Supply_ID.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выборе сделки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int LevenshteinDistance(string source, string target)
        {
            if (string.IsNullOrEmpty(source)) return target.Length;
            if (string.IsNullOrEmpty(target)) return source.Length;

            int[,] distance = new int[source.Length + 1, target.Length + 1];

            for (int i = 0; i <= source.Length; distance[i, 0] = i++) { }
            for (int j = 0; j <= target.Length; distance[0, j] = j++) { }

            for (int i = 1; i <= source.Length; i++)
            {
                for (int j = 1; j <= target.Length; j++)
                {
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;
                    distance[i, j] = Math.Min(
                        Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                        distance[i - 1, j - 1] + cost);
                }
            }

            return distance[source.Length, target.Length];
        }

        private void SearchApartmentsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadApartments(); // Загрузка всех данных перед фильтрацией

            string query = SearchTextBox.Text;

            if (string.IsNullOrWhiteSpace(query))
            {
                MessageBox.Show("Введите поисковый запрос.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var results = dbContext.Apartments.ToList().Where(apartment =>
                LevenshteinDistance(apartment.Adress_City, query) <= 3 ||
                LevenshteinDistance(apartment.Adress_Street, query) <= 3 ||
                LevenshteinDistance(apartment.Adress_House.ToString(), query) <= 1 ||
                LevenshteinDistance(apartment.Adress_Number.ToString(), query) <= 1
            ).ToList();

            ApartmentsDataGrid.ItemsSource = results;

            if (results.Count == 0)
            {
                MessageBox.Show("Нет объектов недвижимости, соответствующих критериям поиска.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            LoadApartments(); // Метод для загрузки всех данных
        }

        public void ShowPopup(string message, bool isSuccess)
        {
            // Сначала закрываем все открытые всплывающие окна
            ClosePopups();

            if (isSuccess)
            {
                SuccessPopupText.Text = message;
                SuccessPopup.IsOpen = true;
                isShowingSuccessPopup = true;
            }
            else
            {
                UnSuccessPopupText.Text = message;
                UnSuccessPopup.IsOpen = true;
                isShowingUnSuccessPopup = true;
            }

            // Устанавливаем таймер на закрытие уведомления через 3 секунды
            timer.Interval = 3000;
            timer.Start();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            // Закрываем открытые уведомления после истечения времени
            ClosePopups();
        }

        private void ClosePopups()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (isShowingSuccessPopup)
                {
                    SuccessPopup.IsOpen = false;
                    isShowingSuccessPopup = false;
                }

                if (isShowingUnSuccessPopup)
                {
                    UnSuccessPopup.IsOpen = false;
                    isShowingUnSuccessPopup = false;
                }
            });
        }


    }
}
