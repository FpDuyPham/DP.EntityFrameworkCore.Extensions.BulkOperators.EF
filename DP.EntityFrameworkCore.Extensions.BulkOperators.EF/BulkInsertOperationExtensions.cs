using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DP.EntityFrameworkCore.Extensions.BulkOperators.EF
{
    public static class BulkInsertOperationExtensions
    {
        public static async Task BulkImportAsync<TEntity>(this DbContext dbContext, IEnumerable<TEntity> entities, int batchSize = 10000) where TEntity : class, new()
        {
            if (!entities.Any())
                return;

            var connectionString = dbContext.Database.GetDbConnection().ConnectionString;
            var entityType = dbContext.Model.FindEntityType(typeof(TEntity));
            var destinationTableName = entityType.GetTableName();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(connection)
                {
                    DestinationTableName = destinationTableName,
                    BatchSize = batchSize
                })
                {
                    var datatable = entities.ToDataTable();
                    datatable.TableName = destinationTableName;

                    //If the data source and the destination table have the same number of columns, and the ordinal position of each source column within the data source matches the ordinal position of the corresponding destination column, the ColumnMappings collection is unnecessary.However, if the column counts differ, or the ordinal positions are not consistent, you must use ColumnMappings to make sure that data is copied into the correct columns.
                    foreach (DataColumn column in datatable.Columns)
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(column.ColumnName, column.ColumnName));

                    connection.Open();
                    try
                    {
                        await sqlBulkCopy.WriteToServerAsync(datatable);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
        }

        //public static async Task BulkImportAsyncUseFastMember<TEntity>(this DbContext dbContext, IEnumerable<TEntity> entities, int batchSize = 10000) where TEntity : class, new()
        //{
        //    if (!entities.Any())
        //        return;

        //var connectionString = dbContext.Database.GetDbConnection().ConnectionString;
        //var entityType = dbContext.Model.FindEntityType(typeof(TEntity));
        //var destinationTableName = entityType.GetTableName();

        //    using (SqlConnection connection = new SqlConnection(conn))
        //    {
        //        //Lets you efficiently bulk load a SQL Server table with data from another source.
        //        //Create an instance of SqlBulkCopy class
        //        using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(connection)
        //        {
        //            DestinationTableName = destinationTableName,
        //            BatchSize = batchSize
        //        })
        //        {


        //            try
        //            {
        //                PropertyInfo[] props = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.GetGetMethod() != null ? p.GetGetMethod().IsVirtual != true : true).ToArray();
        //                var fieldNames = props.Select(p => p.Name).ToList();
        // use FastMember to increase performance
        //                using (var reader = ObjectReader.Create(entities, fieldNames.ToArray()))
        //                {
        //                    await sqlBulkCopy.WriteToServerAsync(reader);
        //                }

        //                // Write from the source to the destination.

        //            }
        //            catch (Exception ex)
        //            {
        //                throw ex;
        //            }

        //        }
        //    }

        //}
    }
}
