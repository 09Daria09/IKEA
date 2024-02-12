using IKEA.Commands;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace IKEA.ViewModel
{
    internal class ViewModelStorage : INotifyPropertyChanged
    {
        private DataTable _productsData;
        string connectionString;
        public ObservableCollection<MenuItem> MenuItems { get; set; }
        public ObservableCollection<MenuItem> MenuItems2 { get; set; }
        public ObservableCollection<MenuItem> MenuItems3 { get; set; }

        public ICommand ShowAllProductsCommand { get; private set; }
        public ICommand ShowAllProductTypesCommand { get; private set; }
        public ICommand ShowAllSuppliersCommand { get; private set; }
        public ICommand ShowProductWithMaxQuantityCommand { get; private set; }
        public ICommand ShowProductWithMinQuantityCommand { get; private set; }
        public ICommand ShowProductWithMinCostCommand { get; private set; }
        public ICommand ShowProductWithMaxCostCommand { get; private set; }
        public ICommand ShowOldestProductCommand { get; private set; }
        public ICommand ShowProductsBySupplierCommand { get; private set; }

        public ICommand ShowProductsByCategoryCommand { get; private set; }
        public ICommand ShowAverageQuantityByTypeCommand { get; private set; }

        public DataTable ProductsData
        {
            get { return _productsData; }
            set
            {
                if (_productsData != value)
                {
                    _productsData = value;
                    OnPropertyChanged(nameof(ProductsData));
                }
            }
        }

        public ViewModelStorage(string _connectionInfo)
        {
            connectionString = _connectionInfo;
            MenuItems = new ObservableCollection<MenuItem>();
            MenuItems2 = new ObservableCollection<MenuItem>();
            MenuItems3 = new ObservableCollection<MenuItem>();
            InitializeMenuItems();
            InitializeMenuItems2();
            InitializeSupplierMenuItems();

            ShowAllProductsCommand = new DelegateCommand(ShowAllProducts, (object parameter) => true);
            ShowAllProductTypesCommand = new DelegateCommand(ShowAllProductTypes, (object parameter) => true);
            ShowAllSuppliersCommand = new DelegateCommand(ShowAllSuppliers, (object parameter) => true);
            ShowProductWithMaxQuantityCommand = new DelegateCommand(ShowProductWithMaxQuantity, (object parameter) => true);
            ShowProductWithMinQuantityCommand = new DelegateCommand(ShowProductWithMinQuantity, (object parameter) => true);
            ShowProductWithMinCostCommand = new DelegateCommand(ShowProductWithMinCost, (object parameter) => true);
            ShowProductWithMaxCostCommand = new DelegateCommand(ShowProductWithMaxCost, (object parameter) => true);
            ShowOldestProductCommand = new DelegateCommand(ShowOldestProduct, (object parameter) => true);
            ShowProductsByCategoryCommand = new DelegateCommand(ExecuteShowProductsByCategory, (object parameter) => true);
            ShowProductsBySupplierCommand = new DelegateCommand(ExecuteShowProductsBySupplier, (object parameter) => true);
            ShowAverageQuantityByTypeCommand = new DelegateCommand(ShowAverageQuantityByType, (object parameter) => true);
        }

        private void ShowAverageQuantityByType(object obj)
        {
            string selectedType = obj.ToString(); 
            DataTable dataTable = new DataTable();
            string query = @"
SELECT 
    pt.Type AS 'Тип продукта', 
    ROUND(AVG(CAST(d.Quantity AS FLOAT)), 2) AS 'Среднее количество'
FROM Deliveries d
JOIN Products p ON d.ProductID = p.ProductID
JOIN ProductTypes pt ON p.ProductTypeID = pt.ProductTypeID
WHERE pt.Type = @SelectedType
GROUP BY pt.Type;";
            ExecuteQueryAndFillDataTable(query, dataTable, selectedType, "@SelectedType"); 
            ProductsData = dataTable;
        }
        public async void InitializeMenuItems2()
        {
            var productTypes = await GetCategoriesAsync();

            MenuItems3.Clear();

            foreach (var type in productTypes)
            {
                var menuItem = new MenuItem
                {
                    Header = type,
                    Command = ShowAverageQuantityByTypeCommand,
                    CommandParameter = type
                };

                MenuItems3.Add(menuItem);
            }
        }

        public async void InitializeSupplierMenuItems() 
        {
            var suppliers = await GetSuppliersAsync();

            MenuItems2.Clear();

            foreach (var supplier in suppliers)
            {
                var menuItem = new MenuItem
                {
                    Header = supplier,
                    Command = ShowProductsBySupplierCommand, 
                    CommandParameter = supplier
                };

                MenuItems2.Add(menuItem);
            }
        }
        private void ExecuteShowProductsBySupplier(object obj)
        {
            string supplierName = obj.ToString();
            DataTable dataTable = new DataTable();
            string query = @"
SELECT 
    p.ProductID AS 'Идентификатор продукта', 
    p.Name AS 'Название продукта', 
    pt.Type AS 'Тип продукта', 
    s.Name AS 'Название поставщика', 
    p.Cost AS 'Стоимость'
FROM Products p
INNER JOIN ProductTypes pt ON p.ProductTypeID = pt.ProductTypeID
INNER JOIN Suppliers s ON p.SupplierID = s.SupplierID
WHERE s.Name = @SupplierName;"; 

            ExecuteQueryAndFillDataTable(query, dataTable, supplierName, "@SupplierName"); 
            ProductsData = dataTable; 
        }
        private void ExecuteShowProductsByCategory(object obj)
        {
            string productType = obj.ToString();
            DataTable dataTable = new DataTable();
            string query = @"
SELECT 
    p.ProductID AS ""Идентификатор продукта"", 
    p.Name AS ""Название продукта"", 
    pt.Type AS ""Тип продукта"", 
    s.Name AS ""Название поставщика"", 
    p.Cost AS ""Стоимость""
FROM Products p
INNER JOIN ProductTypes pt ON p.ProductTypeID = pt.ProductTypeID
INNER JOIN Suppliers s ON p.SupplierID = s.SupplierID
WHERE pt.Type = @ProductType;"; 

            ExecuteQueryAndFillDataTable(query, dataTable, productType, "@ProductType");
            ProductsData = dataTable;
        }
        private void ExecuteQueryAndFillDataTable(string query, DataTable dataTable, string productType, string scalar)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue(scalar, productType);

                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        adapter.Fill(dataTable);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при попытке получения данных из базы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async void InitializeMenuItems()
        {
            var productTypes = await GetCategoriesAsync();

            MenuItems.Clear();

            foreach (var type in productTypes)
            {
                var menuItem = new MenuItem
                {
                    Header = type,
                    Command = ShowProductsByCategoryCommand, 
                    CommandParameter = type
                };

                MenuItems.Add(menuItem);
            }
        }


        public async Task<List<string>> GetCategoriesAsync()
        {
            var productTypes = new List<string>();
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("SELECT DISTINCT Type FROM ProductTypes", connection);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        productTypes.Add(reader.GetString(0));
                    }
                }
            }
            return productTypes;
        }

        public async Task<List<string>> GetSuppliersAsync()
        {
            var suppliers = new List<string>();
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("SELECT DISTINCT Name FROM Suppliers", connection);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        suppliers.Add(reader.GetString(0));
                    }
                }
            }
            return suppliers;
        }


        private void ShowOldestProduct(object obj)
        {
            DataTable dataTable = new DataTable();
            string query = @"
SELECT TOP 1
    p.ProductID AS ""Идентификатор продукта"",
    p.Name AS ""Название продукта"",
    MIN(d.Date) AS ""Дата поступления""
FROM Products p
JOIN Deliveries d ON p.ProductID = d.ProductID
GROUP BY p.ProductID, p.Name
ORDER BY ""Дата поступления"" ASC;
";
            ExecuteQueryAndFillDataTable(query, dataTable);
            ProductsData = dataTable;
        }

        private void ShowProductWithMaxCost(object obj)
        {
            DataTable dataTable = new DataTable();
            string query = @"
SELECT TOP 1
    ProductID AS ""Идентификатор продукта"",
    Name AS ""Название продукта"",
    Cost AS ""Себестоимость""
FROM Products
ORDER BY Cost DESC;"; 

            ExecuteQueryAndFillDataTable(query, dataTable);
            ProductsData = dataTable;
        }

        private void ShowProductWithMinCost(object obj)
        {
            DataTable dataTable = new DataTable();
            string query = @"
SELECT TOP 1
    ProductID AS ""Идентификатор продукта"",
    Name AS ""Название продукта"",
    Cost AS ""Себестоимость""
FROM Products
ORDER BY Cost ASC;
";
            ExecuteQueryAndFillDataTable(query, dataTable);
            ProductsData = dataTable;
        }

        private void ShowProductWithMinQuantity(object obj)
        {
            DataTable dataTable = new DataTable();
            string query = @"
WITH ProductQuantities AS (
    SELECT 
        p.ProductID, 
        p.Name AS ""Название продукта"", 
        SUM(d.Quantity) AS TotalQuantity
    FROM Products p
    JOIN Deliveries d ON p.ProductID = d.ProductID
    GROUP BY p.ProductID, p.Name
),
MaxQuantityProduct AS (
    SELECT TOP 1
        ProductID,
        ""Название продукта"",
        TotalQuantity
    FROM ProductQuantities
    ORDER BY TotalQuantity ASC
)
SELECT 
    mqp.ProductID AS ""Идентификатор продукта"",
    mqp.""Название продукта"",
    mqp.TotalQuantity AS ""Общее количество""
FROM MaxQuantityProduct mqp;;
";
            ExecuteQueryAndFillDataTable(query, dataTable);
            ProductsData = dataTable;
        }

        private void ShowProductWithMaxQuantity(object obj)
        {
            DataTable dataTable = new DataTable();
            string query = @"
WITH ProductQuantities AS (
    SELECT 
        p.ProductID, 
        p.Name AS ""Название продукта"", 
        SUM(d.Quantity) AS TotalQuantity
    FROM Products p
    JOIN Deliveries d ON p.ProductID = d.ProductID
    GROUP BY p.ProductID, p.Name
),
MaxQuantityProduct AS (
    SELECT TOP 1
        ProductID,
        ""Название продукта"",
        TotalQuantity
    FROM ProductQuantities
    ORDER BY TotalQuantity DESC
)
SELECT 
    mqp.ProductID AS ""Идентификатор продукта"",
    mqp.""Название продукта"",
    mqp.TotalQuantity AS ""Общее количество""
FROM MaxQuantityProduct mqp;
";
            ExecuteQueryAndFillDataTable(query, dataTable);
            ProductsData = dataTable;
        }

        private void ShowAllSuppliers(object obj)
        {
            DataTable dataTable = new DataTable();
            string query = @"
SELECT 
    SupplierID AS ""Идентификатор поставщика"", 
    Name AS ""Название поставщика"", 
    Address AS ""Адрес"", 
    Phone AS ""Телефон""
FROM Suppliers;";
            ExecuteQueryAndFillDataTable(query, dataTable);
            ProductsData = dataTable;
        }

        private void ShowAllProductTypes(object obj)
        {
            DataTable dataTable = new DataTable();
            string query = @"
SELECT DISTINCT
    pt.Type AS ""Тип продукта""
FROM ProductTypes pt;";
            ExecuteQueryAndFillDataTable(query, dataTable);
            ProductsData = dataTable;
        }

        private void ShowAllProducts(object obj)
        {
            DataTable dataTable = new DataTable();
            string query = @"
SELECT 
    p.ProductID AS ""Идентификатор продукта"", 
    p.Name AS ""Название продукта"", 
    pt.Type AS ""Тип продукта"", 
    s.Name AS ""Название поставщика"", 
    p.Cost AS ""Стоимость""
FROM Products p
JOIN ProductTypes pt ON p.ProductTypeID = pt.ProductTypeID
JOIN Suppliers s ON p.SupplierID = s.SupplierID;";
            ExecuteQueryAndFillDataTable(query, dataTable);
            ProductsData = dataTable;
        }

        private void ExecuteQueryAndFillDataTable(string query, DataTable dataTable)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    adapter.Fill(dataTable);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при попытке получения данных из базы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
