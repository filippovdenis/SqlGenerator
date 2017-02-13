using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator
{

    public class Program
    {
        public string GenerateSql(TableHierarchy tableHierarchy, string previousSql)
        {
            return $"{previousSql}INSERT {tableHierarchy.TableName};";
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
                    }
                }
            };
            Console.WriteLine($"{tableHier.TableName} : {tableHier.PrimaryKey} : {tableHier.ForeignKey}");
            Console.WriteLine(tableHier.childHierarchy.Count);
            foreach(var childHier in tableHier.childHierarchy)
            {
                Console.WriteLine($"{childHier.TableName} : {childHier.PrimaryKey} : {childHier.ForeignKey}");
            }
            Console.ReadKey();
        }
    }
}
