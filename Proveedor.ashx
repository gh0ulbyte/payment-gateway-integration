<%@ WebHandler Language="C#" Class="Proveedor" %>

using System;
using System.Web;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Data;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;
using System.Web.Script.Serialization;
using System.Configuration;

public class Proveedor : IHttpHandler {

    private string Proveedor_Tasa;
    private string Proveedor_Cuenta;
    private string Proveedor_Razon;
    private string Proveedor_Recibo;
    private string Proveedor_Fecha_ToParse;
    private string Proveedor_Valor_ToParse;
    private string Proveedor_Mail;
    private string Proveedor_CPE;
    private string Proveedor_CBR;
    private string Proveedor_Origen;
    private string Proveedor_Window;
    private string Proveedor_Retorno;

    private DateTime Proveedor_Fecha;
    private double Proveedor_Valor;

    public void ProcessRequest (HttpContext context)
    {
        try
        {
            // Inicializo variable privadas con parametros QueryString 
            Proveedor_Tasa = context.Request.QueryString["tasa"];
           Proveedor_Cuenta = context.Request.QueryString["cuenta"];
            Proveedor_Razon = context.Request.QueryString["razon"];
            Proveedor_Recibo = context.Request.QueryString["recibo"];
            Proveedor_Fecha_ToParse = context.Request.QueryString["fecha"];
            Proveedor_Valor_ToParse = context.Request.QueryString["valor"];
            //if (Proveedor_Valor_ToParse.Contains("."))
            //    Proveedor_Valor_ToParse = Proveedor_Valor_ToParse.Replace(".", "");
            //Proveedor_Valor_ToParse = Proveedor_Valor_ToParse.Replace(",", ".");
            Proveedor_Mail  = context.Request.QueryString["mail"];
            Proveedor_CPE = context.Request.QueryString["cpe"];
            Proveedor_CBR = context.Request.QueryString["cbr"];
            Proveedor_Origen = context.Request.QueryString["origen"];
            Proveedor_Window = context.Request.QueryString["popwindow"];
            Proveedor_Retorno = context.Request.QueryString["retorno"].Replace("|","&");
            // Parseo variables segun tipo de dato Control de formato valido
            Proveedor_Fecha = DateTime.Parse(Proveedor_FechaToParse);
            Proveedor_Valor = double.Parse(Proveedor_ValorToParse);

            string data = SetDataPagoTIC();
            if (data != string.Empty)
            {
                context.Response.Write(data);
            }
            context.Response.End();
        }
        catch (Exception ex)
        {
            if (ex.Message != "Subproceso anulado.")
               throw new Exception(ex.Message);
        }
    }

    protected string SetDataProveedor()
    {
        string ProveedorToken = Guid.NewGuid().ToString();

        // Graba transaccion y token

        int result = int.Parse(new Generic_DAL.SQL().ExecuteScalar("base",
           Proveedor_Tasa,
           Proveedor_Cuenta,
           Proveedor_Razon,
           Proveedor_Recibo,
           Proveedor_CPE,
           Proveedor_CBR,
           Proveedor_Token,
           Proveedor_Fecha,
           Proveedor_Valor,
           Proveedor_Mail,
           Proveedor_Origen,
           Proveedor_Window,
           Proveedor_Retorno).ToString());


        StringBuilder json = new StringBuilder();
        json.Append("data : [ ");
        json.Append("{" + Environment.NewLine);
        json.Append("'token' : '");
        json.Append(ProveedorToken);
        json.Append("'" + Environment.NewLine);
        json.Append("}]");
        return json.ToString();
    }


    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}