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

    /// <summary>
    /// Interaction logic for AddingproductType.xaml
    /// </summary>
    namespace IKEA.View
    {
        public partial class AddingProductType : Window 
        {
            public AddingProductType(string _connect)
            {
                InitializeComponent();
                this.DataContext = new ViewModelAddProductType(_connect);

            }
        }
    }
