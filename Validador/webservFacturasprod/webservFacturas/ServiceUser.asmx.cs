using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Schema;
using System.IO;

namespace webservFacturas
{
    /// <summary>
    /// Descripción breve de Service1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // Para permitir que se llame a este servicio Web desde un script, usando ASP.NET AJAX, quite la marca de comentario de la línea siguiente. 
    // [System.Web.Script.Services.ScriptService]
    public class Service1 : System.Web.Services.WebService
    {
        private string numCerPac = "30001000000100000801";
        private string rutaEsquema = "";
        
       
        
        [WebMethod]
        public ClsRespuesta GetSelloFiscalDigitalUser(string xml, string usuario, string clave, string comentarios)
        {
            webservFacturas.funciones.funciones Funciones = new webservFacturas.funciones.funciones();

           //obtenemos el iidacceso
            string idacceso = Funciones.getiidAcceso(usuario, clave);

            
            ///insertamos el registro de la peticion.
            ///
            Funciones.Inserta_LogAcceso(xml, idacceso, comentarios);

            if (Funciones.isIDLogCorrect(idacceso))
            {
                ///veo que tipo de cliente es
                if (!Funciones.validaTipoCliente(idacceso)) {
                    string msg = "Lo Sentimos ya no cuentas con Facturas disponibles.";
                    //Funciones.SaveErrorLog(UUID, "301", msg);
                    //return msg; 
                    return new ClsRespuesta { correcto = 0, Mensaje = msg };
                }
                int resp = xml.IndexOf("tfd:TimbreFiscalDigital");
                if (resp != -1) {
                    string msg = "El xml que intentas enviar contiene un timbre.";
                    //return msg;
                    return new ClsRespuesta { correcto = 0, Mensaje = msg };
                }
                //GNERO EL UUID
                string UUID = Funciones.GetUUID();
                //es correcto su acceso


                XmlDocument doc = new XmlDocument();

                try
                {
                    // Create the XmlDocument.
                    doc.LoadXml(xml);
                }
                catch
                {
                    string msg = "(301) XML mal formado.";
                    Funciones.SaveErrorLog(UUID, "301", msg, "");
		            //return msg;
                    return new ClsRespuesta { correcto = 0, Mensaje = msg };
                }
                //version------------------
                string version = "";
                XmlNodeList elemList_x = doc.GetElementsByTagName("cfdi:Comprobante");
                for (int i = 0; i < elemList_x.Count; i++)
                {
                    version = elemList_x[i].Attributes["version"].Value;
                }
                if (version == "3.0") { rutaEsquema = "C://xslt/cfdv3.xsd"; } else { if (version == "3.2") { rutaEsquema = "C://xslt/cfdv32.xsd"; } }


                //inserto nueva peticion timbrado
                string IID = Funciones.Inserta_FirstPeticion(xml, UUID, idacceso, comentarios);


                //validando esquema
                if (!Funciones.ValidandoSkema2(xml, rutaEsquema))
                {
                    string msg = "(301) XML mal formado.";
                    Funciones.SaveErrorLog(UUID, "301", msg, IID);
                    //return msg;
                    return new ClsRespuesta { correcto = 0, Mensaje = msg };
                }


                XmlDocument doc2 = new XmlDocument();
                doc2.LoadXml(xml);
                    //veo si su estructura del xml es correcta
                    //valido el esquema del xml
                    //301 Mal Formado
                    //bool variables = Funciones.ValidandoSkema(xml,rutaEsquema);
                    string fechacreado = "";
                    string RFC = "";
                    string RFC_Receptor = "";
                    string noCertificado_exipide = "";
                    string certificado_xml = "";
                    string sello = "";

                    XmlNodeList elemList = doc2.GetElementsByTagName("cfdi:Comprobante");
                    for (int i = 0; i < elemList.Count; i++)
                    {
                        fechacreado = elemList[i].Attributes["fecha"].Value;
			            noCertificado_exipide = elemList[i].Attributes["noCertificado"].Value;
                        certificado_xml = elemList[i].Attributes["certificado"].Value;
                        sello = elemList[i].Attributes["sello"].Value;
                        version = elemList[i].Attributes["version"].Value;
                        ///
                    }
                    XmlNodeList EmisorList = doc2.GetElementsByTagName("cfdi:Emisor");
                    for (int i = 0; i < EmisorList.Count; i++)
                    {
                        RFC = EmisorList[i].Attributes["rfc"].Value;
                    }

                ///
                    XmlNodeList Receptor = doc2.GetElementsByTagName("cfdi:Receptor");
                    for (int i = 0; i < Receptor.Count; i++)
                    {
                        RFC_Receptor = Receptor[i].Attributes["rfc"].Value;
                    }

                    string CO = Funciones.GetCadenaOrignal_byxml(xml, version);
                    if (CO == "") {
                        string msg = "(301) XML mal formado..";
                        Funciones.SaveErrorLog(UUID, "301", msg, IID);
                        //return msg;
                        return new ClsRespuesta { correcto = 0, Mensaje = msg };
                    }


                //////validador del RFC
                    if (!Funciones.validaRFC(RFC_Receptor))
                    {
                        string msg = "(301) XML mal formado..";
                        Funciones.SaveErrorLog(UUID, "301", msg, IID);
                        //return msg;
                        return new ClsRespuesta { correcto = 0, Mensaje = msg };
                    }


                    Funciones.SaveCO(UUID, CO, IID);

                    if (!Funciones.ValidaError403(fechacreado))
                    {
                        string msg = "(403) La fecha de emisión no es posterior al 01 de enero 2011.";
                        Funciones.SaveErrorLog(UUID, "403", msg, IID);
                        //return msg;
                        return new ClsRespuesta { correcto = 0, Mensaje = msg };
                    }else{
                        if (!Funciones.ValidaError401(fechacreado))
                        {
                            string msg = "(401) Fecha y hora de generación fuera de rango";
                            Funciones.SaveErrorLog(UUID, "401", msg, IID);
                            //return msg;
                            return new ClsRespuesta { correcto = 0, Mensaje = msg };
                        }
                        else
                        {
                            if (!Funciones.ValidaError307(CO))
                            {
                                string msg = "(307) El CFDI contiene un timbre previo.";
                                Funciones.SaveErrorLog(UUID, "307", msg, IID);
                                //return msg;
                                return new ClsRespuesta { correcto = 0, Mensaje = msg };
                            }
                            else
                            {
                                string certificado_xml_convertido = Funciones.ConvertCertificado(certificado_xml);
                                //obtengo el estatus de la lco
                                string status = Funciones.GetStatusLco(certificado_xml_convertido,RFC);
                                if (status == "")
                                {
                                    string msg = "(303) Sello no corresponde a emisor o caduco.";
                                    Funciones.SaveErrorLog(UUID, "307", msg, IID);
                                    //return msg;
                                    return new ClsRespuesta { correcto = 0, Mensaje = msg };
                                }
                                else {
                                    if (status == "C")
                                    {
                                        string msg = "(402) RFC del emisor no se encuentra en el regimen de contribuyentes.";
                                        Funciones.SaveErrorLog(UUID, "402", msg, IID);
                                        //return msg;
                                        return new ClsRespuesta { correcto = 0, Mensaje = msg };
                                    }
                                    else {
                                        if (status == "R")
                                        {
                                            string msg = "(304) Certificado revocado o caduco.";
                                            Funciones.SaveErrorLog(UUID, "304", msg, IID);
                                            //return msg;
                                            return new ClsRespuesta { correcto = 0, Mensaje = msg };
                                        }
                                        else {
                                            if (!Funciones.ValidaError305(certificado_xml_convertido, RFC, fechacreado))
                                            {//false no existe
                                                string msg = "(305) La fecha de emision no esta dentro de la vigencia del CSD del Emisor.";
                                                Funciones.SaveErrorLog(UUID, "305", msg, IID);
                                                //return msg;
                                                return new ClsRespuesta { correcto = 0, Mensaje = msg };
                                            }
                                            else {
                                                if(!Funciones.Valida302(xml,CO)){
                                                    string msg = "(302) Sello mal formado o inválido";
                                                    Funciones.SaveErrorLog(UUID, "302", msg, IID);
                                                    //return msg;
                                                    return new ClsRespuesta { correcto = 0, Mensaje = msg };
                                                }else{
                                                    if (!Funciones.Valida306(certificado_xml))
                                                    {
                                                        string msg = "(306) EL certificado no es de tipo CSD";
                                                        Funciones.SaveErrorLog(UUID, "306", msg, IID);
                                                        //return msg;
                                                        return new ClsRespuesta { correcto = 0, Mensaje = msg };
                                                    }
                                                    else
                                                    {
                                                        if (!Funciones.Valida308(xml, CO))
                                                        {
                                                            string msg = "(308) Certificado no expedido por el SAT";
                                                            Funciones.SaveErrorLog(UUID, "308", msg, IID);
                                                            //return msg;
                                                            return new ClsRespuesta { correcto = 0, Mensaje = msg };
                                                        }
                                                        else
                                                        {
                                                            string FechaIngresoTimbrado = Funciones.GetFechaIngresoTimbreado(UUID, idacceso, IID);
                                                            //creo el sello del pac

                                                            


                                                            string SelloPAC = Funciones.CreaSelloPAC(UUID, numCerPac, sello, FechaIngresoTimbrado);
                                                            //Ingreso los nuevos nodos a ese XML
                                                            //XmlNode Complemento = doc2.CreateNode(XmlNodeType.Element, "cfdi:Complemento", null);




                                                            //XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc2.NameTable);

                                                            //XmlNode book = doc2.SelectSingleNode("//Complemento", nsmgr);

                                                            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc2.NameTable);
                                                            nsmgr.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/3");
                                                            XmlNode book = doc2.SelectSingleNode("//cfdi:Complemento", nsmgr);


                                                            
                                                            //XmlNode node2 = doc2.SelectSingleNode("//cfdi:Comprobante//cfdi:Complemento");

                                                            if (book == null)
                                                            {

                                                                XmlNode Complemento = doc2.CreateNode(XmlNodeType.Element, "cfdi", "Complemento", "http://www.sat.gob.mx/cfd/3");
                                                                //       ("cfdi","Complemento", null);
                                                                XmlElement root = doc2.DocumentElement;
                                                                root.AppendChild(Complemento);

                                                                XmlNode Timbre = doc2.CreateNode(XmlNodeType.Element, "tfd", "TimbreFiscalDigital", "http://www.sat.gob.mx/TimbreFiscalDigital");
                                                                Complemento.AppendChild(Timbre);

                                                                XmlAttribute fechtm = doc2.CreateAttribute("FechaTimbrado");
                                                                fechtm.Value = FechaIngresoTimbrado;
                                                                Timbre.Attributes.Append(fechtm);

                                                                XmlAttribute numcrt = doc2.CreateAttribute("noCertificadoSAT");
                                                                numcrt.Value = numCerPac;
                                                                Timbre.Attributes.Append(numcrt);

                                                                XmlAttribute vers = doc2.CreateAttribute("version");
                                                                vers.Value = "1.0";
                                                                Timbre.Attributes.Append(vers);

                                                                XmlAttribute sellcli = doc2.CreateAttribute("selloCFD");
                                                                sellcli.Value = sello;
                                                                Timbre.Attributes.Append(sellcli);

                                                                XmlAttribute sellsat = doc2.CreateAttribute("selloSAT");
                                                                sellsat.Value = SelloPAC;
                                                                Timbre.Attributes.Append(sellsat);

                                                                XmlAttribute uid = doc2.CreateAttribute("UUID");
                                                                uid.Value = UUID;
                                                                Timbre.Attributes.Append(uid);

                                                                XmlNamespaceManager nsmgr2 = new XmlNamespaceManager(doc2.NameTable);
                                                                XmlAttribute schemaLocation = doc2.CreateAttribute("xsi", "schemaLocation", nsmgr2.LookupPrefix("xsi"));
                                                                Timbre.Attributes.Append(schemaLocation);
                                                                schemaLocation.Value = "http://www.sat.gob.mx/TimbreFiscalDigital http://www.sat.gob.mx/sitio_internet/TimbreFiscalDigital/TimbreFiscalDigital.xsd";
                                                                Timbre.Attributes.Append(schemaLocation);
                                                            }
                                                            else {
                                                                XmlNode Timbre = doc2.CreateNode(XmlNodeType.Element, "tfd", "TimbreFiscalDigital", "http://www.sat.gob.mx/TimbreFiscalDigital");
                                                                book.AppendChild(Timbre);

                                                                XmlAttribute fechtm = doc2.CreateAttribute("FechaTimbrado");
                                                                fechtm.Value = FechaIngresoTimbrado;
                                                                Timbre.Attributes.Append(fechtm);

                                                                XmlAttribute numcrt = doc2.CreateAttribute("noCertificadoSAT");
                                                                numcrt.Value = numCerPac;
                                                                Timbre.Attributes.Append(numcrt);

                                                                XmlAttribute vers = doc2.CreateAttribute("version");
                                                                vers.Value = "1.0";
                                                                Timbre.Attributes.Append(vers);

                                                                XmlAttribute sellcli = doc2.CreateAttribute("selloCFD");
                                                                sellcli.Value = sello;
                                                                Timbre.Attributes.Append(sellcli);

                                                                XmlAttribute sellsat = doc2.CreateAttribute("selloSAT");
                                                                sellsat.Value = SelloPAC;
                                                                Timbre.Attributes.Append(sellsat);

                                                                XmlAttribute uid = doc2.CreateAttribute("UUID");
                                                                uid.Value = UUID;
                                                                Timbre.Attributes.Append(uid);

                                                                XmlNamespaceManager nsmgr2 = new XmlNamespaceManager(doc2.NameTable);
                                                                XmlAttribute schemaLocation = doc2.CreateAttribute("xsi", "schemaLocation", nsmgr2.LookupPrefix("xsi"));
                                                                Timbre.Attributes.Append(schemaLocation);
                                                                schemaLocation.Value = "http://www.sat.gob.mx/TimbreFiscalDigital http://www.sat.gob.mx/sitio_internet/TimbreFiscalDigital/TimbreFiscalDigital.xsd";
                                                                Timbre.Attributes.Append(schemaLocation);
                                                            }
                                                            


                                                            if (Funciones.GuardaCFDI(doc2.OuterXml, UUID, IID))
                                                            {
                                                                //le restro al cliente de prepago una existencia
                                                                Funciones.RestaExistenciaFacturaTimbre(idacceso);
                                                                string correctString = doc2.OuterXml.Replace(" schemaLocation", " xsi:schemaLocation");
                                                                //return correctString;
                                                                return new ClsRespuesta { correcto = 1, Mensaje = correctString };
                                                            }
                                                            else {
                                                                string bin =  "Problema al Guardar";
                                                                return new ClsRespuesta { correcto = 0, Mensaje = bin };
                                                            }

                                                            
                                                        }
                                                        

                                                    }

                                                    
                                                }
                                                
                                            }

                                            
                                        }
                                    }
                                }



                                
                            }
                            
                        }
                    }
                    

                    
                    /*if (resp) 
                        return "Correcto squema: ";
                    else
                        return "Erro esquema: ";
                     */
                    //busco si el uuid ya ha sido solicitado para timbrarse
                    
                
               

                
            }
            else {
                //incorrecto
                string ingo =  "Error, Acceso Denegado";
                return new ClsRespuesta { correcto = 0, Mensaje = ingo };
            }

            
        }
        [WebMethod]
        public ClsRespuesta CancelaFacturaElectronica(string idacceso, string UUID)
        {
            webservFacturas.funciones.funciones Funciones = new webservFacturas.funciones.funciones();
            webservFacturas.funciones.funcionescancela FuncionesC = new webservFacturas.funciones.funcionescancela();

           // Funciones.InsertaPeticionCanelacion(idacceso, UUID);


            if (!Funciones.isIDLogCorrect(idacceso))
            {
                //acceso incorrecto
                string returninf = "Error, Acceso Denegado";
                return new ClsRespuesta { correcto = 0, Mensaje = returninf };
            }
            else
            {
                ///veo si existe
                if (FuncionesC.ExisteFac(UUID, idacceso))
                {
                //vemos que ese UUID exista en la otra tabla y se haya enviado al sat
                if (FuncionesC.ExisteEnviadoalSat(UUID, idacceso))
                {
                    //vemos que no exista ya cancelado
                    if (FuncionesC.ExisteCancelado(UUID, idacceso))
                    {
                        string returninf = "Error, El Folio que intentas cancelar ya esta cancelado.";
                        return new ClsRespuesta { correcto = 0, Mensaje = returninf };
                    }
                    else
                    {

                        //guardamos el Intento.
                        bool respuesta = FuncionesC.Inserta_FirstPeticion(UUID, idacceso);

                        //verificamos que ese UUID exista
                        if (respuesta)
                        {
                            //no existe
                            string msg = "Cancelado Correctamente.";
                            //return msg;
                            return new ClsRespuesta { correcto = 1, Mensaje = msg };
                        }
                        else
                        {
                            string msg = "Problema al Cancelar, Intente nuevamente";
                            //return msg;
                            return new ClsRespuesta { correcto = 0, Mensaje = msg };
                        }
                    }
                }
                else {
                    string msg = "Problema al Cancelar, EL UUID que envias aun no se ha enviado al SAT";
                    //return msg;
                    return new ClsRespuesta { correcto = 0, Mensaje = msg };
                }
                }
                else
                {
                    string msg = "Problema al Cancelar, EL UUID que envias aun no existe";
                    //return msg;
                    return new ClsRespuesta { correcto = 0, Mensaje = msg };
                }
            }
        }
    }
}