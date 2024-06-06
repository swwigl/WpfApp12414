using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public partial class AgentsWindow : Window
    {
        private RealEstateDBEntities dbContext = new RealEstateDBEntities();

        public AgentsWindow()
        {
            InitializeComponent();
            LoadAgents();
        }

        private void LoadAgents()
        {
            AgentsDataGrid.ItemsSource = dbContext.Agents.ToList();
        }

        private void AddAgentButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка, что все поля заполнены
            if (string.IsNullOrWhiteSpace(IDTextBox.Text) ||
                string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
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

            // Проверка, что ID уникален
            if (dbContext.Agents.Any(a => a.ID == agentId))
            {
                MessageBox.Show("Агент с таким ID уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Создание нового агента
            var newAgent = new Agents
            {
                ID = agentId,
                FirstName = FirstNameTextBox.Text,
                LastName = LastNameTextBox.Text,
                DealShare = dealShare.ToString()
            };

            try
            {
                // Добавление нового агента в контекст и сохранение изменений
                dbContext.Agents.Add(newAgent);
                dbContext.SaveChanges();

                // Обновление списка агентов в DataGrid и очистка полей ввода
                LoadAgents();
                ClearInputs();

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
                agent.LastName = LastNameTextBox.Text;

                if (double.TryParse(DealShareTextBox.Text, out double dealShare))
                {
                    agent.DealShare = dealShare.ToString(); // Преобразование double в string
                }
                else
                {
                    MessageBox.Show("Доля в сделке должна быть числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                dbContext.Entry(agent).State = EntityState.Modified;
                dbContext.SaveChanges();
                LoadAgents();
                ClearInputs();
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
                ClearInputs();
                MessageBox.Show("Агент успешно удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AgentsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AgentsDataGrid.SelectedItem == null) return;

            var selectedAgent = (Agents)AgentsDataGrid.SelectedItem;
            IDTextBox.Text = selectedAgent.ID.ToString();
            FirstNameTextBox.Text = selectedAgent.FirstName;
            LastNameTextBox.Text = selectedAgent.LastName;
            DealShareTextBox.Text = selectedAgent.DealShare.ToString();
        }

        private void ClearInputs()
        {
            IDTextBox.Clear();
            FirstNameTextBox.Clear();
            LastNameTextBox.Clear();
            DealShareTextBox.Clear();
        }
    }
}
