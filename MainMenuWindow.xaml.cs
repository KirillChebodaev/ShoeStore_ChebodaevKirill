using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ShoeStore_ChebodaevKirill
{
    public partial class MainMenuWindow : Window
    {
        private Users _currentUser;
        private int _roleId;

        public MainMenuWindow(Users user)
        {
            InitializeComponent();
            _currentUser = user;

            if (_currentUser != null)
            {
                TbUserInfo.Text = $"{_currentUser.FullName}";
                _roleId = _currentUser.RoleID;
            }
            else
            {
                TbUserInfo.Text = "Гость";
                _roleId = 0;
            }

            // ИСПРАВЛЕНО: Прячем панель только от Гостя (0) и Клиента (3).
            // Менеджер (2) и Админ (1) видят кнопку "Заказы"
            if (_roleId == 0 || _roleId == 3)
            {
                AdminPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var context = ShoeStore_ChebodaevKirillEntities.GetContext();
            if (_roleId == 1 || _roleId == 2)
            {
                var suppliers = context.Products.Select(p => p.Supplier).Distinct().OrderBy(s => s).ToList();
                suppliers.Insert(0, "Все поставщики");
                CmbSupplier.ItemsSource = suppliers;
                CmbSupplier.SelectedIndex = 0;
            }
            Filtr();
        }

        private void Filtr()
        {
            var context = ShoeStore_ChebodaevKirillEntities.GetContext();
            var searchList = context.Products.Include("Categories").Include("Manufactures").ToList();

            if (CmbSupplier.SelectedIndex > 0)
            {
                string selectedSupplier = CmbSupplier.SelectedItem.ToString();
                searchList = searchList.Where(x => x.Supplier == selectedSupplier).ToList();
            }

            if (!string.IsNullOrWhiteSpace(TbSearch.Text))
            {
                string searchText = TbSearch.Text.ToLower();
                searchList = searchList.Where(x =>
                    (x.ProductName != null && x.ProductName.ToLower().Contains(searchText)) ||
                    (x.Description != null && x.Description.ToLower().Contains(searchText)) ||
                    (x.Supplier != null && x.Supplier.ToLower().Contains(searchText)) ||
                    (x.Manufactures.ManufacturerName != null && x.Manufactures.ManufacturerName.ToLower().Contains(searchText))
                ).ToList();
            }

            if (_roleId == 1 || _roleId == 2)
            {
                if (BtnSortAsc.Tag?.ToString() == "Active")
                    searchList = searchList.OrderBy(x => x.QuantityInStock).ToList();
                else if (BtnSortDesc.Tag?.ToString() == "Active")
                    searchList = searchList.OrderByDescending(x => x.QuantityInStock).ToList();
            }
            ListTovar.ItemsSource = searchList;
        }

        private void TbSearch_TextChanged(object sender, TextChangedEventArgs e) { Filtr(); }
        private void CmbSupplier_SelectionChanged(object sender, SelectionChangedEventArgs e) { Filtr(); }

        private void BtnSortAsc_Click(object sender, RoutedEventArgs e)
        {
            BtnSortAsc.Tag = "Active";
            BtnSortDesc.Tag = "Inactive";
            Filtr();
        }
        private void BtnSortDesc_Click(object sender, RoutedEventArgs e)
        {
            BtnSortDesc.Tag = "Active";
            BtnSortAsc.Tag = "Inactive";
            Filtr();
        }
        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            TbSearch.Text = string.Empty;
            CmbSupplier.SelectedIndex = 0;
            BtnSortAsc.Tag = "Inactive";
            BtnSortDesc.Tag = "Inactive";
            Filtr();
        }
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            AuthorizationWindow authWindow = new AuthorizationWindow();
            authWindow.Show();
            this.Close();
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            OrdersWindow w = new OrdersWindow(_roleId);
            w.ShowDialog();
            Filtr();
        }

        private void BtnAddTovar_Click(object sender, RoutedEventArgs e)
        {
            // Проверка на всякий случай, чтобы менеджер не открыл окно вручную
            if (_roleId != 1) return;
            AddEditTovarWindow w = new AddEditTovarWindow(null);
            if (w.ShowDialog() == true) Filtr();
        }

        private void ListTovar_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_roleId == 1)
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is AddEditTovarWindow) return;
                }
                var selectedItem = ListTovar.SelectedItem as Products;
                if (selectedItem != null)
                {
                    AddEditTovarWindow w = new AddEditTovarWindow(selectedItem);
                    if (w.ShowDialog() == true) Filtr();
                }
            }
        }

        private void BtnDeleteTovar_Click(object sender, RoutedEventArgs e)
        {
            if (_roleId != 1) return;
            var selectedItem = ListTovar.SelectedItem as Products;
            if (selectedItem == null)
            {
                MessageBox.Show("Выберите товар для удаления.");
                return;
            }
            var context = ShoeStore_ChebodaevKirillEntities.GetContext();
            var isOrdered = context.OrderItems.Any(o => o.ProductID == selectedItem.ProductID);
            if (isOrdered)
            {
                MessageBox.Show("Товар присутствует в заказе, удалить нельзя!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (MessageBox.Show("Вы точно хотите удалить этот товар?", "Внимание", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    var itemToDelete = context.Products.Find(selectedItem.ProductID);
                    if (itemToDelete != null)
                    {
                        context.Products.Remove(itemToDelete);
                        context.SaveChanges();
                        MessageBox.Show("Данные удалены!");
                        Filtr();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
