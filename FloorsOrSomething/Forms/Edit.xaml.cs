using System;
using System.Windows;
using Npgsql;
using System.Windows.Controls;
using System.Windows.Input;
using FloorsOrSomething.Classes;

namespace FloorsOrSomething.Forms
{
    /// <summary>
    /// Логика взаимодействия для Edit.xaml
    /// </summary>
    public partial class Edit : Window
    {
        private DatabaseHelper _dbHelper;
        private string _tableName;
        private ButtonAnimator buttonAnimator;

        public Edit(string tableName)
        {
            InitializeComponent();
            UpdateGridVisibility();
            _tableName = tableName;

            _dbHelper = new DatabaseHelper("host=localhost;port=5433;database=Floors;username=postgres;password=12345");

            LoadProducts();
            LoadPartners();

            buttonAnimator = new ButtonAnimator();

            // Подписка на события мыши для кнопки
            ExitBT.MouseEnter += Button_MouseEnter;
            ExitBT.MouseLeave += Button_MouseLeave;
        }

        private void LoadPartners()
        {
            try
            {
                var partners = _dbHelper.GetPartners();
                Console.WriteLine($"Loaded {partners.Count} partners."); // Отладочный вывод
                partner_nameCB.ItemsSource = partners;
                partner_nameCB.DisplayMemberPath = "PartnerName";
                partner_nameCB.SelectedValuePath = "PartnerId";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading partners: {ex.Message}");
            }
        }

        private void LoadProducts()
        {
            var products = _dbHelper.GetProducts();
            product_nameCB.ItemsSource = products; // Правильный ComboBox для продуктов
            product_nameCB.DisplayMemberPath = "ProductName"; // Отображаемое поле
            product_nameCB.SelectedValuePath = "Id"; // Значение для сохранения
        }

        private void UpdateGridVisibility()
        {
            // Hide both grids initially
            partners_product.Visibility = Visibility.Collapsed;
            partners.Visibility = Visibility.Collapsed;

            // Show the appropriate grid based on the selected tag
            if (SharedData.SelectedTag == "partner_products")
            {
                partners_product.Visibility = Visibility.Visible;
            }
            else if (SharedData.SelectedTag == "partners")
            {
                partners.Visibility = Visibility.Visible;
            }
        }

        private void Button_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var button = (Button)sender;
            buttonAnimator.OnMouseEnter(button);
        }

        private void Button_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var button = (Button)sender;
            buttonAnimator.OnMouseLeave(button);
        }

        private void CloseButton_Click (object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Проверяем, что нажата левая кнопка мыши
            if (e.ChangedButton == MouseButton.Left)
            {
                // Начинаем перетаскивание окна
                this.DragMove();
            }
        }
        private void partnerTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (partnerTypeComboBox.SelectedItem != null)
            {
                // Приводим выбранный элемент к ComboBoxItem
                ComboBoxItem selectedItem = (ComboBoxItem)partnerTypeComboBox.SelectedItem;

                // Получаем тег
                string tag = selectedItem.Tag.ToString();

                // Выводим тег или используем его по необходимости
                MessageBox.Show($"Выбранный тег: {tag}", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AddBT_Click(object sender, RoutedEventArgs e)
        {
            if (SharedData.SelectedTag == "partner_products")
            {
                // Получаем ID партнёра из ComboBox
                int partnerId = (int)partner_nameCB.SelectedValue;

                // Получаем ID продукта из ComboBox
                int productId = (int)product_nameCB.SelectedValue;

                // Проверяем, введено ли количество продуктов
                int quantity;
                if (!int.TryParse(productCountTB.Text.Trim(), out quantity))
                {
                    MessageBox.Show("Пожалуйста, введите корректное количество продуктов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверяем, введена ли дата продажи
                DateTime dateOfSale;
                if (!DateTime.TryParse(dateOfSaleTB.Text.Trim(), out dateOfSale))
                {
                    MessageBox.Show("Пожалуйста, введите корректную дату продажи.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    // Вызов метода с правильными параметрами
                    _dbHelper.AddPartnerProduct(partnerId, productId, quantity, dateOfSale);
                    MessageBox.Show("Данные успешно добавлены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Очистка полей после успешного добавления
                    partner_nameCB.SelectedIndex = -1;
                    product_nameCB.SelectedIndex = -1;
                    productCountTB.Clear();
                    dateOfSaleTB.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (SharedData.SelectedTag == "partners")
            {
                if (partnerTypeComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Пожалуйста, выберите тип партнера.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Получаем ID типа партнера из ComboBox
                ComboBoxItem selectedItem = partnerTypeComboBox.SelectedItem as ComboBoxItem;
                if (selectedItem == null)
                {
                    MessageBox.Show("Ошибка: выбранный элемент не является ComboBoxItem.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int partnerType;
                try
                {
                    partnerType = Convert.ToInt32(selectedItem.Tag);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при получении ID типа партнера: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Сбор данных из текстовых полей
                var partnerName = partner_nameTB?.Text.Trim();
                var director = directorTB?.Text.Trim();
                var email = emailTB?.Text.Trim();
                var phoneNumber = phone_numberTB?.Text.Trim();
                var legalAddress = legal_addressTB?.Text.Trim();
                var tin = tinTB?.Text.Trim();
                var ratingString = ratingTB?.Text.Trim(); // Изменено на ratingString

                // Проверка на заполненность полей
                if (string.IsNullOrWhiteSpace(partnerName) || string.IsNullOrWhiteSpace(director) ||
                    string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(phoneNumber) ||
                    string.IsNullOrWhiteSpace(legalAddress) || string.IsNullOrWhiteSpace(tin) ||
                    string.IsNullOrWhiteSpace(ratingString)) // Изменено на ratingString
                {
                    MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Преобразование строки рейтинга в decimal
                decimal ratingValue;
                if (!decimal.TryParse(ratingString, out ratingValue))
                {
                    MessageBox.Show("Пожалуйста, введите корректное значение для рейтинга.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    // Вызов метода добавления партнера с правильными параметрами
                    _dbHelper.AddPartner(partnerType, partnerName, director, email, phoneNumber, legalAddress, tin, ratingValue); // Изменено на ratingValue
                    MessageBox.Show("Партнер успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Очистка полей после успешного добавления
                    partnerTypeComboBox.SelectedIndex = -1;
                    partner_nameTB.Clear();
                    directorTB.Clear();
                    emailTB.Clear();
                    phone_numberTB.Clear();
                    legal_addressTB.Clear();
                    tinTB.Clear();
                    ratingTB.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении партнера: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }

            
        }


    }
}
