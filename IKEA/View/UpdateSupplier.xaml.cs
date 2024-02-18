using IKEA.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IKEA.View
{
    /// <summary>
    /// Interaction logic for UpdateSupplier.xaml
    /// </summary>
    public partial class UpdateSupplier : Window
    {
        public UpdateSupplier(string connectionString)
        {
            InitializeComponent();
            this.DataContext = new ViewModelUpdateSupplier(connectionString);
        }
    }
}
