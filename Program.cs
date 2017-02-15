using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;

namespace SqlGenerator
{

    public delegate string ApplyTemplateToColumn(string columnName, string alias, string dataType);

    public class Program
    {
        ServerConnection connection;
        Server server;
        Database db;
        bool first = true;
        string newId = "NewIdValue";
        string oldId = "OldIdValue";

        public static string OrdinaryTemplate(string columnName, string alias, string dataType)
        {
            string res =         
                columnName;

            if (alias != "")
                res = alias + "." + res;
            return res;
        }


        public static string XMLTemplate(string columnName, string alias, string dataType)
        {
            string res =
                columnName;

            if (alias != "")
                res = alias + "." + res;
            return res;
        }

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

        private string addColumnName(string columnName, string alias, string dataType, ApplyTemplateToColumn template)
        {
            string res = "";
            if (first)
                first = false;
            else
                res += ",";
            string s = template(columnName, alias, dataType);
            res += s;
            return res;
        }

        public string GenerateSelect(TableHierarchy tableHierarchy, string parentIdMap)
        {
            
            string fieldList = GenerateFieldList(tableHierarchy, Program.OrdinaryTemplate, false, true, "idMap");
            string res = $"SELECT {fieldList} FROM {Program.QuoteName(tableHierarchy.TableName)}";
            if (tableHierarchy.ForeignKey != "" && tableHierarchy.ForeignKey != null)
            {
                res += $" JOIN {parentIdMap} idMap ON idMap.{oldId}={Program.QuoteName(tableHierarchy.ForeignKey)}";
            }
            return res;
        }

        public string GenerateFieldList(TableHierarchy tableHierarchy, ApplyTemplateToColumn template, bool removePK = false, bool substituteFK = false, string parentIdMap = "", string alias = "")
        {
            string res = "";
            bool needCheck = true;
            string currentColumn;    
            Table table = db.Tables[tableHierarchy.TableName];
            first = true;
            string columnAlias;
            foreach (Column col in table.Columns)
            {
                columnAlias = alias;
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
                        columnAlias = "";
                        currentColumn = $"{parentIdMap}.{newId}";
                    }
                }

                if (currentColumn != "")
                    currentColumn = addColumnName(currentColumn, columnAlias, GetColumnType(col),  template);

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

        public string GetColumnTypeByName(string tableName, string columnName)
        {
            Table t = db.Tables[tableName];
            return GetColumnType(t.Columns[columnName]);
        }

        public string GetColumnType(Column column)
        {
            string nameOfType = column.DataType.ToString();
            if (nameOfType.Contains("char") || nameOfType.Contains("binary"))
            {
                if (column.DataType.MaximumLength < 0)
                    nameOfType += "(MAX)";
                else
                    nameOfType += $"({column.DataType.MaximumLength})";
            }
            else if (nameOfType.Equals("numeric") || nameOfType.Equals("decimal"))
            {
                
                nameOfType += $"({column.DataType.NumericPrecision},{column.DataType.NumericScale})";
            }
            return nameOfType;
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
            Console.WriteLine(p.GetColumnTypeByName("ArticleGroup", "ArticleGroupId"));
            string sql = p.GenerateSelect(tableHier, "#ArticleGroupMap");
            Console.WriteLine(sql);
            Console.WriteLine("===========FINISH============");
            Console.ReadKey();
        }
    };
}
