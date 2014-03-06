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
    ///<summary>
    /// Webservice para la validación de CFD y CFDI, en sus versiones 2.0, 2.2, 3.0 y 3.2.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // Para permitir que se llame a este servicio Web desde un script, usando ASP.NET AJAX, quite la marca de comentario de la línea siguiente. 
    // [System.Web.Script.Services.ScriptService]
    public class Validador : System.Web.Services.WebService
    {
        //Declaramos la Ruta del Esquema
        //private string rutaEsquema = "C://xslt/";
        //Iniciamos el WebMethod "ValidarFactura"
        [WebMethod(Description = "Función para validar un CFD o CFDI por medio de un ID de Acceso.")]
        public string ValidaCFDI(string xml, string idacceso, string comentarios)
        {
            //Inicializamos la variable versión.
            //string version = "";
			string versionp = "3.2";
            string addendap = "";
            //Declaramos las funciones
            webservFacturas.funciones.funciones Funciones = new webservFacturas.funciones.funciones();
            //Insertamos en la Base de Datos un Registro de Acceso
            Funciones.Inserta_LogAcceso(xml, idacceso, comentarios);
            //Verificamos si el Acceso es Correcto
            if (Funciones.isIDLogCorrect(idacceso))
            {
                //Se genera el UUID para identificar el acceso.
                string UUID = Funciones.GetUUID();
                //Si el acceso es correcto
                try
                {
                    //Creamos el XmlDocument.
                    XmlDocument doc = new XmlDocument();
                    //Cargamos el XML
                    doc.LoadXml(xml);
                }
                catch
                {
                    //Si no se pudo carga el XML, mandamos el codigo de error 301, XML con una estructura invalida
                    string msg = "(301) El CFDI no tiene una estructura XML correcta. - No es un archivo XML valido.";
                    Funciones.SaveErrorLog(UUID, "301", msg, "");
                    return msg;
                }
                try
                {
                    //Se verifica que versión es.
                    XmlDocument doc1 = new XmlDocument();
                    doc1.LoadXml(xml);
                    XmlElement CFD = doc1.DocumentElement;
                    //Definimos el Nodo "Comprobante".
                    //Leemos el Nodo Comprobante, correspondiente a la versión 2.0 y 2.2
                    XmlNodeList elemList_x = doc1.GetElementsByTagName("Comprobante");
                    for (int i = 0; i < elemList_x.Count; i++)
                    {
                        //Asignamos el nodo de la versión a una variable.
                        versionp = elemList_x[i].Attributes["version"].Value;
                    }
                    XmlNodeList Addenda = doc1.GetElementsByTagName("Addenda");
                    for (int i = 0; i < Addenda.Count; i++)
                    {
                        //Asignamos el nodo de la versión a una variable.
                        addendap = Addenda[i].Value;
                    }
                    //Si la variable versión esta vacia.
                    if (string.IsNullOrEmpty(versionp))
                    {
                        //Leemos desde el nodo "cfdi:Comprobante", correspondiente a la versión 3.0 y 3.2
                        XmlNodeList elemList_z = doc1.GetElementsByTagName("cfdi:Comprobante");
                        for (int i = 0; i < elemList_z.Count; i++)
                        {
                            //Asignamos el nodo de la versión a una variable.
                            versionp = elemList_z[i].Attributes["version"].Value;
                        }
                        XmlNodeList Addenda2 = doc1.GetElementsByTagName("Addenda");
                        for (int i = 0; i < Addenda2.Count; i++)
                        {
                            //Asignamos el nodo de la versión a una variable.
                            addendap = Addenda2[i].Value;
                            ;
                        }
                    }
                }
                catch
                { 
                    //Si no se pudo leer el nodo versión del XML, mandamos el codigo de error 301, XML con una estructura invalida en el atributo versión.
                    string msg = "(301) El CFDI no tiene una estructura XML correcta. Falta el atributo 'versión' necesario.";
                    Funciones.SaveErrorLog(UUID, "301", msg, "");
                    return msg;
                }
                //Insertamos la nueva petición de validación.
               // string IID = Funciones.Inserta_FirstPeticion(xml, UUID, idacceso, comentarios, versionp);
                string IID = "1";
                //Validación contra Esquema.
                if (addendap == "")
                {
                    if (!Funciones.ValidandoSkemaNew(xml))
                    {
                        //Mandamos el codigo de error 301, XML con una estructura invalida
                        string msjerror = "";
                        msjerror = Funciones.ValidandoSkema2(xml, versionp);
                        string msg = "(301) El CFDI no tiene una estructura XML correcta. - " + msjerror + "";
                        Funciones.SaveErrorLog(UUID, "301", msg, IID);
                        return msg;
                    }
                }
                //Si la versión del CFD es "2.0" o "2.2".

                if ((versionp == "2.0") || (versionp == "2.2"))
                {
                    
                    //Cargamos el XML .
                    XmlDocument doc2 = new XmlDocument();
                    doc2.LoadXml(xml);
                    //Creamos las variables donde se almacenara la información.
                    string fechacreado = "";
                    string RFC = "";
                    string noCertificado_exipide = "";
                    string certificado_xml = "";
                    string sello = "";
                    string version = "";
                    string RFC_Receptor = "";
                    string serie = "";
                    string folio = "";
                    XmlElement Comprobante = doc2.DocumentElement;
                    //Leemos el Nodo "Comprobante"
                    XmlNodeList elemList = doc2.GetElementsByTagName("Comprobante");
                    for (int i = 0; i < elemList.Count; i++)
                    {
                        //Leemos los atributos y los asignamos a variables.
                        fechacreado = elemList[i].Attributes["fecha"].Value;
                        noCertificado_exipide = elemList[i].Attributes["noCertificado"].Value;
                        certificado_xml = elemList[i].Attributes["certificado"].Value;
                        sello = elemList[i].Attributes["sello"].Value;
                        try
                        {
                            version = elemList[i].Attributes["version"].Value;
                        }
                        catch
                        {
                            string msg = "(301) El CFDI no tiene una estructura XML correcta. - Falta Atributo version.";
                            Funciones.SaveErrorLog(UUID, "301", msg, "");
                            return msg;
                        }
                        //En el caso de los CFD leemos la serie y el folio.
                        serie = elemList[i].Attributes["serie"].Value;
                        folio = elemList[i].Attributes["folio"].Value;
                    }
                    //Leemos el Nodo Emisor.
                    XmlNodeList EmisorList = doc2.GetElementsByTagName("Emisor");
                    for (int i = 0; i < EmisorList.Count; i++)
                    {
                        RFC = EmisorList[i].Attributes["rfc"].Value;
                    }
                    //Leemos el Nodo Receptor.
                    XmlNodeList Receptor = doc2.GetElementsByTagName("Receptor");
                    for (int i = 0; i < Receptor.Count; i++)
                    {
                        RFC_Receptor = Receptor[i].Attributes["rfc"].Value;
                    }
                    //Leemos los elemetos de domicilio.
                    XmlNodeList ListaDomRecep = ((XmlElement)Comprobante).GetElementsByTagName("Domicilio");
                    string noExterior_domRecep = "";
                    string municipio_domRecep = "";
                    foreach (XmlElement Elementos in ListaDomRecep)
                    {
                        noExterior_domRecep = Elementos.GetAttribute("noExterior");
                        municipio_domRecep = Elementos.GetAttribute("municipio");
                    }

                    //Validamos el formato del RFC.
                    if (!Funciones.validaRFC(RFC_Receptor))
                    {
                        //Si no es valido, regresamos un codigo 301.
                        string msg = "(301) El CFDI no tiene una estructura XML correcta. - El Formato del RFC no es valido.";
                        Funciones.SaveErrorLog(UUID, "301", msg, IID);
                        return msg;
                    }
                    //Obtenemos la cadena Original del XML.
                    string CO = Funciones.GetCadenaOrignal_byxml(xml, version).Trim();
                    if (CO == "")
                    {
                        //Si la cadena original es vacia, regresamos un 301
                        string msg = "(301) El CFDI no tiene una estructura XML correcta. - La Cadena Original no es Valida.";
                        Funciones.SaveErrorLog(UUID, "301", msg, IID);
                        return msg;
                    }

                    //Guardamos la cadena original, para su posterior uso.
                    Funciones.SaveCO(UUID, CO, IID);
                    //Convertimos el certificado
                    string certificado_xml_convertido = Funciones.ConvertCertificado(certificado_xml);
                    if (certificado_xml_convertido == "error")
                    {
                        string msg = "(303) El Certificado de Sello Digital no es valido o esta en blanco.";
                        Funciones.SaveErrorLog(UUID, "303", msg, IID);
                        return msg; 
                    }
                    
                    //Obtenemos su estado en la LCO
                    string status = Funciones.GetStatusLco(certificado_xml_convertido, RFC);
                    //Si la consulta se regresa vacia.
                    if (status == "")
                    {
                        //Si el Estado es vacío, regresamos un código 303
                        string msg = "(303) El Certificado de Sello Digital no corresponde al contribuyente emisor.";
                        Funciones.SaveErrorLog(UUID, "303", msg, IID);
                        return msg;
                    }
                    else
                    {
                        if (!Funciones.ValidaError305(certificado_xml_convertido, RFC, fechacreado))   
                        {
                            //Validamos la fecha de Emisión del comprobante, si esta fuera del rango de validez del certificado, regresamos un código de error 305
                            string msg = "(305) La fecha del CFDI está fuera del rango de la validez del certificado.";
                            Funciones.SaveErrorLog(UUID, "305", msg, IID);
                            return msg;
                        }
                        else
                        {
                            if (!Funciones.Valida302(xml, CO,version))
                            {
                                //Validamos el Sello del Emisor, apartir de la Cadena Original, si el sello es erroneo, regresamos el código de error 302
                                string msg = "(302) El sello del emisor no es válido.";
                                Funciones.SaveErrorLog(UUID, "302", msg, IID);
                                return msg;
                            }
                            else
                            {
                                if (!Funciones.Valida306(certificado_xml))
                                {
                                    //Validamos que el certificado corresponda a un CSD y no a una FIEL, si no, regresamos el código de error 306
                                    string msg = "(306) El certificado usado para generar el sello digital no es un Certificado de Sello Digital.";
                                    Funciones.SaveErrorLog(UUID, "306", msg, IID);
                                    return msg;
                                }
                                else
                                {
                                    if (!Funciones.Valida308(xml, CO, version))
                                    {
                                        //Validamos el Certificado, que fuera emitido por el SAT, si no, regresamos un 308
                                        string msg = "(308) El certificado utilizado para generar el sello digital no ha sido emitido por el SAT.";
                                        Funciones.SaveErrorLog(UUID, "308", msg, IID);
                                        return msg;
                                    }
                                    else
                                    {
                                        if (!Funciones.Valida311(serie, RFC,folio))
                                        {
                                            //Validamos el Certificado, que fuera emitido por el SAT, si no, regresamos un 308
                                            string msg = "(311) La Serie y/o el Folio no estan autorizados por el SAT.";
                                            Funciones.SaveErrorLog(UUID, "311", msg, IID);
                                            return msg;
                                        }
                                        else
                                        { 
                                            return "(100) CFDI valido.";
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    //Cargamos el XML.
                    XmlDocument doc2 = new XmlDocument();
                    doc2.LoadXml(xml);
                    //Creamos las variables donde se almacenara la información.
                    string fechacreado = "";
                    string RFC = "";
                    string noCertificado_exipide = "";
                    string certificado_xml = "";
                    string sello = "";
                    string version = "";
                    string RFC_Receptor = "";
                    XmlElement Comprobante = doc2.DocumentElement;
                    //Leemos el Nodo "Comprobante".
                    XmlNodeList elemList = doc2.GetElementsByTagName("cfdi:Comprobante");
                    for (int i = 0; i < elemList.Count; i++)
                    {
                        //Asignamos los atributos a variables.
                        fechacreado = elemList[i].Attributes["fecha"].Value;
                        noCertificado_exipide = elemList[i].Attributes["noCertificado"].Value;
                        certificado_xml = elemList[i].Attributes["certificado"].Value;
                        sello = elemList[i].Attributes["sello"].Value;
                        try
                        {
                            version = elemList[i].Attributes["version"].Value;
                        }
                        catch
                        {
                            string msg = "(301) El CFDI no tiene una estructura XML correcta. - Falta Atributo version.";
                            Funciones.SaveErrorLog(UUID, "301", msg, "");
                            return msg;
                        }
                    }
                    //Leemos el Nodo Emisor.
                    XmlNodeList EmisorList = doc2.GetElementsByTagName("cfdi:Emisor");
                    for (int i = 0; i < EmisorList.Count; i++)
                    {
                        RFC = EmisorList[i].Attributes["rfc"].Value;
                    }
                    //Leemos el Nodo Receptor
                    XmlNodeList Receptor = doc2.GetElementsByTagName("cfdi:Receptor");
                    for (int i = 0; i < Receptor.Count; i++)
                    {
                        RFC_Receptor = Receptor[i].Attributes["rfc"].Value;
                    }
                    //Leemos los elementos de domicilio.
                    XmlNodeList ListaDomRecep = ((XmlElement)Comprobante).GetElementsByTagName("Domicilio");
                    string noExterior_domRecep = "";
                    string municipio_domRecep = "";
                    foreach (XmlElement Elementos in ListaDomRecep)
                    {
                        noExterior_domRecep = Elementos.GetAttribute("noExterior");
                        municipio_domRecep = Elementos.GetAttribute("municipio");
                    }
                    //Validamos el XML contra esquema TFD.
                    if (!Funciones.ValidandoSkemaTFD(xml))
                    {
                        string msg = "(301) El CFDI no tiene una estructura XML correcta.";
                        Funciones.SaveErrorLog(UUID, "301", msg, IID);
                        return msg;
                    }
                    //Validamos el formato del RFC.
                    if (!Funciones.validaRFC(RFC_Receptor))
                    {
                        //Si no es valido, regresamos un codigo 301.
                        string msg = "(301) El CFDI no tiene una estructura XML correcta.- El Formato del RFC no es valido.";
                        Funciones.SaveErrorLog(UUID, "301", msg, IID);
                        return msg;
                    }
                    //Obtenemos la cadena Original del XML.
                    string CO = Funciones.GetCadenaOrignal_byxml(xml, version).Trim();
                    //string COTFD = Funciones.GetCadenaOrignalTFD(xml).Trim();
                    if (CO == "")
                    {
                        //Si la cadena original es vacia, regresamos un 301.
                        string msg = "(301) El CFDI no tiene una estructura XML correcta. - La Cadena Original no es Valida.";
                        Funciones.SaveErrorLog(UUID, "301", msg, IID);
                        return msg;
                    }
                    //Guardamos la cadena original, para su posterior uso.
                    Funciones.SaveCO(UUID, CO, IID);
                    //Validamos si la fecha de Emisión sea posterior al 1 de Enero del 2011 para CFDI's.
                    if (!Funciones.ValidaError403(fechacreado))
                    {
                        //Si no es así, regresamos un código 403.
                        string msg = "(403) La fecha de emisión del CFDI no puede ser anterior al 1 de enero de 2011.";
                        Funciones.SaveErrorLog(UUID, "403", msg, IID);
                        return msg;
                    }
                    else
                    {
                        //Convertimos el certificado.
                        string certificado_xml_convertido = Funciones.ConvertCertificado(certificado_xml);
                        if (certificado_xml_convertido == "error")
                        {
                            string msg = "(303) El Certificado de Sello Digital no es valido o esta en blanco.";
                            Funciones.SaveErrorLog(UUID, "303", msg, IID);
                            return msg;
                        }
                        //Obtenemos su estado en la LCO.
                        string status = Funciones.GetStatusLco(certificado_xml_convertido, RFC);
                        if (status == "")
                        {
                            //Si el Estado es vacío, regresamos un código 303
                            string msg = "(303) El Certificado de Sello Digital no corresponde al contribuyente emisor.";
                            Funciones.SaveErrorLog(UUID, "303", msg, IID);
                            return msg;
                        }
                        else
                        {
                            if (status == "C")
                            {
                                //Si el Estado es "C", el contribuyente ya no esta activo, el certificado esta "Cancelado", regresamos un código de error 402.
                                string msg = "(402) El contribuyente no se encuentra dentro del régimen fiscal para emitir CFDI.";
                                Funciones.SaveErrorLog(UUID, "402", msg, IID);
                                return msg;
                            }
                            else
                            {
                                if (status == "R")
                                {
                                    //Si el Estado es "R", al contribuyente le ha sido revocado el certificado, regresamos un código de error 402.
                                    string msg = "(304) El certificado se encuentra revocado o caduco.";
                                    Funciones.SaveErrorLog(UUID, "304", msg, IID);
                                    return msg;
                                }
                                else
                                {
                                    if (!Funciones.ValidaError305(certificado_xml_convertido, RFC, fechacreado))
                                    {
                                        //Validamos la fecha de Emisión del comprobante, si esta fuera del rango de validez del certificado, regresamos un código de error 305
                                        string msg = "(305) La fecha del CFDI está fuera del rango de la validez del certificado.";
                                        Funciones.SaveErrorLog(UUID, "305", msg, IID);
                                        return msg;
                                    }
                                    else
                                    {
                                        if (!Funciones.Valida302(xml, CO,version))
                                        {
                                            //Validamos el Sello del Emisor, apartir de la Cadena Original, si el sello es erroneo, regresamos el código de error 302
                                            string msg = "(302) El sello del emisor no es válido.";
                                            Funciones.SaveErrorLog(UUID, "302", msg, IID);
                                            return msg;
                                        }
                                        else
                                        {
                                            if (!Funciones.Valida306(certificado_xml))
                                            {
                                                //Validamos que el certificado corresponda a un CSD y no a una FIEL, si no, regresamos el código de error 306
                                                string msg = "(306) El certificado usado para generar el sello digital no es un Certificado de Sello Digital.";
                                                Funciones.SaveErrorLog(UUID, "306", msg, IID);
                                                return msg;
                                            }
                                            else
                                            {
                                                if (!Funciones.Valida308(xml, CO,version))
                                                {
                                                    //Validamos el Certificado, que fuera emitido por el SAT, si no, regresamos un 308
                                                    string msg = "(308) El certificado utilizado para generar el sello digital no ha sido emitido por el SAT.";
                                                    Funciones.SaveErrorLog(UUID, "308", msg, IID);
                                                    return msg;
                                                }
                                                else
                                                {
                                                    /*  if (!Funciones.Valida307(xml, CO))
                                                      {
                                                          //Validamos el Certificado, que fuera emitido por el SAT, si no, regresamos un 308
                                                          string msg = "(307) El Sello del SAT, no es Valido.";
                                                          Funciones.SaveErrorLog(UUID, "307", msg, IID);
                                                          return msg;
                                                      }
                                                      else
                                                      { */
                                                    return "(100) CFDI valido.";
                                                }
                                                //}
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                return "(200) Han ocurrido errores que no han permitido completar el proceso de validación.";
            }
        }
    }
}