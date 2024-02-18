using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using IKEA.Commands;
using System.Data.SqlClient;
using IKEA.ViewModel;
using System.Windows;


namespace IKEA.ViewModel
{
    public class ProductViewModel
    {
        public int ProductID { get; set; }
        public string Name { get; set; }
        public int ProductType { get; set; }
        public int Supplier{ get; set; } 
        public decimal Cost { get; set; }
        public ProductViewModel DeepCopy()
        {
            return new ProductViewModel
            {
                ProductID = this.ProductID,
                Name = this.Name,
                ProductType = this.ProductType,
                Supplier = this.Supplier,
                Cost = this.Cost
            };
        }
    }

    internal class ViewModelUpDate : INotifyPropertyChanged
    {
        private string connectionString;

        private ProductViewModel originalSelectedProduct;

        private string productName;
        public string ProductName
        {
            get => productName;
            set
            {
                if (productName != value)
                {
                    productName = value;
                    OnPropertyChanged(nameof(ProductName));
                }
            }
        }

        private decimal productCost;
        public decimal ProductCost
        {
            get => productCost;
            set
            {
                if (productCost != value)
                {
                    productCost = value;
                    OnPropertyChanged(nameof(ProductCost));
                }
            }
        }


        private ProductViewModel selectedProductForUpdate;
        public ProductViewModel SelectedProductForUpdate
        {
            get => selectedProductForUpdate;
            set
            {
                if (selectedProductForUpdate != value)
                {
                    selectedProductForUpdate = value;
                    OnPropertyChanged(nameof(SelectedProductForUpdate));
                    ProductName = selectedProductForUpdate?.Name;
                    SelectedProductType = ProductTypes.FirstOrDefault(pt => pt.ProductTypeID == selectedProductForUpdate.ProductType);
                    SelectedSupplier = Suppliers.FirstOrDefault(s => s.SupplierID == selectedProductForUpdate.Supplier);
                    ProductCost = selectedProductForUpdate?.Cost ?? 0m;
                    originalSelectedProduct = selectedProductForUpdate?.DeepCopy();
                }
            }
        }

        private ObservableCollection<ProductType> productTypes = new ObservableCollection<ProductType>();
        public ObservableCollection<ProductType> ProductTypes
        {
            get => productTypes;
            set
            {
                productTypes = value;
                OnPropertyChanged(nameof(ProductTypes));
            }
        }

        public ObservableCollection<Supplier> suppliers = new ObservableCollection<Supplier>();
        public ObservableCollection<Supplier> Suppliers 
        {
            get => suppliers;
            set
            {
                suppliers = value;
                OnPropertyChanged(nameof(Suppliers));
            }
        }

        private ProductType selectedProductType;
        public ProductType SelectedProductType
        {
            get => selectedProductType;
            set
            {
                if (selectedProductType != value)
                {
                    selectedProductType = value;
                    OnPropertyChanged(nameof(SelectedProductType));

                    if (SelectedProductForUpdate != null && selectedProductType != null)
                    {
                        SelectedProductForUpdate.ProductType = selectedProductType.ProductTypeID;
                        UpdateProductTypeInDatabase(SelectedProductForUpdate.ProductID, selectedProductType.ProductTypeID);
                    }
                }
            }
        }

        private Supplier selectedSupplier;
        public Supplier SelectedSupplier
        {
            get => selectedSupplier;
            set
            {
                if (selectedSupplier != value)
                {
                    selectedSupplier = value;
                    OnPropertyChanged(nameof(SelectedSupplier));

                    if (SelectedProductForUpdate != null && selectedSupplier != null)
                    {
                        SelectedProductForUpdate.Supplier = selectedSupplier.SupplierID;
                        UpdateProductSupplierInDatabase(SelectedProductForUpdate.ProductID, selectedSupplier.SupplierID);
                    }
                }
            }
        }
        private void UpdateProductSupplierInDatabase(int productId, int supplierId)
        {
            string query = "UPDATE Products SET SupplierID = @SupplierID WHERE ProductID = @ProductID";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@SupplierID", supplierId);
                command.Parameters.AddWithValue("@ProductID", productId);

                try
                {
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        private void UpdateProductTypeInDatabase(int productId, int productTypeId)
        {
            string query = "UPDATE Products SET ProductTypeID = @ProductTypeID WHERE ProductID = @ProductID";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ProductTypeID", productTypeId);
                command.Parameters.AddWithValue("@ProductID", productId);

                try
                {
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private ObservableCollection<ProductViewModel> products = new ObservableCollection<ProductViewModel>();
        public ObservableCollection<ProductViewModel> Products
        {
            get => products;
            set
            {
                products = value;
                OnPropertyChanged(nameof(Products));
            }
        }

        public ICommand UpdateProductCommand { get; private set; }

        public ViewModelUpDate(string connectionString)
        {
            this.connectionString = connectionString;
            LoadData();
            UpdateProductCommand = new DelegateCommand(UpdateProduct, CanUpdateProduct);
        }

        private bool CanUpdateProduct(object obj)
        {
            if (SelectedProductForUpdate == null || originalSelectedProduct == null) return false;

            return SelectedProductForUpdate.ProductID == originalSelectedProduct.ProductID &&
                   (ProductName != originalSelectedProduct.Name ||
                    SelectedProductForUpdate.ProductType != originalSelectedProduct.ProductType ||
                    SelectedProductForUpdate.Supplier != originalSelectedProduct.Supplier ||
                    ProductCost != originalSelectedProduct.Cost);
        }


        private void UpdateProduct(object obj)
        {
            string query = @"
        UPDATE Products
        SET Name = @Name, ProductTypeID = @ProductTypeID, SupplierID = @SupplierID, Cost = @Cost
        WHERE ProductID = @ProductID";

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Name", ProductName);
                command.Parameters.AddWithValue("@ProductTypeID", SelectedProductForUpdate.ProductType);
                command.Parameters.AddWithValue("@SupplierID", SelectedProductForUpdate.Supplier);
                command.Parameters.AddWithValue("@Cost", ProductCost);
                command.Parameters.AddWithValue("@ProductID", SelectedProductForUpdate.ProductID);

                try
                {
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Продукт успешно обновлен!");
                    }
                    else
                    {
                        MessageBox.Show("Обновление не выполнено. Проверьте данные и попробуйте снова.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления продукта: {ex.Message}");
                }
            }
            LoadData();
        }

        private void LoadData()
        {
            LoadProductTypes();
            LoadSuppliers();

            Products.Clear();
            ProductCost = 0;
            string query = "SELECT ProductID, Name, ProductTypeID, SupplierID, Cost FROM Products"; 
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(query, connection);
                try
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Products.Add(new ProductViewModel
                            {
                                ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                                Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? "" : reader.GetString(reader.GetOrdinal("Name")),
                                ProductType = reader.IsDBNull(reader.GetOrdinal("ProductTypeID")) ? 0 : reader.GetInt32(reader.GetOrdinal("ProductTypeID")),
                                Supplier = reader.IsDBNull(reader.GetOrdinal("SupplierID")) ? 0 : reader.GetInt32(reader.GetOrdinal("SupplierID")), 
                                Cost = reader.IsDBNull(reader.GetOrdinal("Cost")) ? 0m : reader.GetDecimal(reader.GetOrdinal("Cost")),
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        private void LoadProductTypes()
        {
            ProductTypes.Clear();
            string query = "SELECT ProductTypeID, Type FROM ProductTypes";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(query, connection);
                try
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ProductTypes.Add(new ProductType
                            {
                                ProductTypeID = reader.GetInt32(reader.GetOrdinal("ProductTypeID")),
                                Type = reader.GetString(reader.GetOrdinal("Type")),
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        private void LoadSuppliers()
        {
            Suppliers.Clear();
            string query = "SELECT SupplierID, Name FROM Suppliers";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(query, connection);
                try
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Suppliers.Add(new Supplier
                            {
                                SupplierID = reader.GetInt32(reader.GetOrdinal("SupplierID")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
