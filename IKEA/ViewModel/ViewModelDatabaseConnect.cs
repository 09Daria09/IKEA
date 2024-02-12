using IKEA.Commands;
using IKEA.Model;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace IKEA.ViewModel
{
    internal class ViewModelDatabaseConnect : INotifyPropertyChanged
    {
        private Storage _windowStorage;

        private DatabaseConnectionInfo _connectionInfo = new DatabaseConnectionInfo();
        string connectionString = null;

        public string UserName
        {
            get { return _connectionInfo.UserName; }
            set
            {
                if (_connectionInfo.UserName != value)
                {
                    _connectionInfo.UserName = value;
                    OnPropertyChanged(nameof(UserName));
                    (ConnectCommand as DelegateCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string DatabaseName
        {
            get { return _connectionInfo.DatabaseName; }
            set
            {
                if (_connectionInfo.DatabaseName != value)
                {
                    _connectionInfo.DatabaseName = value;
                    OnPropertyChanged(nameof(DatabaseName));
                    (ConnectCommand as DelegateCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand ConnectCommand { get; private set; }

        public ViewModelDatabaseConnect()
        {
            ConnectCommand = new DelegateCommand(
                execute: async (object parameter) =>
                {
                    ConnectToDatabase(); 
                },
                canExecute: (object parameter) =>
                {
                    return !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(DatabaseName);
                });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task ConnectToDatabase() 
        {
            try
            {
                connectionString = _connectionInfo.GetConnectionString();
                using (var connection = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    MessageBox.Show("Успешное подключение к базе данных!", "Подключение", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                    _windowStorage = new Storage(connectionString);
                _windowStorage.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



    }
}
