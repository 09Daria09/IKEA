using IKEA.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace IKEA.ViewModel
{
    class ViewModelAddProductType : INotifyPropertyChanged
    {
        private string connectionString;
        private string _typeName;
        public string TypeName
        {
            get => _typeName;
            set
            {
                _typeName = value;
                OnPropertyChanged(nameof(TypeName));
            }
        }

        public ICommand AddTypeCommand { get; private set; }

        public ViewModelAddProductType(string connect)
        {
            connectionString = connect;
            AddTypeCommand = new DelegateCommand(AddTypeExecute, CanAddTypeExecute);
        }

        private bool CanAddTypeExecute(object obj)
        {
            return !string.IsNullOrWhiteSpace(TypeName);
        }

        private void AddTypeExecute(object obj)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var query = "INSERT INTO ProductTypes (Type) VALUES (@Type)";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Type", TypeName);
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Тип товара успешно добавлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Ошибка при добавлении типа товара.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при работе с базой данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            TypeName = "";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

