using System.Linq;
using System.Windows;

namespace ShoeStore_ChebodaevKirill
{
    public partial class AuthorizationWindow : Window
    {
        public AuthorizationWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = TbLogin.Text.Trim();
            string password = PbPassword.Password.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var context = ShoeStore_ChebodaevKirillEntities.GetContext();
            
            var user = context.Users.FirstOrDefault(u => u.Login == login && u.Password == password);

            if (user != null)
            {
                
                MessageBox.Show($"Добро пожаловать, {user.FullName}!\nРоль: {user.Roles.RoleName}", "Успех");

                MainMenuWindow menuWindow = new MainMenuWindow(user);
                menuWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль!", "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnGuest_Click(object sender, RoutedEventArgs e)
        {
            MainMenuWindow menuWindow = new MainMenuWindow(null);
            menuWindow.Show();
            this.Close();
        }
    }
}
