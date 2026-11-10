using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ShoeStore_ChebodaevKirill
{
    public partial class AddEditOrderWindow : Window
    {
        private Orders _currentOrder = new Orders();

        public AddEditOrderWindow(Orders selOrder)
        {
            InitializeComponent();
            var context = ShoeStore_ChebodaevKirillEntities.GetContext();
            CmbStatus.ItemsSource = context.OrderStatus.ToList();
            CmbPickupPoint.ItemsSource = context.PickupPoints.ToList();

            if (selOrder != null) _currentOrder = selOrder;
            DataContext = _currentOrder;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TbOrderNumber.Text) || CmbStatus.SelectedItem == null || CmbPickupPoint.SelectedItem == null)
            {
                MessageBox.Show("Заполните обязательные поля (Артикул, Статус, Пункт выдачи).");
                return;
            }
            int orderNum;
            if (!int.TryParse(TbOrderNumber.Text, out orderNum))
            {
                MessageBox.Show("Артикул должен быть числом.");
                return;
            }
            if (DpOrderDate.SelectedDate == null || DpDeliveryDate.SelectedDate == null)
            {
                MessageBox.Show("Выберите даты заказа и доставки.");
                return;
            }

            _currentOrder.OrderNumber = orderNum;
            _currentOrder.StatusID = (int)CmbStatus.SelectedValue;
            _currentOrder.PickupPointID = (int)CmbPickupPoint.SelectedValue;
            _currentOrder.OrderDate = DpOrderDate.SelectedDate.Value;
            _currentOrder.DeliveryDate = DpDeliveryDate.SelectedDate.Value;

            var context = ShoeStore_ChebodaevKirillEntities.GetContext();

            try
            {
                if (_currentOrder.OrderID == 0)
                {
                    _currentOrder.PickupCode = new Random().Next(100, 999).ToString();
                    _currentOrder.UserID = 1; 
                    context.Orders.Add(_currentOrder);
                }
                else
                {
                    var existingOrder = context.Orders.Find(_currentOrder.OrderID);
                    if (existingOrder != null)
                        context.Entry(existingOrder).CurrentValues.SetValues(_currentOrder);
                }
                context.SaveChanges();
                MessageBox.Show("Заказ успешно сохранен!");
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void TbOrderNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0)) e.Handled = true;
        }
    }
}