using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator
{
    public class TableHierarchy
    {
        public string TableName { get; set; }
        public string PrimaryKey { get; set; }
        public string ForeignKey { get; set; }
        public List<TableHierarchy> childHierarchy { get; set; }       

    }
}
