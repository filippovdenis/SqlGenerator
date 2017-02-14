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
        string newId = "NewIdValue";
        string oldId = "OldIdValue";


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

        public string GenerateSelect(TableHierarchy tableHierarchy, string parentIdMap)
        {
            
            string fieldList = GenerateFieldList(tableHierarchy, false, true, "idMap");
            string res = $"SELECT {fieldList} FROM {Program.QuoteName(tableHierarchy.TableName)}";
            if (tableHierarchy.ForeignKey != "" && tableHierarchy.ForeignKey != null)
            {
                res += $" JOIN {parentIdMap} idMap ON idMap.{oldId}={Program.QuoteName(tableHierarchy.ForeignKey)}";
            }
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
                        currentColumn = $"{parentIdMap}.{newId}";
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
                TableName = "Article",
                PrimaryKey = "ArticleId",
                //ForeignKey = "ArticleGroupId"
            };
            Program p = new Program();
            string sql = p.GenerateSelect(tableHier, "#ArticleGroupMap");
            Console.WriteLine(sql);
            Console.WriteLine("===========FINISH============");
            Console.ReadKey();
        }
    };
}
