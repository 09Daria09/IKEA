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
    /// Interaction logic for Deletion.xaml
    /// </summary>
    public partial class Deletion : Window
    {
        public Deletion(string _connectionInfo, int i)
        {
            InitializeComponent();
            this.DataContext = new ViewModelDeletion(_connectionInfo, i);
        }
    }
}
