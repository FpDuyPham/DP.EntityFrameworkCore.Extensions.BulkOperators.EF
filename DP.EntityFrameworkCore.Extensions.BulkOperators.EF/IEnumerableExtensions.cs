using DP.EntityFrameworkCore.Extensions.BulkOperators.EF.Resolvers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace DP.EntityFrameworkCore.Extensions.BulkOperators.EF
{
    public static class IEnumerableExtensions
    {
        public static DataTable ToDataTable<TEntity>(this IEnumerable<TEntity> items, bool exceptVirtualMethod = true) where TEntity : class, new()
        {
            DataTable dataTable = new DataTable(typeof(TEntity).Name);

            //Get all the properties
            PropertyInfo[] Props;
            if (typeof(IBulkPropertyResolver).IsAssignableFrom(typeof(TEntity)))
            {
                var fields = (new TEntity() as IBulkPropertyResolver).GetFields();
                Props = typeof(TEntity).GetProperties()
                    .Where(p => fields.Contains(p.Name))
                    .OrderBy(p => Array.IndexOf(fields, p.Name))
                    .ToArray();
            }
            else
            {
                Props = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                if (exceptVirtualMethod)
                    Props = Props.Where(p => p.GetGetMethod() == null || p.GetGetMethod().IsVirtual != true).ToArray();
            }

            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType;
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }

            foreach (TEntity item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }

                dataTable.Rows.Add(values);
            }

            return dataTable;
        }
    }
}
