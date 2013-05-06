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
            /*string conSQL = "Data Source=LUISACA-PC\\DL360G7"
                                + ";Initial Catalog=Validador"
                                + ";Persist Security Info=False;User ID=sa"
                                + ";Password=Pa$$word";*/
            string conSQL = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["CadenaConexion"].ConnectionString;
            cnn = new SqlConnection(conSQL);
            cnn.Open();
            return cnn;
        }

        //Función para realiuzar consultas.
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
            SqlConnection cnn = ConexionSQL();
            try
            {
                SqlCommand comando = new SqlCommand(Query, cnn);
                comando.ExecuteNonQuery();
            }
            catch
            {
                return false;
            }
            cnn.Close();
            return true;
        }

        //Funciómn para cerrar la conexión
        public static void Closeconection(SqlConnection cnn)
        {
            cnn.Close();
        }
    }
}