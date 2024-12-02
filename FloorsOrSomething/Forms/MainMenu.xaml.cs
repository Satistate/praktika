using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using FloorsOrSomething.Classes;

namespace FloorsOrSomething.Forms
{
    /// <summary>
    /// Логика взаимодействия для MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Window
    {
        private string connectionString = "host = localhost; port = 5433; database = Floors; username = postgres; password = 12345";

        private ButtonAnimator buttonAnimator;

        public MainMenu()
        {
            InitializeComponent();
            buttonAnimator = new ButtonAnimator();
            // Подписка на события мыши для кнопки
            ExitBT.MouseEnter += Button_MouseEnter;
            ExitBT.MouseLeave += Button_MouseLeave;
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

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Проверяем, что нажата левая кнопка мыши
            if (e.ChangedButton == MouseButton.Left)
            {
                // Начинаем перетаскивание окна
                this.DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) 
        {
             this.Close();
        }
       

        private void TableSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tableSelectionComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                SharedData.SelectedTag = selectedItem.Tag as string;
                string selectedTable = selectedItem.Tag.ToString();
                DataTable data = LoadData(selectedTable);
                dataGrid.ItemsSource = data.DefaultView;
                // Проверка, если DataGrid уже виден
                if (dataGrid.Opacity < 1)
                {
                    // Здесь вы можете добавить логику для обновления данных в DataGrid
                    FadeIn(dataGrid);
                }
            }

        }



        private DataTable LoadData(string tableName)
        {
            DataTable dataTable = new DataTable();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                if (tableName == "partners")
                {
                    // Обновление значений в столбце discount
                    UpdateDiscounts(connection);

                    // Получение данных для таблицы partners
                    string query = @"
                SELECT p.partner_id AS ""ID"", 
                       pt.partner_type_name AS ""Тип Партнера"",
                       p.partner_name AS ""Название Партнера"", 
                       p.director AS ""Директор"", 
                       p.email AS ""Электронная Почта"", 
                       p.phone_number AS ""Номер Телефона"", 
                       p.legal_address AS ""Юридический Адрес"", 
                       p.TIN AS ""ИНН"", 
                       p.rating AS ""Рейтинг"", 
                       p.discount AS ""Скидка""  -- Добавляем скидку в выборку
                FROM partners p
                JOIN partner_type pt ON p.partner_type = pt.partner_type_id;";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        using (var adapter = new NpgsqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
                else if (tableName == "partner_products")
                {
                    // Получение данных для таблицы partner_products
                    string query = @"
                SELECT pp.partner_products_id AS ""ID"", 
                       p.product_name AS ""Название Продукта"",
                       pa.partner_name AS ""Название Партнера"",
                       pp.quantity_of_products AS ""Количество Продуктов"", 
                       pp.date_of_sale AS ""Дата Продажи"" 
                FROM partner_products pp
                JOIN product p ON pp.product = p.product_id
                JOIN partners pa ON pp.partner_name = pa.partner_id;";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        using (var adapter = new NpgsqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
            }

            return dataTable;
        }

        private void UpdateDiscounts(NpgsqlConnection connection)
        {
            // Проверка наличия столбца discount
            var checkColumnCommand = new NpgsqlCommand("SELECT column_name FROM information_schema.columns WHERE table_name = 'partners' AND column_name = 'discount';", connection);
            var columnExists = checkColumnCommand.ExecuteScalar() != null;

            // Если столбец не существует, создаем его
            if (!columnExists)
            {
                var addColumnCommand = new NpgsqlCommand("ALTER TABLE partners ADD COLUMN discount INTEGER DEFAULT 0;", connection);
                addColumnCommand.ExecuteNonQuery();
            }

            // Обновление значений в столбце discount
            var updateCommand = new NpgsqlCommand(@"
        UPDATE partners p
        SET discount = CASE 
            WHEN COALESCE(total_quantity, 0) < 10000 THEN 0
            WHEN COALESCE(total_quantity, 0) >= 10000 AND COALESCE(total_quantity, 0) < 50000 THEN 5
            WHEN COALESCE(total_quantity, 0) >= 50000 AND COALESCE(total_quantity, 0) < 300000 THEN 10
            ELSE 15
        END
        FROM (
            SELECT 
                pp.partner_name,  
                SUM(COALESCE(pp.quantity_of_products::INTEGER, 0)) AS total_quantity  -- Приведение к INTEGER
            FROM 
                partner_products pp
            GROUP BY 
                pp.partner_name
        ) AS subquery
        WHERE p.partner_id = subquery.partner_name;", connection);

            updateCommand.ExecuteNonQuery();
        }

        private void ReloadCB_Click(object sender, RoutedEventArgs e)
        {
            // Сброс выбранного элемента в ComboBox
            tableSelectionComboBox.SelectedIndex = -1; // Устанавливаем индекс на -1 для сброса выбора
            FadeOut(dataGrid);
            // Очистка DataGrid
            dataGrid.ItemsSource = null; // Устанавливаем источник данных в null
        }

        private void AddBT_Click(object sender, RoutedEventArgs e)
        {
            if (tableSelectionComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string tableName = selectedItem.Tag.ToString(); // Получаем имя таблицы
                Edit editWindow = new Edit(tableName);
                editWindow.ShowDialog(); // Открываем форму как модальное окно
            }
        }



        private void FadeIn(UIElement element)
        {
            // Убедитесь, что элемент видим перед анимацией
            element.Visibility = Visibility.Visible;

            DoubleAnimation fadeInAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromSeconds(0.5))
            };
            element.BeginAnimation(UIElement.OpacityProperty, fadeInAnimation);
        }

        private void FadeOut(UIElement element)
        {
            DoubleAnimation fadeOutAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(0.5))
            };
            fadeOutAnimation.Completed += (s, e) =>
            {
                // После завершения затухания скрываем элемент
                element.Visibility = Visibility.Collapsed; // или Visibility.Hidden
            };
            element.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimation);
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            // Получаем текущее окно
            Window window = Window.GetWindow(this);

            if (window != null)
            {
                // Минимизируем окно
                window.WindowState = WindowState.Minimized;
            }
        }

    }
}
