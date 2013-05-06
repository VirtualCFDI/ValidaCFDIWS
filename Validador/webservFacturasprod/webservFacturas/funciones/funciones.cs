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
using Chilkat;
using System.Text.RegularExpressions;

namespace webservFacturas.funciones
{
    public class funciones
    {
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
        public bool InsertaPeticionCanelacion(string idacceso, string UUID)
        {
            webservFacturas.conexion.conector conexion = new webservFacturas.conexion.conector();
            string sql = "insert into HisIntentoCancel ( vchuuid, vchidacceo ) values ( '" + UUID + "', '" + idacceso + "') ";
            return conexion.InsertaSql(sql);

        }

        public string GetUUID() {
            string sql = "SELECT NEWID() as uuid";
            DataTable data = new DataTable();
            data  = webservFacturas.conexion.conector.Consultasql(sql);
            string UUID = data.Rows[0]["uuid"].ToString();
            return UUID;
        }
        public string getiidAcceso(string usuario, string clave)
        {
            string sql = "SELECT iidacceso  FROM UsuariosTimbres WHERE vchUsuario = '" + usuario + "' AND vchClave = '" + clave + "' AND SiActivo = 1 ";
            DataTable data = new DataTable();
            
            string iidacceso = "";
            try
            {
                 data = webservFacturas.conexion.conector.Consultasql(sql);
                 iidacceso = data.Rows[0]["iidacceso"].ToString();
            }catch(Exception exp){}
            return iidacceso;
        }

        public string Inserta_FirstPeticion(string xml, string UUID, string idacceso, string msgadd)
        {
            webservFacturas.conexion.conector conexion = new webservFacturas.conexion.conector();
            string pstrClientAddress = HttpContext.Current.Request.UserHostAddress;

            XmlDocument doc2 = new XmlDocument();
            doc2.LoadXml(xml);

            string total = "";
            string version = "";
            string serie = "";
            string folio = "";
            XmlNodeList elemList = doc2.GetElementsByTagName("cfdi:Comprobante");
            for (int i = 0; i < elemList.Count; i++)
            {
                total = elemList[i].Attributes["total"].Value;
                version = elemList[i].Attributes["version"].Value;
                try
                {
                    folio = elemList[i].Attributes["folio"].Value;
                    serie = elemList[i].Attributes["serie"].Value;
                }
                catch
                {
                }
            }

            string RFC = "";
            string razon_emisor = "";
            XmlNodeList EmisorList = doc2.GetElementsByTagName("cfdi:Emisor");
            for (int i = 0; i < EmisorList.Count; i++)
            {
                RFC = EmisorList[i].Attributes["rfc"].Value;
                razon_emisor = EmisorList[i].Attributes["nombre"].Value;
            }

            string rfc_receptor = "";
            string razon_receptor = "";
            XmlNodeList ReceptorList = doc2.GetElementsByTagName("cfdi:Receptor");
            for (int i = 0; i < ReceptorList.Count; i++)
            {
                rfc_receptor = ReceptorList[i].Attributes["rfc"].Value;
                razon_receptor = ReceptorList[i].Attributes["nombre"].Value;
            }

            string sql = "" +
                "INSERT INTO TmpSol_Intentos_timbres" +
                "(iidacceso, vchip, vchuuid, dfecha_ingreso, vchXML_cfd, vchRFC_emite, vchmsg, vchRazonEmite, vchRfc_receptor, vchRazon_receptor, total, vchversion, vchNumSerie, vchFolio )" +
                "VALUES" +
                "('" + idacceso + "','" + pstrClientAddress + "', '" + UUID + "', GETDATE(), '" + xml + "', '" + RFC + "','" + msgadd + "', '" + razon_emisor + "', '" + rfc_receptor + "','" + razon_receptor + "','" + total + "', '" + version + "', '" + serie + "', '" + folio + "')";
            conexion.InsertaSql(sql);
            //obtenemo el Id insertado
            sql=" SELECT top 1 iid FROM TmpSol_Intentos_timbres WHERE iidacceso = '"+idacceso+"' AND vchuuid = '"+UUID+"' ORDER BY dfecha_ingreso DESC ";
            DataTable data = new DataTable();
            data  = webservFacturas.conexion.conector.Consultasql(sql);
            string IID = data.Rows[0]["iid"].ToString();
            return IID;

        }
        public bool SaveErrorLog(string UUID, string codigoerror, string msg, string IID)
        {
            webservFacturas.conexion.conector conexion = new webservFacturas.conexion.conector();
            string sql = "UPDATE TmpSol_Intentos_timbres SET vchcodError = '"+codigoerror+"', vchmsgError = '"+msg+"', dfecha_salida = GETDATE() "+
            " WHERE vchuuid = '" + UUID + "'  AND iid = " + IID;
            return conexion.InsertaSql(sql);
        }
        public bool SaveCO(string UUID, string co, string IID)
        {
            webservFacturas.conexion.conector conexion = new webservFacturas.conexion.conector();
            string sql = "UPDATE TmpSol_Intentos_timbres SET vchCadenaOriginal = '" + co + "'  WHERE vchuuid = '" + UUID + "'  AND iid = " + IID;
            return conexion.InsertaSql(sql);
        }
        public string GetCadenaOrignal_byxml(string xml, string version)
        {
            
                string cadena_origina = "";
                try
                {
                    
                   
                    XmlDocument myXMLPath = new XmlDocument();
                    myXMLPath.LoadXml(xml);
                    
                   
                    XslCompiledTransform myXSLTrans = new XslCompiledTransform();
                    if (version == "3.0")
                    {
                        myXSLTrans.Load(@"C://xslt//cadenaoriginal_3_0.xslt");          //load the Xsl 
                    }
                    else
                    {
                        if (version == "3.2")
                        {
                            myXSLTrans.Load(@"C://xslt//cadenaoriginal_3_2.xslt");          //load the Xsl 
                        }
                    }
                    

                   
                    //XmlTextWriter myWriter = new XmlTextWriter("tmp", null);     //create the output stream
                    //myXSLTrans.Transform(myXMLPath, null, myWriter);             //do the actual transform of Xml ---> fout!!??

                   

                    //string reader = myWriter.ToString();
                    StringWriter sr = new StringWriter();

                    

                    myXSLTrans.Transform(myXMLPath, null, sr);

                   

                    cadena_origina = sr.ToString();

                    

                   // myWriter.Close();


                    return cadena_origina.Trim();
                    
                }
                catch (Exception e)
                {
                    //this.Inserta_FirstPeticion(e.ToString(), "error xml", "bien");
                    return cadena_origina;
                }
        }
        public string GetStatusLco(string certificado, string rfc) {
            string sql = "SELECT vchstatus FROM CatLco2 WHERE vchrfc = '" + rfc + "' AND vchnumcer = '" + certificado + "'";
            DataTable data = new DataTable();
            data = webservFacturas.conexion.conector.Consultasql(sql);
            string vchstatus ="";
            try
            {
                vchstatus = data.Rows[0]["vchstatus"].ToString();
            }
            catch (Exception e) { }

            return vchstatus;
        }
        private static string hexString2Ascii(string hexString)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i <= hexString.Length - 2; i += 2)
            {
                sb.Append(Convert.ToString(Convert.ToChar(Int32.Parse(hexString.Substring(i, 2), System.Globalization.NumberStyles.HexNumber))));
            }
            return sb.ToString();
        }
        public string ConvertCertificado(string CertificadoEnBase64)
        {
            //carga el certificado del comprobante
            byte[] bytes = System.Convert.FromBase64String(CertificadoEnBase64);
            var x509 = new X509Certificate2(bytes);
            var NoCertificado = hexString2Ascii(x509.SerialNumber);
            return NoCertificado.ToString();
        }
        public string GetFechaIngresoTimbreado(string UUID, string idacceso, string IID)
        {
            string sql = "SELECT CONVERT(VARCHAR,dfecha_ingreso, 120) dfecha_ingreso FROM TmpSol_Intentos_timbres WHERE vchuuid = '" + UUID + "' AND iidacceso = '" + idacceso + "' AND iid = " + IID;
            DataTable data = new DataTable();
            data = webservFacturas.conexion.conector.Consultasql(sql);
            string fechaTimbre = data.Rows[0]["dfecha_ingreso"].ToString();
            return fechaTimbre.Replace(" ","T");
        }
        public int FacturasdelMes(string idacesso) { 
            string sql=" "
                +" SELECT COUNT(*)cantidad "
                +" FROM TmpSol_Intentos_timbres "
                +" WHERE iidacceso ='"+idacesso+"' "
                +" AND dfecha_ingreso between "
	                +" cast(datepart(year,GETDATE())as varchar(4))+'-'+cast(datepart(MONTH,GETDATE())as varchar(2))+'-01' "
	                +" AND CAST( "
	                +" cast(datepart(year,dateadd(MONTH,1,GETDATE()))as varchar(4))+'-'+cast(datepart(MONTH,dateadd(MONTH,1,GETDATE()))as varchar(2))+'-01' "
	                +" AS DATETIME "
                +" AND iCorrecto = 1 ";
            DataTable data = new DataTable();
            data = webservFacturas.conexion.conector.Consultasql(sql);
            int cantidad = Convert.ToInt32(data.Rows[0]["cantidad"].ToString());
            return cantidad;
        }
        public bool RestaExistenciaFacturaTimbre( string idacceso) {
            conexion.conector conexion = new conexion.conector();
            string sql = "SELECT vchTipoCliente, iLimiteFac, iExistenciaFacturas FROM clientesprepago WHERE iactivo = 1 AND iidacceso = '" + idacceso + "' ";
            DataTable data = new DataTable();
            data = webservFacturas.conexion.conector.Consultasql(sql);
            string vchTipoCliente = data.Rows[0]["vchTipoCliente"].ToString();
            if (vchTipoCliente == "prepago")
            {
                sql = "UPDATE clientesprepago SET iExistenciaFacturas = iExistenciaFacturas - 1 WHERE iidacceso = '" + idacceso + "' ";
                return conexion.InsertaSql(sql);
            }

            return true;
        }
        public bool validaTipoCliente(string idacceso) {
            string sql = "SELECT vchTipoCliente, iLimiteFac, iExistenciaFacturas FROM clientesprepago WHERE iactivo = 1 AND iidacceso = '" + idacceso + "' ";
            DataTable data = new DataTable();
            data = webservFacturas.conexion.conector.Consultasql(sql);
            string vchTipoCliente = data.Rows[0]["vchTipoCliente"].ToString();
            int iLimite = Convert.ToInt32(data.Rows[0]["iLimiteFac"].ToString());
            int iExistencia = Convert.ToInt32(data.Rows[0]["iExistenciaFacturas"].ToString());

            if (vchTipoCliente == "prepago")
            {
                //veo que tenga existencias
                if (iExistencia > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else { 
                //es credito

                int tiene_actualemte_consumidas = FacturasdelMes(idacceso);
                if (tiene_actualemte_consumidas >= iLimite)
                {
                    return false;
                }
                else {
                    return true;
                }
            }
            
        }

        
        public static void ShowCompileErrors(object sender, ValidationEventArgs args)
        {
            Console.WriteLine("Validation Error: {0}", args.Message);
        }
        public bool ValidandoSkema2(string xml, string ruta_xsd)
        {
            
            bool bandera = false;
            XmlValidatingReader reader = null;
            XmlSchemaCollection myschema = new XmlSchemaCollection();
            ValidationEventHandler eventHandler = new ValidationEventHandler(funciones.ShowCompileErrors);
            try
            {
                
                //Create the XmlParserContext.
                XmlParserContext context = new XmlParserContext(null, null, "", XmlSpace.None);

                //Implement the reader. 
                reader = new XmlValidatingReader(xml, XmlNodeType.Element, context);
                //Add the schema.
                myschema.Add("http://www.sat.gob.mx/cfd/3", ruta_xsd);

                //Set the schema type and add the schema to the reader.
                reader.ValidationType = ValidationType.Schema;
                reader.Schemas.Add(myschema);

                    while (reader.Read())
                    {
                    }

                   // Console.WriteLine("Completed validating xmlfragment");
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
        public bool GuardaCFDI(string xml, string UUID, string IID)
        {
            string correctString = xml.Replace(" schemaLocation", " xsi:schemaLocation");
            webservFacturas.conexion.conector conexion = new webservFacturas.conexion.conector();
            string sql = "UPDATE TmpSol_Intentos_timbres SET vchXML_cfdi = '" + correctString + "', iCorrecto=1, dfecha_salida=GETDATE()  WHERE vchuuid = '" + UUID + "'  AND iid = " + IID;
            return conexion.InsertaSql(sql);
        }


        public string genSelloPac(string cadena) {
            /*WebReferenceTimbre.FuncionesPlataformas Fntimbradopac = new WebReferenceTimbre.FuncionesPlataformas();
            string sello = Fntimbradopac.GenSelloPac(cadena);
            return sello;*/
            try
            {
                string result = "";
                //string CertificadoSelloDigital = ConfigurationManager.AppSettings["CertificadoSelloDigital"];
                //string RutaArchivoCer = "c://PAC32/pac.cer";/////////////////////////////////////////////////////////////////////////////
                string RutaArchivoKey = "c://PAC/20001000000100001695.key";//ConfigurationManager.AppSettings["Key"];/////////////////////////////////////////////////////////////////////////////
                string Contrasena = "12345678a";//ConfigurationManager.AppSettings["Clave"];/////////////////////////////////////////////////////////////////////////////

                Chilkat.PrivateKey llave = new PrivateKey();
                Chilkat.Rsa algoritmoRSA = new Rsa();
                llave.LoadPkcs8EncryptedFile(RutaArchivoKey, Contrasena);
                string keyPM = llave.GetXml();
                algoritmoRSA.ImportPrivateKey(keyPM);
                algoritmoRSA.LittleEndian = false;
                algoritmoRSA.Charset = "utf-8";
                algoritmoRSA.EncodingMode = "base64";

                bool numeroSerie1 = false; //RSAT34MB34N_7F1CD986683M
                bool numeroSerie2 = false; //RSAT34MB34N_2637664B634J
                bool numeroSerie3 = false; //RSAT34MB34N_3F0D2D9C642S
                bool numeroSerie4 = false; //RSAT34MB34N_7A2D7D1A680G
                bool numeroSerie5 = false; //RSAT34MB34N_7F1CD986683M

                //algoritmoRSA.UnlockComponent("RSAT34MB34N_2637664B 634J");
                //string xmltoText = fnObtenxml(RutaXML);
                //string CadenaOriginal = fnCadenaOriginal(xmltoText);
                //CadenaOriginal = "||3.0|2012-03-08T13:23:18|ingreso|Pago en una sola exhibición|38.5208|USD|44.6841|ABC010203ABC|Juan y Asociados|Hidalgo|1589|A|Centro|BENITO JUAREZ|Quintana Roo|México|47789|TEQUILA|102|VALLARTA PTE|GUADALAJARA|Jalisco|Mexico|44110|IDE110221ID8|ITROL DEVELOPMENT SA DE CV|TEQUILA|102|VALLARTA PONIENTE|GUADALAJARA|Jalisco|México|44110|1|PZ|CAMARA|38.5208|38.5208|IVA|16.00|6.1633|6.1633||";
                string cadenaOriginalFormateada = cadena;

                if (numeroSerie1 = algoritmoRSA.UnlockComponent("RSAT34MB34N_7F1CD986683M"))
                {
                    //cadenaOriginalFormateada = CadenaOriginal.ToString().Replace(System.Environment.NewLine, string.Empty).Replace("\t", string.Empty);
                    result = algoritmoRSA.SignStringENC(cadenaOriginalFormateada, "sha1");
                    //result = algoritmoRSA.SignStringENC(cadenaOriginalFormateada, "sha1");
                }
                else if (numeroSerie2 = algoritmoRSA.UnlockComponent("RSAT34MB34N_2637664B634J"))
                {
                    result = algoritmoRSA.SignStringENC(cadenaOriginalFormateada, "sha1");
                }
                else if (numeroSerie3 = algoritmoRSA.UnlockComponent("RSAT34MB34N_3F0D2D9C642S"))
                {
                    result = algoritmoRSA.SignStringENC(cadenaOriginalFormateada, "sha1");
                }
                else if (numeroSerie4 = algoritmoRSA.UnlockComponent("RSAT34MB34N_7A2D7D1A680G"))
                {
                    result = algoritmoRSA.SignStringENC(cadenaOriginalFormateada, "sha1");
                }
                else if (numeroSerie5 = algoritmoRSA.UnlockComponent("RSAT34MB34N_7F1CD986683M"))
                {
                    result = algoritmoRSA.SignStringENC(cadenaOriginalFormateada, "sha1");
                }
                else
                {
                    result = "";
                }

                result = result.Replace(System.Environment.NewLine, string.Empty).Replace("\t", string.Empty);

                return result;
            }
            catch (Exception exc)
            {
                //MessageBox.Show(exc.Message);
                return "";
            }
        }
        
        //SELLO PAC--------------------------------------
        public string CreaSelloPAC(string UUID,string numCerPac,string sello, string FechaIngresoTimbrado){
            string CadenaOriginalPAC = "||1.0|"+ UUID +"|"+ FechaIngresoTimbrado +"|"+ sello +"|"+ numCerPac +"||";
            //----webservice LUNA
            return genSelloPac(CadenaOriginalPAC);
        }
        //codigos de errores------------------------------------------------------------------------------------

        public bool ValidaError401(string fecha)
        {
            string v = fecha.Replace("T", " ");
            string sql = "SELECT DATEDIFF(HOUR,'"+v+"',GETDATE() ) horas";
            DataTable data = new DataTable();
            data = webservFacturas.conexion.conector.Consultasql(sql);
            int horas = Convert.ToInt32(data.Rows[0]["horas"].ToString());
            if (horas > 72)
                return false;
            else
                return true;
        }
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
        public bool ValidaError307(string CO)
        {
            string sql = "SELECT * FROM  TmpSol_Intentos_timbres where cast(vchCadenaOriginal as NVARCHAR(MAX)) = '" + CO + "' AND  iCorrecto = 1 AND vchcodError is null ";
            int cantidad = 0;

            DataTable dt = new DataTable();
            dt = webservFacturas.conexion.conector.Consultasql(sql);
            cantidad = Convert.ToInt16(dt.Rows.Count.ToString());
            if (cantidad == 0)
                return true;
            else return false;

        }
        public bool ValidaError305(string certificado, string RFC, string fechacreado)
        {
            string sql = "SELECT vchstatus FROM CatLco2 WHERE vchrfc = '" + RFC + "' AND vchnumcer = '" + certificado + "' AND dfechainicio < '" + fechacreado + "' AND dfechafin >'" + fechacreado + "'";
            int cantidad = 0;

            DataTable dt = new DataTable();
            dt = webservFacturas.conexion.conector.Consultasql(sql);
            cantidad = Convert.ToInt16(dt.Rows.Count.ToString());
            if (cantidad == 0)
                return false;
            else return true;

        }
       
        public bool Valida302(string xmltoText, string CadenaOriginal)
        {
            X509Certificate2 x509;

            //string xmltoText = fnObtenxml(Comprobante);
            
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(xmltoText);
            System.Xml.XmlNode element = doc.SelectSingleNode("/Emisor/DomicilioFiscal");
            XmlNodeList elemListEmisor = doc.GetElementsByTagName("cfdi:Comprobante");
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
                    if (extension.Oid.FriendlyName == "Uso de la clave")
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


        public bool Valida308(string xml, string CadenaOriginal)
        {
            X509Certificate2 x509;

            
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(xml);
            System.Xml.XmlNode element = doc.SelectSingleNode("/Emisor/DomicilioFiscal");
            XmlNodeList elemListEmisor = doc.GetElementsByTagName("cfdi:Comprobante");
            string Sello = elemListEmisor[0].Attributes["sello"].Value;
            string Certificado = elemListEmisor[0].Attributes["certificado"].Value;

            byte[] sello = Convert.FromBase64String(Sello);
            byte[] certificado = Encoding.UTF8.GetBytes(Certificado);
            byte[] cadenaOriginal = Encoding.UTF8.GetBytes(CadenaOriginal);

            x509 = new X509Certificate2(certificado);
            string CertificadosBanxico = @"C:\CFDIS\certificadosSAT\";

            return true;//validaCA(x509.GetRawCertData(), CertificadosBanxico);
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
        public void Inserta_LogAcceso(string xml, string UUID, string msgadd)
        {
            webservFacturas.conexion.conector conexion = new webservFacturas.conexion.conector();



            string sql = "" +
                "INSERT INTO tmpwebservice " +
                "(dfecha, vchLocation, vchUsuario, vchMensaje )" +
                "VALUES" +
                "(GETDATE(), 'wsv_timbrado','" + UUID + "','-" + xml + "')";
            conexion.InsertaSql(sql);
            //obtenemo el Id insertado

        }


        public bool validaRFC(string campo)
        {
            //if (Regex.IsMatch(campo, @"^([&A-Z\s][&]{4})\d{6}([A-Z\w]{3})$"))
            if (Regex.IsMatch(campo, @"^(([&A-Z]|[&a-z]){3})([0-9]{6})((([A-Z]|[a-z]|[0-9]){3}))$"))
            {

                return true;
            }
            else
            {
                //if (Regex.IsMatch(campo, @"^([&A-Z\s]{3})\d{6}([A-Z\w]{3})$"))
                if (Regex.IsMatch(campo, @"^(([&A-Z]|[&a-z]|\s){1})(([A-Z]|[a-z]){3})([0-9]{6})((([A-Z]|[a-z]|[0-9]){3}))$"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}