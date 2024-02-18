using IKEA.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace IKEA.ViewModel
{
    public class ProductType
    {
        public int ProductTypeID { get; set; }
        public string Type { get; set; }
    }

    public class Supplier
    {
        public int SupplierID { get; set; }
        public string Name { get; set; }
    }

    class AdditionViewModel : INotifyPropertyChanged
    { 
         public ICommand AddProductCommand { get; private set; }

        public AdditionViewModel(string connectionInfo)
        {
            AddProductCommand = new DelegateCommand(AddProduct, (object parameter) => true);
            connectionString = connectionInfo;
            LoadProductTypes();
            LoadSuppliers();
        }

        private void AddProduct(object obj)
        {
            if (string.IsNullOrEmpty(ProductName) || SelectedProductType == null || SelectedSupplier == null || ProductCost <= 0)
            {
                MessageBox.Show("Пожалуйста, заполните все поля корректно.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var query = "INSERT INTO Products (Name, ProductTypeID, SupplierID, Cost) VALUES (@Name, @ProductTypeID, @SupplierID, @Cost)";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Name", ProductName);
                        command.Parameters.AddWithValue("@ProductTypeID", SelectedProductType.ProductTypeID);
                        command.Parameters.AddWithValue("@SupplierID", SelectedSupplier.SupplierID);
                        command.Parameters.AddWithValue("@Cost", ProductCost);

                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Товар успешно добавлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении товара: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private decimal _productCost;
        public decimal ProductCost
        {
            get => _productCost;
            set
            {
                _productCost = value;
                OnPropertyChanged(nameof(ProductCost));
            }
        }
        private string _productName;
        public string ProductName
        {
            get => _productName;
            set
            {
                _productName = value;
                OnPropertyChanged(nameof(ProductName));
            }
        }

        private string connectionString;
        public ObservableCollection<ProductType> ProductTypes { get; set; } = new ObservableCollection<ProductType>();
        public ObservableCollection<Supplier> Suppliers { get; set; } = new ObservableCollection<Supplier>();

        private ProductType _selectedProductType;
        public ProductType SelectedProductType
        {
            get => _selectedProductType;
            set
            {
                _selectedProductType = value;
                OnPropertyChanged(nameof(SelectedProductType));
            }
        }

        private Supplier _selectedSupplier;
        public Supplier SelectedSupplier
        {
            get => _selectedSupplier;
            set
            {
                _selectedSupplier = value;
                OnPropertyChanged(nameof(SelectedSupplier));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private void LoadProductTypes()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var query = "SELECT ProductTypeID, Type FROM ProductTypes;";
                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var productType = new ProductType
                                {
                                    ProductTypeID = reader.GetInt32(0),
                                    Type = reader.GetString(1)
                                };
                                ProductTypes.Add(productType);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка загрузки типов товаров: " + ex.Message);
                }
            }
        }

        private void LoadSuppliers()
        {
            Suppliers = new ObservableCollection<Supplier>();

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var query = "SELECT SupplierID, Name FROM Suppliers;";
                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var supplier = new Supplier
                                {
                                    SupplierID = reader.GetInt32(0),
                                    Name = reader.GetString(1)
                                };
                                Suppliers.Add(supplier);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка загрузки поставщиков: " + ex.Message);
                }
            }
        }
    }
}
