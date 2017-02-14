using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;

namespace SqlGenerator
{

    public class Program
    {
        ServerConnection connection;
        Server server;
        Database db;
        bool first = true;
        string newId = "newId";
        string oldId = "oldId";


        private static string QuoteName(string name)
        {
            return "[" + name + "]";
        }

        public Program()
        {
            //connection = new ServerConnection()
            server = new Server("localhost\\SQLEXPRESS");
            db = server.Databases["Budget"];
        }

        public int TestDB()
        {
            return db.Tables.Count;
        }

        private string addColumnName(string columnName)
        {
            string res = "";
            if (first)
                first = false;
            else
                res += ",";
            res += columnName;
            return res;
        }

        public string GenerateFieldList(TableHierarchy tableHierarchy, bool removePK = false, bool substituteFK = false, string parentIdMap = "")
        {
            string res = "";
            bool needCheck = true;
            string currentColumn;    
            Table table = db.Tables[tableHierarchy.TableName];
            first = true;
            foreach (var col in table.Columns)
            {
                needCheck = true;
                currentColumn = col.ToString();
                if (needCheck && removePK)
                {
                    if (col.ToString().Equals(Program.QuoteName(tableHierarchy.PrimaryKey)))
                    {
                        needCheck = false;
                        currentColumn = "";
                    }
                }

                if (needCheck && substituteFK)
                {
                    if (col.ToString().Equals(Program.QuoteName(tableHierarchy.ForeignKey)))
                    {
                        needCheck = false;
                        currentColumn = parentIdMap + ".newId";
                    }
                }

                if (currentColumn != "")
                    currentColumn = addColumnName(currentColumn);

                res += currentColumn;
            }
            return res;
        }

        public string GenerateSql(TableHierarchy tableHierarchy, string previousSql)
        {
            string sql = $"{previousSql}INSERT {tableHierarchy.TableName};";
            if (tableHierarchy.childHierarchy != null)
            {
                foreach (var childHier in tableHierarchy.childHierarchy)
                    sql = GenerateSql(childHier, sql);
            }
            return sql;
        }

        static void Main(string[] args)
        {
            TableHierarchy tableHier = new TableHierarchy
            {
                TableName = "ArticleGroup",
                PrimaryKey = "ArticleGroupId",
                childHierarchy = new List<TableHierarchy>
                {
                    new TableHierarchy
                    {
                        TableName = "Article",
                        PrimaryKey = "ArticleId",
                        ForeignKey = "ArticleGroupId"
                    },
                    new TableHierarchy
                    {
                        TableName = "ArticleGroupLabel",
                        PrimaryKey = "ArticleGroupLabelId",
                        ForeignKey = "ArticleGroupId"
                    }
                }
            };
            Program p = new Program();
            string sql = p.GenerateFieldList(tableHier);
            Console.WriteLine(sql);
            Console.WriteLine("===========FINISH============");
            Console.ReadKey();
        }
    };
}
