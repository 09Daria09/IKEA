using IKEA.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace IKEA.ViewModel
{
    internal class ViewModelUpDateTypeProduct : INotifyPropertyChanged
    {

        private string connectionString;
        public ICommand UpdateTypeProductCommand { get; private set; }
        private string newName;
        public string NewName
        {
            get => newName;
            set
            {
                if (newName != value)
                {
                    newName = value;
                    OnPropertyChanged(nameof(NewName));
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
                }
            }
        }

        public ViewModelUpDateTypeProduct(string connectionInfo) 
        {
            connectionString = connectionInfo;
            LoadProductTypes();
            UpdateTypeProductCommand = new DelegateCommand(UpdateTypeProduct, CanUpdateTypeProduct);
        }

        private bool CanUpdateTypeProduct(object obj)
        {
            return SelectedProductType != null && !string.IsNullOrEmpty(NewName) && NewName != SelectedProductType.Type;
        }

        private void UpdateTypeProduct(object obj)
        {
            if (SelectedProductType == null) return;

            string query = "UPDATE ProductTypes SET Type = @NewName WHERE ProductTypeID = @ProductTypeID";

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@NewName", NewName);
                command.Parameters.AddWithValue("@ProductTypeID", SelectedProductType.ProductTypeID);

                try
                {
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Тип продукта успешно обновлен.");
                        LoadProductTypes(); 
                    }
                    else
                    {
                        MessageBox.Show("Обновление не выполнено.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления типа продукта: {ex.Message}");
                }
            }

            NewName = "";
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

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
