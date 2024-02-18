using IKEA.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace IKEA.ViewModel
{
    public class SupplierFull
    {
        public int SupplierID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public SupplierFull DeepCopy()
        {
            return new SupplierFull
            {
                SupplierID = this.SupplierID,
                Name = this.Name,
                Address = this.Address,
                Phone = this.Phone
            };
        }
    }

    internal class ViewModelUpdateSupplier : INotifyPropertyChanged
    {
        private string connectionString;

        private SupplierFull originalSelectedSupplier; 
        public ICommand UpdateSupplierCommand { get; private set; }

        private SupplierFull selectedSupplier;
        public SupplierFull SelectedSupplier 
        {
            get => selectedSupplier;
            set
            {
                if (selectedSupplier != value)
                {
                    selectedSupplier = value;
                    OnPropertyChanged(nameof(SelectedSupplier));
                    Name = selectedSupplier?.Name;
                    Address = selectedSupplier?.Address;
                    Phone = selectedSupplier?.Phone;
                    originalSelectedSupplier = selectedSupplier?.DeepCopy();
                }
            }
        }

        private string name;
        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        private string address;
        public string Address
        {
            get => address;
            set
            {
                if (address != value)
                {
                    address = value;
                    OnPropertyChanged(nameof(Address));
                }
            }
        }

        private string phone;
        public string Phone
        {
            get => phone;
            set
            {
                if (phone != value)
                {
                    phone = value;
                    OnPropertyChanged(nameof(Phone));
                }
            }
        }

        public ObservableCollection<SupplierFull> suppliers = new ObservableCollection<SupplierFull>();
        public ObservableCollection<SupplierFull> Suppliers
        {
            get => suppliers;
            set
            {
                suppliers = value;
                OnPropertyChanged(nameof(Suppliers));
            }
        }

        public ViewModelUpdateSupplier(string connectionString) 
        {
            this.connectionString = connectionString;
            LoadSuppliers();
            UpdateSupplierCommand = new DelegateCommand(UpdateSupplier, CanUpdateSupplier);
        }

        private bool CanUpdateSupplier(object obj)
        {
            return selectedSupplier != null &&
            (Name != originalSelectedSupplier.Name ||
             Address != originalSelectedSupplier.Address ||
             Phone != originalSelectedSupplier.Phone);
        }

        private void UpdateSupplier(object obj)
        {
            string query = "UPDATE Suppliers SET Name = @Name, Address = @Address, Phone = @Phone WHERE SupplierID = @SupplierID";

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Name", Name);
                command.Parameters.AddWithValue("@Address", Address);
                command.Parameters.AddWithValue("@Phone", Phone);
                command.Parameters.AddWithValue("@SupplierID", selectedSupplier.SupplierID);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    MessageBox.Show("Данные поставщика успешно обновлены.");

                    originalSelectedSupplier.Name = selectedSupplier.Name;
                    originalSelectedSupplier.Address = selectedSupplier.Address;
                    originalSelectedSupplier.Phone = selectedSupplier.Phone;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления данных поставщика: {ex.Message}");
                }
            }
            LoadSuppliers();
        }

        private void LoadSuppliers()
        {
            Suppliers.Clear();
            string query = "SELECT SupplierID, Name, Address, Phone FROM Suppliers";
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
                            Suppliers.Add(new SupplierFull
                            {
                                SupplierID = reader.GetInt32(reader.GetOrdinal("SupplierID")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? string.Empty : reader.GetString(reader.GetOrdinal("Address")),
                                Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? string.Empty : reader.GetString(reader.GetOrdinal("Phone")),
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
