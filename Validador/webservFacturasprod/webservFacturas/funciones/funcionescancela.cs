using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace webservFacturas.funciones
{
    public class funcionescancela
    {
        public bool Inserta_FirstPeticion(string UUID, string idacceso)
        {
            webservFacturas.conexion.conector conexion = new webservFacturas.conexion.conector();

            string pstrClientAddress = HttpContext.Current.Request.UserHostAddress;

            string sql = "" +
                "INSERT INTO TmpSol_Intentos_cancelacion " +
                "(iidacceso, vchip, vchuuid, dfecha_ingreso )" +
                "VALUES" +
                "('" + idacceso + "','" + pstrClientAddress + "', '" + UUID + "', GETDATE()   )";
            bool respuesta = conexion.InsertaSql(sql);
            return respuesta;
            /*sql = " SELECT top 1 iid FROM TmpSol_Intentos_cancelacion WHERE iidacceso = '" + idacceso + "' AND vchuuid = '" + UUID + "' ORDER BY dfecha_ingreso DESC ";
            DataTable data = new DataTable();
            data = webservFacturas.conexion.conector.Consultasql(sql);
            string IID = data.Rows[0]["iid"].ToString();
            return IID;*/
            
        }
        public bool ExisteUUID(string UUID, string idacceso) {
            //verificar que este uuid haya sido enviado correctamente anteriormente
            //verificamos que ese UUID que intanta cancelar le corresponda a el
            string sql = "SELECT * FROM TmpSol_Intentos_timbres WHERE vchuuid = '" + UUID + "' AND iCorrecto = 1 AND iEnviado_Sat = 1 AND iidacceso = '"+idacceso+"' ";
            int cantidad = 0;

            DataTable dt = new DataTable();
            dt = webservFacturas.conexion.conector.Consultasql(sql);
            cantidad = Convert.ToInt16(dt.Rows.Count.ToString());
            if (cantidad == 0)
                return false;
            else return true;
        }
        public bool SaveErrorLogC(string msg, string UUID, string IID)
        {
            webservFacturas.conexion.conector conexion = new webservFacturas.conexion.conector();
            string sql = "UPDATE TmpSol_Intentos_cancelacion SET  vchmsgError = '" + msg + "', dfecha_salida = GETDATE() " +
            " WHERE vchuuid = '" + UUID + "'  AND iid = " + IID;
            return conexion.InsertaSql(sql);
        }
        public bool ActivaPendienteEnvioSAt( string IID, string UUID, string idacceso) {
            webservFacturas.conexion.conector conexion = new webservFacturas.conexion.conector();
            string sql = "UPDATE TmpSol_Intentos_cancelacion SET  iCorrecto = '1', dfecha_salida = GETDATE() " +
            " WHERE vchuuid = '" + UUID + "'  AND iid = " + IID;
            return conexion.InsertaSql(sql);
        }

        public bool ExisteCancelado(string UUID, string idacceso) {
            string sql = "SELECT * FROM TmpSol_Intentos_cancelacion WHERE  vchuuid = '" + UUID + "' AND iidacceso = '" + idacceso + "'  ";
            int cantidad = 0;
            DataTable dt = new DataTable();
            dt = webservFacturas.conexion.conector.Consultasql(sql);
            cantidad = Convert.ToInt16(dt.Rows.Count.ToString());
            if (cantidad > 0)
                return true;
            else return false;
        }
        public bool ExisteEnviadoalSat(string UUID, string idacceso)
        {
            string sql = "SELECT * FROM TmpSol_Intentos_timbres WHERE iidacceso = '" + idacceso + "' AND vchuuid = '" + UUID + "' AND iCorrecto = 1 AND iEnviado_Sat = 1 ";
            int cantidad = 0;
            DataTable dt = new DataTable();
            dt = webservFacturas.conexion.conector.Consultasql(sql);
            cantidad = Convert.ToInt16(dt.Rows.Count.ToString());
            if (cantidad == 0)
                return false;
            else return true;
            
        }
        ///////////existe factura
        public bool ExisteFac(string UUID, string idacceso)
        {
            string sql = "SELECT * FROM TmpSol_Intentos_timbres WHERE iidacceso = '" + idacceso + "' AND vchuuid = '" + UUID + "' AND iCorrecto = 1  ";
            int cantidad = 0;
            DataTable dt = new DataTable();
            dt = webservFacturas.conexion.conector.Consultasql(sql);
            cantidad = Convert.ToInt16(dt.Rows.Count.ToString());
            if (cantidad == 0)
                return false;
            else return true;
        }
    }
}