using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Npgsql;
using System.Windows.Threading;
using FloorsOrSomething.Forms;

namespace FloorsOrSomething
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string connectionString = "host = localhost; port = 5433; database = Floors; username = postgres; password = 12345";

        private ButtonAnimator buttonAnimator;
        public MainWindow()
        {
            InitializeComponent();
            this.ShowInTaskbar = false;
            this.Loaded += Authorization_Loaded;

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
            this.Close(); // Закрывает текущее окно
        }

        private void Authorization_Loaded(object sender, RoutedEventArgs e)
        {
            // Запуск анимации
            AnimateLetters();
        }

        private void METB(object sender, MouseEventArgs e)
        {
            // Запуск анимации увеличения
            Storyboard storyboard = (Storyboard)this.Resources["MouseEnterAnimation"];
            storyboard.Begin();
        }

        private void MLTB(object sender, MouseEventArgs e)
        {
            // Запуск анимации уменьшения
            Storyboard storyboard = (Storyboard)this.Resources["MouseLeaveAnimation"];
            storyboard.Begin();
        }

        private void LoginTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PasswordTB.Focus();
                e.Handled = true;
            }
        }
        private void passwordTB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginB_Click(null, new RoutedEventArgs());
                e.Handled = true;
            }
        }

        private void LoginB_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTB.Text;
            string password = PasswordTB.Password;

            using (var connection = new NpgsqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // SQL-запрос для проверки наличия пользователя
                    string query = "SELECT COUNT(*) FROM users WHERE user_login = @login AND user_password = @password";

                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        // Используйте параметры для предотвращения SQL-инъекций
                        cmd.Parameters.AddWithValue("login", login);
                        cmd.Parameters.AddWithValue("password", password);

                        // Выполняем запрос
                        int userCount = Convert.ToInt32(cmd.ExecuteScalar());

                        if (userCount > 0)
                        {
                            // Если пользователь найден, открываем MainWindow
                            MainMenu mainWindow = new MainMenu();
                            mainWindow.Show();
                            this.Close(); // Закрываем текущее окно (если это необходимо)
                        }
                        else
                        {
                            MessageBox.Show("Неверный логин или пароль.", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AnimateLetters()
        {
            // Массив букв
            TextBlock[] letters = { Letter1, Letter2, Letter3, Letter4, Letter5, Letter6, Letter7, Letter8, Letter9, Letter10 };

            // Анимация для каждой буквы
            for (int i = 0; i < letters.Length; i++)
            {
                DoubleAnimation opacityAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                    BeginTime = TimeSpan.FromSeconds(i * 0.2) // Задержка между буквами
                };

                letters[i].BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
            }
        }

        private void LoginTB_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
         
        private void LoginTB_KeyDown2(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PasswordTB.Focus();
                e.Handled = true;
            }
        }
        private void passwordTB_PreviewKeyDown2(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginB_Click(null, new RoutedEventArgs());
                e.Handled = true;
            }
        }
    }
}