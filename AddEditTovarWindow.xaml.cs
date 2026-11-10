using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Input;

namespace ShoeStore_ChebodaevKirill
{
    public partial class AddEditTovarWindow : Window
    {
        private Products _currentTovar = new Products();
        private string _photoPath;
        public AddEditTovarWindow(Products selTovar)
        {
            InitializeComponent();
            var context = ShoeStore_ChebodaevKirillEntities.GetContext();
            CmbCategory.ItemsSource = context.Categories.ToList();
            CmbManufacturer.ItemsSource = context.Manufactures.ToList();
            if (selTovar != null)
            {
                _currentTovar = selTovar;
                _photoPath = _currentTovar.Photo;
            }
            DataContext = _currentTovar;
            if (!string.IsNullOrEmpty(_currentTovar.Photo))
            {
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", _currentTovar.Photo);
                if (File.Exists(fullPath))
                {
                    ImgPhoto.Source = new BitmapImage(new Uri(fullPath));
                    TbPhotoName.Text = _currentTovar.Photo;
                }
            }
        }
        private void SelectImageBtn_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".png";
            dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string targetDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
                    if (!Directory.Exists(targetDirectory)) Directory.CreateDirectory(targetDirectory);
                    string oldPhotoPath = Path.Combine(targetDirectory, _currentTovar.Photo);
                    if (File.Exists(oldPhotoPath) && _currentTovar.Photo != Path.GetFileName(dialog.FileName))
                    {
                        File.Delete(oldPhotoPath);
                    }
                    _photoPath = Path.GetFileName(dialog.FileName);
                    string newPath = Path.Combine(targetDirectory, _photoPath);
                    File.Copy(dialog.FileName, newPath, overwrite: true);
                    _currentTovar.Photo = _photoPath;
                    ImgPhoto.Source = new BitmapImage(new Uri(newPath));
                    TbPhotoName.Text = _photoPath;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении изображения: {ex.Message}");
                }
            }
        }
        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TbName.Text) || CmbCategory.SelectedItem == null || CmbManufacturer.SelectedItem == null)
            {
                MessageBox.Show("Заполните обязательные поля (Наименование, Категория, Производитель).");
                return;
            }
            decimal price;
            if (!decimal.TryParse(TbPrice.Text, out price) || price < 0)
            {
                MessageBox.Show("Цена должна быть положительным числом.");
                return;
            }
            int quantity;
            if (!int.TryParse(TbQuantity.Text, out quantity) || quantity < 0)
            {
                MessageBox.Show("Количество не может быть отрицательным.");
                return;
            }
            _currentTovar.ProductName = TbName.Text;
            _currentTovar.CategoryID = (int)CmbCategory.SelectedValue;
            _currentTovar.Description = TbDescription.Text;
            _currentTovar.ManufacturerID = (int)CmbManufacturer.SelectedValue;
            _currentTovar.Supplier = TbSupplier.Text;
            _currentTovar.Price = price;
            _currentTovar.Unit = TbUnit.Text;
            _currentTovar.QuantityInStock = quantity;
            int discount;
            if (int.TryParse(TbDiscount.Text, out discount)) _currentTovar.Discount = discount;
            var context = ShoeStore_ChebodaevKirillEntities.GetContext();
            try
            {
                if (string.IsNullOrEmpty(_currentTovar.ProductID))
                {
                    _currentTovar.ProductID = "P" + (context.Products.Count() + 1).ToString("D3");
                    context.Products.Add(_currentTovar);
                }
                else
                {
                    var existingTovar = context.Products.Find(_currentTovar.ProductID);
                    if (existingTovar != null)
                    {
                        context.Entry(existingTovar).CurrentValues.SetValues(_currentTovar);
                    }
                }
                context.SaveChanges();
                MessageBox.Show("Данные успешно сохранены!");
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }
        private void TbPrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0) && e.Text != ",") e.Handled = true;
        }
        private void TbQuantity_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0)) e.Handled = true;
        }
        private void TbDiscount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0)) e.Handled = true;
        }
    }
}
