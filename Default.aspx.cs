using System;
using System.Web;
using System.Web.UI;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;
using System.Configuration;

public partial class _Default : System.Web.UI.Page
{
    private readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            LogDebug("Iniciando procesamiento de pago");

            // Obtener token y opción de la URL
            string token = Request.QueryString["token"];
            string opcion = Request.QueryString["opcion"];

            LogDebug(String.Format("Token recibido: {0}", token));
            LogDebug(String.Format("Opción recibida: {0}", opcion));

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(opcion))
            {
                LogError("Token u opción no válidos");
                ShowError("Token u opción no válidos");
                return;
            }

            LogDebug("Llamando a PaymentService.CreateCheckout");
            
            try
            {
                // Crear checkout en Proveedor y redirigir
                string checkoutUrl = Proveedor.V1.PaymentService.CreateCheckout(token, opcion);
                LogDebug(String.Format("URL de checkout obtenida: {0}", checkoutUrl));
                Response.Redirect(checkoutUrl);
            }
            catch (Exception checkoutEx)
            {
                LogError(String.Format("Error en CreateCheckout: {0}", checkoutEx.Message));
                LogError(String.Format("StackTrace: {0}", checkoutEx.StackTrace));
                throw;
            }
        }
        catch (Exception ex)
        {
            LogError(String.Format("Error general: {0}", ex.Message));
            LogError(String.Format("StackTrace: {0}", ex.StackTrace));
            ShowError("Error al procesar el pago: " + ex.Message);
        }
    }

    private void ShowError(string message)
    {
        LogError(String.Format("Mostrando error: {0}", message));
        Response.Write("<h3 style='color: red; text-align: center; margin-top: 20px;'>" + Server.HtmlEncode(message) + "</h3>");
    }

    private void LogDebug(string message)
    {
        try
        {
            string logPath = Server.MapPath("~/App_Data/debug.log");
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string logMessage = String.Format("[{0}] {1}\r\n", timestamp, message);
            
            string directory = Path.GetDirectoryName(logPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            File.AppendAllText(logPath, logMessage);
        }
        catch (Exception ex)
        {
            // Si falla el logging principal, intentar un log alternativo
            try
            {
                string altLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "error.log");
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string logMessage = String.Format("[{0}] Error de logging: {1}\r\n", timestamp, ex.Message);
                File.AppendAllText(altLogPath, logMessage);
            }
            catch
            {
                // Si todo falla, no podemos hacer mucho más
            }
        }
    }

    private void LogError(string message)
    {
        try
        {
            string logPath = Server.MapPath("~/App_Data/error.log");
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string logMessage = String.Format("[{0}] ERROR: {1}\r\n", timestamp, message);
            
            string directory = Path.GetDirectoryName(logPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            File.AppendAllText(logPath, logMessage);
        }
        catch (Exception ex)
        {
            // Si falla el logging principal, intentar un log alternativo
            try
            {
                string altLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "error.log");
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string logMessage = String.Format("[{0}] Error de logging: {1}\r\n", timestamp, ex.Message);
                File.AppendAllText(altLogPath, logMessage);
            }
            catch
            {
                // Si todo falla, no podemos hacer mucho más
            }
        }
    }
} 