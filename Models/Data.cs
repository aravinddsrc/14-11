using System;
using System.Collections.Generic;

using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace DSRCManagementSystem.Models
{
    public static class Data
    {
        #region DataAdaptor Fill
        public static void Fill(this DataTable dataTable, string procedureName)
        {
            SqlConnection connection = new SqlConnection();
            //connection.ConnectionString = "data source=DSRCMCSP16;initial catalog=DSRCHRMS_DevDB;user id=rebar;password=rebar@123";
            connection.ConnectionString = ConfigurationManager.AppSettings["connstr"];
            FillData(dataTable, procedureName, null, connection);
        }
        public static void Fill(this DataTable dataTable, string procedureName, SqlParameter[] parameters)
        {
            SqlConnection connection = new SqlConnection();
            //connection.ConnectionString = @"Data Source=192.168.4.43\sde;Initial Catalog=ssc_2015;uid=admin;pwd=dsrcadmin; Connect Timeout=4200";
            connection.ConnectionString = ConfigurationManager.AppSettings["connstr"];
            FillData(dataTable, procedureName, parameters, connection);
        }
        private static void FillData(DataTable dataTable, string procedureName, SqlParameter[] parameters, SqlConnection connection)
        {
            SqlCommand command = new SqlCommand(procedureName, connection);
            command.CommandType = CommandType.StoredProcedure;

            if (parameters != null)
                command.Parameters.AddRange(parameters);

            SqlDataAdapter dataAdapter = new SqlDataAdapter();

            dataAdapter.SelectCommand = command;
            try
            {
                dataAdapter.Fill(dataTable);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
                connection.Dispose();
                dataAdapter.Dispose();
            }
        }


        #endregion

    }
}