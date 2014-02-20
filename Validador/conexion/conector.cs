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
            
            //Se introducen los datos de la Base de Datos
            SqlConnection cnn = null;
            string conSQL = "Data Source=PRUEBASPLATAFOR"
                                + ";Initial Catalog=validaCFDI"
                                + ";Persist Security Info=False;User ID=sa"
                                + ";Password=ASDasd123*";
            cnn = new SqlConnection(conSQL);
            cnn.Open();

            return cnn;

                               
         //  string conSQL = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["CadenaConexion"].ConnectionString;
            
          
        }

        //Función para realizar consultas.
        public static DataTable Consultasql(string query)
        {
            SqlConnection cnn = ConexionSQL();
            DataTable dt = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter();
            adapter.SelectCommand = new SqlCommand(query, cnn);
            adapter.Fill(dt);
            cnn.Close();
            return dt;
        }

        //Función para insertar registros
        public bool InsertaSql(string Query)
        {
            try
            {
                SqlConnection cnn = ConexionSQL();            
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

        //Funciómn para cerrar la conexión
        public static void Closeconection(SqlConnection cnn)
        {
            cnn.Close();
        }
    }
}