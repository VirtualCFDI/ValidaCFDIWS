using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;


namespace webservFacturas.conexion
{
    public class conector
    {
        public static SqlConnection ConexionSQL()
        {
            SqlConnection cnn = null;
            string conSQL = "Data Source=192.168.80.10"
                                + ";Initial Catalog=CFDIv2"
                                + ";Persist Security Info=False;User ID=sa"
                                + ";Password=Pa$$word";
            cnn = new SqlConnection(conSQL);
            cnn.Open();

            return cnn;
        }

        public static DataTable Consultasql(string query)
        {
            DataTable dt = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter();
            SqlConnection cnn = null;
            cnn = ConexionSQL();
            adapter.SelectCommand = new SqlCommand(query, cnn);

            adapter.Fill(dt);
            cnn.Close();
            return dt;
        }
        public bool InsertaSql(string Query)
        {
            try
            {
                SqlConnection cnn = null;
                cnn = ConexionSQL();
                SqlCommand comando = new SqlCommand(Query, cnn);
                comando.ExecuteNonQuery();
                cnn.Close();
            }
            catch
            {
                return false;
            }
            return true;

        }
    }
}