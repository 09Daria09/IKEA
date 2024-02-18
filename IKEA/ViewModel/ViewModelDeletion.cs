using IKEA.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Diagnostics;
using System.Windows.Controls;

namespace IKEA.ViewModel
{
    public class ComboBoxItem
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    internal class ViewModelDeletion : INotifyPropertyChanged
    {
        private string connectionString;
        private object selectedObject;
        int index;
        public ICommand DeleteCommand { get; private set; }

        public object SelectedObject
        {
            get => selectedObject;
            set
            {
                selectedObject = value;
                OnPropertyChanged(nameof(SelectedObject));
            }
        }
        public ViewModelDeletion(string connectionInfo, int i)
        {
            connectionString = connectionInfo;
            LoadData(i);
            DeleteCommand = new DelegateCommand(Delete, (object parameter) => true);
            index = i;
        }

        private void Delete(object parameter)
        {
            if (parameter is ComboBoxItem selectedItem)
            {
                int objectId = selectedItem.ID;

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand();
                    command.Connection = connection;

                    SqlTransaction transaction = connection.BeginTransaction();
                    command.Transaction = transaction;

                    try
                    {
                        switch (index)
                        {
                            case 1:
                                command.CommandText = "DELETE FROM Deliveries WHERE ProductID = @ID; " +
                                                      "DELETE FROM Products WHERE ProductID = @ID;";
                                break;
                            case 2:
                                command.CommandText = "DELETE FROM Products WHERE ProductTypeID = @ID; " +
                                                      "DELETE FROM ProductTypes WHERE ProductTypeID = @ID;";
                                break;
                            case 3:
                                command.CommandText = "UPDATE Products SET SupplierID = NULL WHERE SupplierID = @ID; " +
                          "DELETE FROM Suppliers WHERE SupplierID = @ID;";
                                break;
                        }


                        command.Parameters.AddWithValue("@ID", objectId);
                        command.ExecuteNonQuery();
                        transaction.Commit();
                        MessageBox.Show("Удаление прошло успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                        LoadData(index);
                        SelectedObject = null;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Ошибка при удалении данных: {ex.Message}");
                        transaction.Rollback();
                    }
                }
            }
            else
            {
                Debug.WriteLine("Предоставленный параметр имеет неверный формат или тип.");
            }
        }





        private ObservableCollection<ComboBoxItem> deletableObjects = new ObservableCollection<ComboBoxItem>();

        public ObservableCollection<ComboBoxItem> DeletableObjects
        {
            get => deletableObjects;
            set
            {
                deletableObjects = value;
                OnPropertyChanged(nameof(DeletableObjects));
            }
        }

        private void LoadData(int index)
        {
            DeletableObjects.Clear();

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand();
                    command.Connection = connection;

                    switch (index)
                    {
                        case 1:
                            command.CommandText = "SELECT ProductID AS ID, Name FROM Products";
                            break;
                        case 2:
                            command.CommandText = "SELECT ProductTypeID AS ID, Type AS Name FROM ProductTypes";
                            break;
                        case 3:
                            command.CommandText = "SELECT SupplierID AS ID, Name FROM Suppliers";
                            break;
                    }

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DeletableObjects.Add(new ComboBoxItem
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                Name = reader["Name"].ToString()
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка при загрузке данных: {ex.Message}");
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
