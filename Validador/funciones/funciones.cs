using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Xml;
using System.Xml.Schema;
using System.IO;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
namespace webservFacturas.funciones
{
    public class funciones
    {
        bool m_Success = false;

        //Función para verificar si el acceso fue correcto.
        public bool isIDLogCorrect(string idacceso)
        {
            webservFacturas.conexion.conector Conexion = new webservFacturas.conexion.conector();
            string sql = "SELECT * FROM clientesprepago WHERE iidacceso = '" + idacceso + "' AND  iactivo = 1";
            int cantidad = 0;
            DataTable dt = new DataTable();
            dt = webservFacturas.conexion.conector.Consultasql(sql);
            cantidad = Convert.ToInt16(dt.Rows.Count.ToString());
            if (cantidad == 0)
                return false;
            else return true;
        }

        //Función para obtener un nuevo UUID.
        public string GetUUID() {
            string sql = "SELECT NEWID() as uuid";
            DataTable data = new DataTable();
            data  = webservFacturas.conexion.conector.Consultasql(sql);
            string UUID = data.Rows[0]["uuid"].ToString();
            return UUID;
        }

        //Función para obtener el IID de Acceso.
        public string getiidAcceso(string usuario, string clave)
        {
            string sql = "SELECT iidacceso  FROM UsuariosValidacion WHERE vchUsuario = '" + usuario + "' AND vchClave = '" + clave + "' AND SiActivo = 1 ";
            DataTable data = new DataTable();
            string iidacceso = "";
            try
            {
                data = webservFacturas.conexion.conector.Consultasql(sql);
                iidacceso = data.Rows[0]["iidacceso"].ToString();
            }
            catch (Exception exp) { }
            return iidacceso;
        }

        //Función para insertar Petición.
        public string Inserta_FirstPeticion(string xml, string UUID, string idacceso,string msgadd, string versionp) {
            webservFacturas.conexion.conector conexion = new webservFacturas.conexion.conector();           
            string pstrClientAddress = HttpContext.Current.Request.UserHostAddress;
            //Creamos el XMLDocument.
            XmlDocument doc2 = new XmlDocument();
            //Cargamos el XML
            doc2.LoadXml(xml);
            //Inicializamos las variables.
            string total = "";
            string version = "";
            string tagcomprobante = "";
            string tagemisor = "";
            string tagreceptor = "";
            //Verificamos si la versión es 2.0 o 2.2 y asignamos los tags correspondientes.
            if ((versionp == "2.0") || (versionp == "2.2"))
            {
                tagcomprobante = "Comprobante";
                tagemisor = "Emisor";
                tagreceptor = "Receptor";
            }
            //Verificamos si la versión es 3.0 o 3.2 y asignamos los tags correspondientes.
            if ((versionp == "3.0") || (versionp == "3.2"))
            {
                tagcomprobante = "cfdi:Comprobante";
                tagemisor = "cfdi:Emisor";
                tagreceptor = "cfdi:Receptor";
            }
            //Leemos el nodo "Comprobante".
            XmlNodeList elemList = doc2.GetElementsByTagName(tagcomprobante);
            for (int i = 0; i < elemList.Count; i++)
            {   
                //Asignamos los atributos a variables.
                total = elemList[i].Attributes["total"].Value;
                version = elemList[i].Attributes["version"].Value;
            }
            //Inicializamos las variables.
            string RFC = "";
            string razon_emisor = "";
            //Leemos el Nodo Emisor.
            XmlNodeList EmisorList = doc2.GetElementsByTagName(tagemisor);
            for (int i = 0; i < EmisorList.Count; i++)
            {
                //Asignamos los atributos a variables.
                RFC = EmisorList[i].Attributes["rfc"].Value;
                razon_emisor = EmisorList[i].Attributes["nombre"].Value;
            }
            //Inicializamos las variables
            string rfc_receptor = "";
            string razon_receptor = "";
            //Leemos el nodo receptor.
            XmlNodeList ReceptorList = doc2.GetElementsByTagName(tagreceptor);
            for (int i = 0; i < ReceptorList.Count; i++)
            {
                //Asignamos los atributos a variables.
                rfc_receptor = ReceptorList[i].Attributes["rfc"].Value;
                razon_receptor = ReceptorList[i].Attributes["nombre"].Value;

            }
            string sql = ""+
                "INSERT INTO TmpSol_Intentos_Validaciones"+
                "(iidacceso, vchip, vchuuid, dfecha_ingreso, vchXML_cfd, vchRFC_emite, vchmsg, vchRazonEmite, vchRfc_receptor, vchRazon_receptor, total, vchversion )" +
                "VALUES"+
                "('" + idacceso + "','" + pstrClientAddress + "', '" + UUID + "', GETDATE(), '" + xml + "', @vchRFC_emite,'" + msgadd + "', @vchRazonEmite, @vchRfc_receptor, @vchRazon_receptor,'" + total + "', '" + version + "')";
            SqlCommand command = new SqlCommand(sql, webservFacturas.conexion.conector.ConexionSQL());
            command.Parameters.Add("@vchRFC_emite", SqlDbType.VarChar);
            command.Parameters["@vchRFC_emite"].Value = RFC;
            command.Parameters.Add("@vchRazonEmite", SqlDbType.VarChar);
            command.Parameters["@vchRazonEmite"].Value = razon_emisor;
            command.Parameters.Add("@vchRfc_receptor", SqlDbType.VarChar);
            command.Parameters["@vchRfc_receptor"].Value = rfc_receptor;
            command.Parameters.Add("@vchRazon_receptor", SqlDbType.VarChar);
            command.Parameters["@vchRazon_receptor"].Value = razon_receptor;
            command.ExecuteNonQuery();
            conexion.InsertaSql(sql);
            //Obtenemos el código insertado.
            sql=" SELECT top 1 iid FROM TmpSol_Intentos_Validaciones WHERE iidacceso = '"+idacceso+"' AND vchuuid = '"+UUID+"' ORDER BY dfecha_ingreso DESC ";
            DataTable data = new DataTable();
            data  = webservFacturas.conexion.conector.Consultasql(sql);
            string IID = data.Rows[0]["iid"].ToString();
            return IID;
        }

        //Función para Guardar el Registro de errorres en Base de Datos.
        public bool SaveErrorLog(string UUID, string codigoerror, string msg, string IID)
        {
            webservFacturas.conexion.conector conexion = new webservFacturas.conexion.conector();

            string sql = "INSERT INTO TmpSol_Intentos_Validaciones(vchuuid,vchcodError,vchmsgError,iid) VALUES('" + UUID + "','" + codigoerror + "','" + msg + "', '"+ IID +"')";
               

           /*
            string sql = "UPDATE TmpSol_Intentos_Validaciones SET (vchuuid, vchcodError, vchmsgError,iid ) values('" + UUID + "','" + codigoerror + "','" + msg + "','" + IID + "')";
            */
            return conexion.InsertaSql(sql);
            

          
        }

        //Función para Guardar la Cadena Original del XML, en Base de Datos.
        public bool SaveCO(string UUID, string co, string IID)
        {
            string sql = "UPDATE TmpSol_Intentos_Validaciones SET vchCadenaOriginal = @vchCadenaOriginal  WHERE vchuuid = '" + UUID + "'  AND iid = " + IID;
            SqlCommand command = new SqlCommand(sql, webservFacturas.conexion.conector.ConexionSQL());
            command.Parameters.Add("@vchCadenaOriginal", SqlDbType.VarChar);
            command.Parameters["@vchCadenaOriginal"].Value = co;//
            try
            {
                command.ExecuteNonQuery();
                return true;
            }catch(Exception exp){
                return false;
            }
        }

        //Función para Obtener la Cadena Original del XML.
        public string GetCadenaOrignal_byxml(string xml,string version)
        {
            string cadena_origina = "";
            //Asignamos la ruta dek directorio XSLT.
            string rutaxslt = ConfigurationManager.AppSettings["dirxslt"];
            //Nombres de los Archivos XSLT.
            string xslt20 = ConfigurationManager.AppSettings["xslt20"];
            string xslt22 = ConfigurationManager.AppSettings["xslt22"];
            string xslt30 = ConfigurationManager.AppSettings["xslt30"];
            string xslt32 = ConfigurationManager.AppSettings["xslt32"];    
            try
                {
                    XmlDocument myXMLPath = new XmlDocument();
                    myXMLPath.LoadXml(xml);                  
                    XslCompiledTransform myXSLTrans = new XslCompiledTransform();
                    //Si la versión es 2.0
                    if (version == "2.0")
                    {
                        myXSLTrans.Load(@""+ rutaxslt + xslt20 + "");//Cargamos el XSL 
                    }
                    //Si la versión es 2.2
                    if (version == "2.2")
                    {
                        myXSLTrans.Load(@"" + rutaxslt + xslt22 + "");//Cargamos el XSL
                    }
                    //Si la versión es 3.0
                    if (version == "3.0")
                    {
                        myXSLTrans.Load(@"" + rutaxslt + xslt30 + "");//Cargamos el XSL
                    }
                    //Si la versión es 3.2
                    if (version == "3.2")
                    {
                        myXSLTrans.Load(@"" + rutaxslt + xslt32 + ""); //Cargamos el XSL
                    }                    
                    StringWriter sr = new StringWriter();
                    myXSLTrans.Transform(myXMLPath, null, sr);
                    cadena_origina = sr.ToString();
                    return cadena_origina;
                }
                catch (Exception e)
                {
                    //Regresamos la Cadena Original.
                    return cadena_origina;
                }
        }
        
        //Función para obtener la Cadena Original del "Timbre Fiscal Digital".
        public string GetCadenaOrignalTFD(string xml)
        {
            //Inicializamos la variable.
            string cadena_origina = "";
            //Asignamos la ruta del directorio XSLT.
            string rutaxslt = ConfigurationManager.AppSettings["dirxslt"];
            //Nombre del Archivo XSLT.
            string xslttfd = ConfigurationManager.AppSettings["xslt32"];
            try
            {
                XmlDocument myXMLPath = new XmlDocument();
                myXMLPath.LoadXml(xml);
                XslCompiledTransform myXSLTrans = new XslCompiledTransform();
                myXSLTrans.Load(@"" + rutaxslt + xslttfd + "");          //Cargamos el XSLT
                StringWriter sr = new StringWriter();
                myXSLTrans.Transform(myXMLPath, null, sr);
                cadena_origina = sr.ToString();
                return cadena_origina;
            }
            catch (Exception e)
            {
                //Regresamos la Cadena Original del Timbre Fiscal Digital.
                return cadena_origina;
            }
        }

        //Función para obtener el estado de un certificado en la LCO(Lista de Contribuyentes Obligados).
        public string GetStatusLco(string certificado, string rfc) {
            string sql = "SELECT vchstatus FROM CatLCO WHERE vchrfc = '" + rfc + "' AND vchnumcer = '" + certificado + "'";
            DataTable data = new DataTable();
            data = webservFacturas.conexion.conector.Consultasql(sql);
            //Inicializamos la variable.
            string vchstatus ="";
            try
            {
                //COnvertimos el Estatus a String.
                vchstatus = data.Rows[0]["vchstatus"].ToString();
            }
            catch (Exception e) { }
            //Regresamos el Estatus.
            return vchstatus;
        }

        //Función para convertir Hexadecimal a ASCII.
        private static string hexString2Ascii(string hexString)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i <= hexString.Length - 2; i += 2)
            {
                sb.Append(Convert.ToString(Convert.ToChar(Int32.Parse(hexString.Substring(i, 2), System.Globalization.NumberStyles.HexNumber))));
            }
            //Regresamos la cadena.
            return sb.ToString();
        }
        
        //Función para convertir el certificado a Hexadecimal y extraer el Número de certificado.
        public string ConvertCertificado(string CertificadoEnBase64)
        {
            try
            {
            //Cargamos el certificado del CFDI.
            byte[] bytes = System.Convert.FromBase64String(CertificadoEnBase64);
            var x509 = new X509Certificate2(bytes);
            //Leemos el número de certificado.
            var NoCertificado = hexString2Ascii(x509.SerialNumber);
            //Regresamos el numero de certificado.
            return NoCertificado.ToString();
            }
            catch (System.Exception exp)
            {
                // Error creating stream or reading from it.
                System.Console.WriteLine("{0}", exp.Message);
                return "error";
            }
        }

        //Función para mostrar los errores de compilación.
        public static void ShowCompileErrors(object sender, ValidationEventArgs args)
        {
            Console.WriteLine("Validation Error: {0}", args.Message);
        }

        //Función para Validar el Esquema del CFDI
       // public bool ValidandoSkema2(string xml, string version)
        public string ValidandoSkema2(string xml, string version)
        {
            string schemaurl = "";
            //Ruta del Directorio XSD.
            string rutaxsd = ConfigurationManager.AppSettings["dirxsd"];
            //Nombre de los Archivos XSD.
            string xsd20 = ConfigurationManager.AppSettings["xsd20"];
            string xsd22 = ConfigurationManager.AppSettings["xsd22"];
            string xsd30 = ConfigurationManager.AppSettings["xsd30"];
            string xsd32 = ConfigurationManager.AppSettings["xsd32"];
            //Verificamos que versión es.
            if (version == "2.0")
            {
                schemaurl = "http://www.sat.gob.mx/cfd/2";
                rutaxsd += xsd20; //Asignamos el XSD 
            }

            if (version == "2.2")
            {
                schemaurl = "http://www.sat.gob.mx/cfd/2";
                rutaxsd += xsd22;//Asignamos el XSD 
            }
            if (version == "3.0")
            {
                rutaxsd += xsd30;//Asignamos el XSD
                schemaurl = "http://www.sat.gob.mx/cfd/3";
            }

            if (version == "3.2")
            {
                schemaurl = "http://www.sat.gob.mx/cfd/3";
                rutaxsd += xsd32;//Asignamos el XSD 
            }
            bool bandera = false;
            string msjerror = "";
            XmlValidatingReader reader = null;
            XmlSchemaCollection myschema = new XmlSchemaCollection();
            ValidationEventHandler eventHandler = new ValidationEventHandler(funciones.ShowCompileErrors);
            try
            {
                //Creamos el Context del XMLParser
                XmlParserContext context = new XmlParserContext(null, null, "", XmlSpace.None);
                //Se implementa el lector 
                reader = new XmlValidatingReader(xml, XmlNodeType.Element, context);
                //Agregamos el Esquema.
                myschema.Add(schemaurl, rutaxsd);
                //Colocamos el tipo de esquema, y agregamos el esqyema al lector
                reader.ValidationType = ValidationType.Schema;
                reader.Schemas.Add(myschema);
                    while (reader.Read())
                    {
                    }
                    //bandera = true;
                }
                catch (XmlException XmlExp)
                {
                    //bandera = false;
                    msjerror = XmlExp.Message;
                }
              catch (XmlSchemaException XmlSchExp)
              {
                  //bandera = false;
                  msjerror = XmlSchExp.Message;
                  //msjerror = "";
              }
                catch (Exception GenExp)
                {
                    //bandera = false;
                    msjerror = GenExp.Message;
                }
                finally
                {
                
                   // bandera = true;
                }

            return msjerror;
        }

        //Función para Validar el Esquema del TFD.
        public bool ValidandoSkemaTFD(string xml)
        {
            //Asignamos la ruta de la carpeta del XSD.
            string rutaxsd = ConfigurationManager.AppSettings["dirxsd"];
            string xsdtfd = ConfigurationManager.AppSettings["xsdtfd"];
            rutaxsd += xsdtfd;
            bool bandera = false;
            XmlValidatingReader reader = null;
            XmlSchemaCollection myschema = new XmlSchemaCollection();
            ValidationEventHandler eventHandler = new ValidationEventHandler(funciones.ShowCompileErrors);
            try
            {
                //Creamos el Context del XMLParser
                XmlParserContext context = new XmlParserContext(null, null, "", XmlSpace.None);
                //Se implementa el lector 
                reader = new XmlValidatingReader(xml, XmlNodeType.Element, context);
                //Agregamos el Esquema.
                myschema.Add("http://www.sat.gob.mx/TimbreFiscalDigital", rutaxsd);
                //Colocamos el tipo de esquema, y agregamos el esquema al lector
                reader.ValidationType = ValidationType.Schema;
                reader.Schemas.Add(myschema);
                while (reader.Read())
                {
                }
                bandera = true;
            }
            catch (XmlException XmlExp)
            {
                bandera = false;
            }
            catch (XmlSchemaException XmlSchExp)
            {
                bandera = false;
            }
            catch (Exception GenExp)
            {
                bandera = false;
            }
            finally
            {
                bandera = true;
            }
            return bandera;
        }

        //Función para Insertar Registro de Acceso a la Base de Datos.
        public void Inserta_LogAcceso(string xml, string UUID, string msgadd)
        {
            //Se obtiene la direccion IP del cliente.
            IPAddress ip = Dns.GetHostAddresses(Dns.GetHostName()).Where(address => address.AddressFamily == AddressFamily.InterNetwork).First();

            webservFacturas.conexion.conector conexion = new webservFacturas.conexion.conector();

            string sql = "INSERT INTO WebService (vchIP, vchUUIDAcceso, vchXML ) values('" +ip+ "','" + UUID + "','" + xml + "')";
            conexion.InsertaSql(sql);

            /*
            string sql = "" +
                "INSERT INTO WebService " +
                "(vchUUIDAcceso )" +
                "VALUES" +
                "('" + UUID + "')";
            conexion.InsertaSql(sql);*/
                     
        }

        //Función para validar la estructura del RFC
        public bool validaRFC(string campo)
        {
            //if (Regex.IsMatch(campo, @"^([&A-Z\s][&]{4})\d{6}([A-Z\w]{3})$"))
            if (Regex.IsMatch(campo, @"^(([&A-Z]|[&a-z]|[Ñ]){3})([0-9]{6})((([A-Z]|[a-z]|[0-9]){3}))$"))
            {
                return true;
            }
            else
            {
                //if (Regex.IsMatch(campo, @"^([&A-Z\s]{3})\d{6}([A-Z\w]{3})$"))
                if (Regex.IsMatch(campo, @"^(([&A-Z]|[&a-z]|[Ñ]|\s){1})(([A-Z]|[a-z]){3})([0-9]{6})((([A-Z]|[a-z]|[0-9]){3}))$"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        //Función para guardar el XML en la Base de Datos
        public bool GuardaCFDI(string xml, string UUID, string IID)
        {
            string correctString = xml.Replace(" schemaLocation", " xsi:schemaLocation");
            webservFacturas.conexion.conector conexion = new webservFacturas.conexion.conector();
            string sql = "UPDATE TmpSol_Intentos_Validaciones SET vchXML_cfdi = @vchXML_cfdi, iCorrecto=1, dfecha_salida=GETDATE()  WHERE vchuuid = '" + UUID + "'  AND iid = " + IID;
            //return conexion.InsertaSql(sql);            
            SqlCommand command = new SqlCommand(sql, webservFacturas.conexion.conector.ConexionSQL());
            command.Parameters.Add("@vchXML_cfdi", SqlDbType.VarChar);
            command.Parameters["@vchXML_cfdi"].Value = correctString;//
            //////////////////
            try
            {
                command.ExecuteNonQuery();
                return true;
            }
            catch (Exception exp)
            {
                return false;
            }
        }

        /*Funciones de los Códigos de Error*/
        //Función para validar el Error 403.
        public bool ValidaError403(string fecha)
        {
            string v = fecha.Replace("T", " ");
            string sql = "SELECT DATEDIFF(MINUTE,'2011-01-01', '" + v + "' ) horas";
            DataTable data = new DataTable();
            data = webservFacturas.conexion.conector.Consultasql(sql);
            int horas = Convert.ToInt32(data.Rows[0]["horas"].ToString());
            if (horas > 0 )
                return true;
            else
                return false;
        }

        //Función para validar el error 302.
        public bool Valida302(string xmltoText, string CadenaOriginal, string version)
        {
            string tagnamecfd = "";
            X509Certificate2 x509;
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(xmltoText);
            System.Xml.XmlNode element = doc.SelectSingleNode("/Emisor/DomicilioFiscal");
            if ((version == "2.0") || (version == "2.2"))
            {
                tagnamecfd = "Comprobante";
            }
            if ((version == "3.0") || (version == "3.2"))
            {
                tagnamecfd = "cfdi:Comprobante";
            }
            XmlNodeList elemListEmisor = doc.GetElementsByTagName(tagnamecfd);
            string Sello = elemListEmisor[0].Attributes["sello"].Value;
            string Certificado = elemListEmisor[0].Attributes["certificado"].Value;
            byte[] sello = Convert.FromBase64String(Sello);
            byte[] certificado = Encoding.UTF8.GetBytes(Certificado);
            byte[] cadenaOriginal = Encoding.UTF8.GetBytes(CadenaOriginal);
            x509 = new X509Certificate2(certificado);
            RSACryptoServiceProvider rsaCSP = (RSACryptoServiceProvider)x509.PublicKey.Key;
            bool bandera = rsaCSP.VerifyData(cadenaOriginal, CryptoConfig.MapNameToOID("SHA1"), sello);
            return bandera;
        }
        
        //Función para validar el error 305.
        public bool ValidaError305(string certificado, string RFC, string fechacreado)
        {
            string sql = "SELECT vchstatus FROM CatLCO WHERE vchrfc = '" + RFC + "' AND vchnumcer = '" + certificado + "' AND dfechainicio < '" + fechacreado + "' AND dfechafin >'" + fechacreado + "'";
            int cantidad = 0;
            DataTable dt = new DataTable();
            dt = webservFacturas.conexion.conector.Consultasql(sql);
            cantidad = Convert.ToInt16(dt.Rows.Count.ToString());
            if (cantidad == 0)
                return false;
            else return true;

        }

        //Función para validar el error 306.
        public bool Valida306(string cadena)
        {
            byte[] bytes = System.Convert.FromBase64String(cadena);
            var x509 = new X509Certificate2(bytes);
            Boolean es_correcto = false;
            var Cercolection = new X509Certificate2Collection(x509);
            for (int i = 0; i < Cercolection.Count; i++)
            {
                foreach (X509Extension extension in Cercolection[i].Extensions)
                {
                    if ((extension.Oid.FriendlyName == "Uso de la clave") || (extension.Oid.FriendlyName == "Key Usage"))
                    {
                        X509KeyUsageExtension ext = (X509KeyUsageExtension)extension;
                        // Console.WriteLine(ext.KeyUsages);
                        if (ext.KeyUsages.ToString().IndexOf("NonRepudiation") != -1 || ext.KeyUsages.ToString().IndexOf("DigitalSignature") != -1)
                        {
                            es_correcto = true;
                        }
                        else
                        {
                            es_correcto = false;
                        }

                    }

                }
            }
            return es_correcto;
        }
       
        //Función para validar el error 307
        public bool Valida307(string xmltoText, string CadenaOriginal, string version)
        {
            string tagnamecfd = "";
            X509Certificate2 x509;
            //string xmltoText = fnObtenxml(Comprobante);
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(xmltoText);
            System.Xml.XmlNode element = doc.SelectSingleNode("/Emisor/DomicilioFiscal");
            if ((version == "2.0") || (version == "2.2")) {
                tagnamecfd = "Comprobante";
            }
            if ((version == "3.0") || (version == "3.2"))
            {
                tagnamecfd = "cfdi:Comprobante";
            }
            XmlNodeList elemListEmisor = doc.GetElementsByTagName(tagnamecfd);
            string Sello = elemListEmisor[0].Attributes["sello"].Value;
            string Certificado = elemListEmisor[0].Attributes["certificado"].Value;
            byte[] sello = Convert.FromBase64String(Sello);
            byte[] certificado = Encoding.UTF8.GetBytes(Certificado);
            byte[] cadenaOriginal = Encoding.UTF8.GetBytes(CadenaOriginal);
            x509 = new X509Certificate2(certificado);
            RSACryptoServiceProvider rsaCSP = (RSACryptoServiceProvider)x509.PublicKey.Key;
            bool bandera = rsaCSP.VerifyData(cadenaOriginal, CryptoConfig.MapNameToOID("SHA1"), sello);
            return bandera;
        }

        //Función para validar el error 308
        public bool Valida308(string xml, string CadenaOriginal, string version)
        {
            string tagnamecfd = "";
            X509Certificate2 x509;
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(xml);
            System.Xml.XmlNode element = doc.SelectSingleNode("/Emisor/DomicilioFiscal");
            if ((version == "2.0") || (version == "2.2"))
            {
                tagnamecfd = "Comprobante";
            }
            if ((version == "3.0") || (version == "3.2"))
            {
                tagnamecfd = "cfdi:Comprobante";
            }
            XmlNodeList elemListEmisor = doc.GetElementsByTagName(tagnamecfd);
            string Sello = elemListEmisor[0].Attributes["sello"].Value;
            string Certificado = elemListEmisor[0].Attributes["certificado"].Value;
            byte[] sello = Convert.FromBase64String(Sello);
            byte[] certificado = Encoding.UTF8.GetBytes(Certificado);
            byte[] cadenaOriginal = Encoding.UTF8.GetBytes(CadenaOriginal);
            x509 = new X509Certificate2(certificado);
            //string CertificadosBanxico = @"C:\CFDIS\certificadosSAT\";
            return true;
            //validaCA(x509.GetRawCertData(), CertificadosBanxico);
        }

        /*public static bool validaCA(byte[] certificateValidate, String pathBanxico)
        {
            MonoX509.X509Certificate certt = new MonoX509.X509Certificate(certificateValidate);
            Mono.Security.X509.X509Chain xchan = new MonoX509.X509Chain();
            MonoX509.X509CertificateCollection collection = new MonoX509.X509CertificateCollection();

            byte[] caCertificate = File.ReadAllBytes(pathBanxico + "ca.crt");
            collection.Add(new MonoX509.X509Certificate(caCertificate));
            caCertificate = File.ReadAllBytes(pathBanxico + "AC0_SAT.cer");
            collection.Add(new MonoX509.X509Certificate(caCertificate));
            caCertificate = File.ReadAllBytes(pathBanxico + "AC1_SAT.cer");
            collection.Add(new MonoX509.X509Certificate(caCertificate));
            caCertificate = File.ReadAllBytes(pathBanxico + "AC2_SAT.cer");
            collection.Add(new MonoX509.X509Certificate(caCertificate));
            caCertificate = File.ReadAllBytes(pathBanxico + "ARC0_IES.cer");
            collection.Add(new MonoX509.X509Certificate(caCertificate));
            caCertificate = File.ReadAllBytes(pathBanxico + "ARC1_IES.cer");
            collection.Add(new MonoX509.X509Certificate(caCertificate));
            caCertificate = File.ReadAllBytes(pathBanxico + "int.cer");
            collection.Add(new MonoX509.X509Certificate(caCertificate));

            xchan.TrustAnchors = collection;
            return xchan.Build(certt);
        }*/

        //Función para validar el error 311.
        public bool Valida311(string serie, string RFC, string folio)
        {
            string sql = "SELECT * FROM Folios WHERE vchrfc = '" + RFC + "' AND vchserie = '" + serie + "' AND vchfoliofin <= '" + folio + "' AND vchfolioini >='" + folio + "'";
            int cantidad = 0;
            DataTable dt = new DataTable();
            dt = webservFacturas.conexion.conector.Consultasql(sql);
            cantidad = Convert.ToInt16(dt.Rows.Count.ToString());
            if (cantidad == 0)
                return false;
            else return true;
        }

        //Función para dar una segunda validación contra Esquema
        public bool ValidandoSkemaNew(string XmlString) {
            //Creamos el Lector del XML
            XmlTextReader xmlr = new XmlTextReader(new StringReader(XmlString));
            XmlValidatingReader xmlvread = new XmlValidatingReader(xmlr);
            xmlvread.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);
            m_Success = true; //La variable se tiene que inicializar despuyes de usada.
            while (xmlvread.Read()) { }
            //Cerramos el Lector.
            xmlvread.Close();
            //'El validationeventhandler es el unico que puede volver al m_Success false 
            return m_Success;
        }

        private void ValidationCallBack(Object sender, ValidationEventArgs args)
        {
            //'Display the validation error.  This is only called on error
            m_Success = false; //'Validation failed

        }
    }
}