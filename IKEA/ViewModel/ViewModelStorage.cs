using IKEA.Commands;
using IKEA.View;
using Microsoft.VisualBasic;
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


        public ICommand ShowSupplierWithMaxProductsCommand { get; private set; }
        public ICommand ShowSupplierWithMinProductsCommand { get; private set; }
        public ICommand ShowProductTypeWithMaxQuantityCommand { get; private set; }
        public ICommand ShowProductTypeWithMinQuantityCommand { get; private set; }
        public ICommand ShowProductsOlderThanNDaysCommand { get; private set; }
        public ICommand AddNewProductCommand { get; private set; }
        public ICommand AddNewProductTypeCommand { get; private set; }
        public ICommand AddNewSupplierCommand { get; private set; }

        public ICommand DeleteProductCommand { get; private set; }
        public ICommand DeleteProductTypeCommand { get; private set; }
        public ICommand DeleteSupplierCommand { get; private set; }

        public ICommand UpdateProductCommand { get; private set; }
        public ICommand UpdateProductTypeCommand { get; private set; }

        public ICommand UpdateSupplierCommand { get; private set; }
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
            ShowSupplierWithMaxProductsCommand = new DelegateCommand(ShowSupplierWithMaxProducts, (object parameter) => true);
            ShowSupplierWithMinProductsCommand = new DelegateCommand(ShowSupplierWithMinProducts, (object parameter) => true);
            ShowProductTypeWithMaxQuantityCommand = new DelegateCommand(ShowProductTypeWithMaxQuantity, (object parameter) => true);
            ShowProductTypeWithMinQuantityCommand = new DelegateCommand(ShowProductTypeWithMinQuantity, (object parameter) => true);
            ShowProductsOlderThanNDaysCommand = new DelegateCommand(ShowProductsOlderThanNDays, (object parameter) => true);
            AddNewProductCommand = new DelegateCommand(AddNewProduct, (object parameter) => true);
            AddNewProductTypeCommand = new DelegateCommand(AddNewProductType, (object parameter) => true);
            AddNewSupplierCommand = new DelegateCommand(AddNewSupplier, (object parameter) => true);
            DeleteProductCommand = new DelegateCommand(OpenDeleteProductDialog, (object parameter) => true);
            DeleteProductTypeCommand = new DelegateCommand(OpenDeleteProductTypeDialog, (object parameter) => true);
            DeleteSupplierCommand = new DelegateCommand(OpenDeleteSupplierDialog, (object parameter) => true);


            UpdateProductCommand = new DelegateCommand(UpdateProduct, (object parameter) => true);
            UpdateProductTypeCommand = new DelegateCommand(UpdateProductType, (object parameter) => true);
            UpdateSupplierCommand = new DelegateCommand(UpdateSupplier, (object parameter) => true);
        }

        private void UpdateSupplier(object obj)
        {
            var UpdateSupplier = new UpdateSupplier(connectionString);
            UpdateSupplier.ShowDialog();
            InitializeSupplierMenuItems();
            ShowAllProducts(obj);
        }

        private void UpdateProductType(object obj)
        {
            var UpdateTypeProduct = new TypeProductUpdate(connectionString);
            UpdateTypeProduct.ShowDialog();
            InitializeMenuItems();
            ShowAllProducts(obj);
        }

        private void UpdateProduct(object obj)
        {
            var UpDateWindow = new ProductUpdate(connectionString);
            UpDateWindow.ShowDialog();
            InitializeMenuItems2();
            ShowAllProducts(obj);
        }

        private void OpenDeleteProductDialog(object obj)
        {
            var DeletionWindow = new Deletion(connectionString, 1);
            DeletionWindow.ShowDialog();
        }

        private void OpenDeleteProductTypeDialog(object obj)
        {
            var DeletionWindow = new Deletion(connectionString, 2);
            DeletionWindow.ShowDialog();
        }

        private void OpenDeleteSupplierDialog(object obj)
        {
            var DeletionWindow = new Deletion(connectionString, 3);
            DeletionWindow.ShowDialog();
        }

        private void AddNewSupplier(object obj)
        {
            var additionWindow = new AddingSupplier(connectionString);
            additionWindow.ShowDialog();
        }

        private void AddNewProductType(object obj)
        {
            var additionWindow = new AddingProductType(connectionString);
            additionWindow.ShowDialog();
        }

        private void AddNewProduct(object obj)
        {
            var additionWindow = new Addition(connectionString);
            additionWindow.ShowDialog();
        }

        private void ShowProductsOlderThanNDays(object obj)
        {
            DataTable dataTable = new DataTable();
            string allProductsQuery = @"
SELECT p.Name AS 'Название товара', d.Date AS 'Дата поставки', DATEDIFF(DAY, d.Date, GETDATE()) AS 'Дней на складе'
FROM Products p
JOIN Deliveries d ON p.ProductID = d.ProductID;";
            ExecuteQueryAndFillDataTable(allProductsQuery, dataTable);
            ProductsData = dataTable;

            string input = Interaction.InputBox("Введите количество дней:", "Ввод дней", "10", -1, -1);
            if (int.TryParse(input, out int days))
            {
                dataTable.Clear();
                string filteredQuery = $@"
SELECT p.Name AS 'Название товара', d.Date AS 'Дата поставки', DATEDIFF(DAY, d.Date, GETDATE()) AS 'Дней на складе'
FROM Products p
JOIN Deliveries d ON p.ProductID = d.ProductID
WHERE DATEDIFF(DAY, d.Date, GETDATE()) = {days};";
                ExecuteQueryAndFillDataTable(filteredQuery, dataTable);
            }
            else
            {
                MessageBox.Show("Некорректный ввод! Пожалуйста, введите число.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowProductTypeWithMinQuantity(object obj)
        {
            DataTable dataTable = new DataTable();
            string query = @"
SELECT TOP 1 
    pt.Type AS 'Тип товара',
    COUNT(p.ProductID) AS 'Количество товаров'
FROM ProductTypes pt
JOIN Products p ON pt.ProductTypeID = p.ProductTypeID
GROUP BY pt.Type
ORDER BY COUNT(p.ProductID) ASC;";

            ExecuteQueryAndFillDataTable(query, dataTable);
            ProductsData = dataTable;
        }

        private void ShowProductTypeWithMaxQuantity(object obj)
        {
            DataTable dataTable = new DataTable();
            string query = @"
SELECT TOP 1 
    pt.Type AS 'Тип товара',
    COUNT(p.ProductID) AS 'Количество товаров'
FROM ProductTypes pt
JOIN Products p ON pt.ProductTypeID = p.ProductTypeID
GROUP BY pt.Type
ORDER BY COUNT(p.ProductID) DESC;";

            ExecuteQueryAndFillDataTable(query, dataTable);
            ProductsData = dataTable;
        }

        private void ShowSupplierWithMinProducts(object obj)
        {
            DataTable dataTable = new DataTable();
            string query = @"
SELECT TOP 1 
    s.SupplierID AS 'Идентификатор поставщика',
    s.Name AS 'Название поставщика',
    COUNT(p.ProductID) AS 'Количество товаров'
FROM Suppliers s
LEFT JOIN Products p ON s.SupplierID = p.SupplierID
GROUP BY s.SupplierID, s.Name
ORDER BY COUNT(p.ProductID) ASC;";

            ExecuteQueryAndFillDataTable(query, dataTable);
            ProductsData = dataTable;
        }

        private void ShowSupplierWithMaxProducts(object obj)
        {
            DataTable dataTable = new DataTable();
            string query = @"
SELECT TOP 1 
    s.SupplierID AS 'Идентификатор поставщика',
    s.Name AS 'Название поставщика',
    COUNT(p.ProductID) AS 'Количество товаров'
FROM Suppliers s
LEFT JOIN Products p ON s.SupplierID = p.SupplierID
GROUP BY s.SupplierID, s.Name
ORDER BY COUNT(p.ProductID) DESC;";

            ExecuteQueryAndFillDataTable(query, dataTable);
            ProductsData = dataTable;
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
