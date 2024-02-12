using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKEA.Model
{
    internal class DatabaseConnectionInfo
    {
        public string UserName { get; set; } = "6E7PN2T";
        public string DatabaseName { get; set; } = "Storage";

        public string GetConnectionString()
        {
            return $"Initial Catalog={DatabaseName};Data Source={UserName};Integrated Security=true";
        }
    }
}
