using IKEA.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace IKEA.ViewModel
{
    class ViewModelAddSupplier : INotifyPropertyChanged
    {
        private string connectionString;
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        private string _Address;
        public string Address
        {
            get => _Address;
            set
            {
                _Address = value;
                OnPropertyChanged(nameof(Address));
            }
        }
        private string _Phone;
        public string Phone
        {
            get => _Phone;
            set
            {
                _Phone = value;
                OnPropertyChanged(nameof(Phone));
            }
        }
        public ICommand AddSupplierCommand { get; private set; }

        public ViewModelAddSupplier(string connect) 
        {
            connectionString = connect;
            AddSupplierCommand = new DelegateCommand(AddTypeExecute, CanAddTypeExecute);
        }

        private bool CanAddTypeExecute(object obj)
        {
            return !string.IsNullOrWhiteSpace(Name)&& !string.IsNullOrWhiteSpace(Phone)&& !string.IsNullOrWhiteSpace(Address);
        }

        private void AddTypeExecute(object obj)
        {
            if (CanAddTypeExecute(obj))
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        var query = "INSERT INTO Suppliers (Name, Address, Phone) VALUES (@Name, @Address, @Phone);";

                        using (var command = new SqlCommand(query, connection))
                        {
                            // Добавляем параметры для предотвращения SQL инъекций
                            command.Parameters.AddWithValue("@Name", Name);
                            command.Parameters.AddWithValue("@Address", Address);
                            command.Parameters.AddWithValue("@Phone", Phone);

                            var result = command.ExecuteNonQuery();
                            // Проверка успешности выполнения команды
                            if (result > 0)
                            {
                                MessageBox.Show("Поставщик успешно добавлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                                // Очистка полей после успешного добавления
                                Name = string.Empty;
                                Address = string.Empty;
                                Phone = string.Empty;
                            }
                            else
                            {
                                MessageBox.Show("Не удалось добавить поставщика.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при добавлении поставщика: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

