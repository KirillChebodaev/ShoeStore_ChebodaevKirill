using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ShoeStore_ChebodaevKirill
{
    public partial class OrdersWindow : Window
    {
        private int _roleId;

        public OrdersWindow(int roleId)
        {
            InitializeComponent();
            _roleId = roleId;
            if (_roleId != 1)
            {
                BtnAddOrder.Visibility = Visibility.Collapsed;
                BtnDeleteOrder.Visibility = Visibility.Collapsed;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadOrders();
        }

        private void LoadOrders()
        {
            var context = ShoeStore_ChebodaevKirillEntities.GetContext();
            var orders = context.Orders.Include("OrderStatus").Include("PickupPoints").ToList();
            ListOrders.ItemsSource = orders;
        }

        private void BtnAddOrder_Click(object sender, RoutedEventArgs e)
        {
            AddEditOrderWindow w = new AddEditOrderWindow(null);
            if (w.ShowDialog() == true) LoadOrders();
        }

        private void ListOrders_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_roleId == 1)
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is AddEditOrderWindow) return;
                }
                var selectedItem = ListOrders.SelectedItem as Orders;
                if (selectedItem != null)
                {
                    AddEditOrderWindow w = new AddEditOrderWindow(selectedItem);
                    if (w.ShowDialog() == true) LoadOrders();
                }
            }
        }

        private void BtnDeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_roleId != 1) return;
            var selectedItem = ListOrders.SelectedItem as Orders;
            if (selectedItem == null)
            {
                MessageBox.Show("Выберите заказ для удаления.");
                return;
            }
            if (MessageBox.Show("Вы точно хотите удалить этот заказ?", "Внимание", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var context = ShoeStore_ChebodaevKirillEntities.GetContext();
                try
                {
                    var orderItems = context.OrderItems.Where(oi => oi.OrderID == selectedItem.OrderID).ToList();
                    if (orderItems.Any()) context.OrderItems.RemoveRange(orderItems);

                    var orderToDelete = context.Orders.Find(selectedItem.OrderID);
                    if (orderToDelete != null) context.Orders.Remove(orderToDelete);

                    context.SaveChanges();
                    MessageBox.Show("Заказ удален!");
                    LoadOrders();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}");
                }
            }
        }
    }
}
