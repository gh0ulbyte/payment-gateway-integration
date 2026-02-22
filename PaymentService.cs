using System;
using System.Configuration;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Globalization;

namespace Proveedor.App.V1
{
    public class CheckoutRequest
    {
        public string collector_id { get; set; }
        public string currency_id { get; set; }
        public string external_transaction_id { get; set; }
        public string due_date { get; set; }
        public PresetOptions presets { get; set; }
        public List<TransactionDetail> details { get; set; }
        public Payer payer { get; set; }
        public string return_url { get; set; }
    }

    public class PresetOptions
    {
        public string type { get; set; }
    }

    public class TransactionDetail
    {
        public string external_reference { get; set; }
        public string concept_id { get; set; }
        public string concept_description { get; set; }
        public decimal amount { get; set; }
    }

    public class Payer
    {
        public string name { get; set; }
        public string email { get; set; }
        public Identification identification { get; set; }
    }

    public class Identification
    {
        public string type { get; set; }
        public string number { get; set; }
        public string country { get; set; }
    }

    public class PaymentService
    {
        private static readonly JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
        private static readonly string ProveedorBaseUrl;
        private static readonly string ProveedorAuthUrl;
        private static readonly string TokenEndpoint;
        private static readonly string CheckoutUrl;
        private static readonly string Username;
        private static readonly string Password;
        private static readonly string ClientId;
        private static readonly string ClientSecret;
        private static readonly string NotificationUrl;
        private static readonly string CollectorId;
        private static readonly string ConnectionString;
        private static readonly string ProvApiUrl;
        private static readonly string ProvApiKey;
        private static string logPath;

        static PaymentService()
        {
            try
            {
                
                ProveedorBaseUrl = GetConfigValue("Proveedor:BaseUrl");
                ProveedorAuthUrl = GetConfigValue("Proveedor:AuthUrl");
                TokenEndpoint = GetConfigValue("Proveedor:TokenEndpoint");
                CheckoutUrl = GetConfigValue("Proveedor:CheckoutUrl");
                Username = GetConfigValue("Proveedor:Username");
                Password = GetConfigValue("Proveedor:Password");
                ClientId = GetConfigValue("Proveedor:ClientId");
                ClientSecret = GetConfigValue("Proveedor:ClientSecret");
                NotificationUrl = GetConfigValue("Proveedor:NotificationUrl");
                CollectorId = GetConfigValue("Proveedor:CollectorId");
                
                var connStr = ConfigurationManager.ConnectionStrings["DefaultConnection"];
                if (connStr == null)
                {
                    throw new ConfigurationErrorsException("La cadena de conexión 'DefaultConnection' no está configurada");
                }
                ConnectionString = connStr.ConnectionString;
                
                ProveedorApiUrl = ConfigurationManager.AppSettings["ProvApiUrl"];
                ProveedorApiKey = ConfigurationManager.AppSettings["ProvApiKey"];

                if (string.IsNullOrEmpty(ConnectionString))
                {
                    throw new ConfigurationErrorsException("La cadena de conexión 'DefaultConnection' está vacía");
                }

                string appDataPath = Path.Combine(HttpContext.Current.Server.MapPath("~/"), "App_Data");
                if (!Directory.Exists(appDataPath))
                {
                    Directory.CreateDirectory(appDataPath);
                }
                logPath = Path.Combine(appDataPath, "payment.log");
                LogDebug("PaymentService inicializado correctamente");
                
                
                LogDebug(String.Format("Configuraciones cargadas: BaseUrl={0}, AuthUrl={1}, TokenEndpoint={2}, CheckoutUrl={3}, Username={4}, ClientId={5}, NotificationUrl={6}, CollectorId={7}",
                    ProveedorBaseUrl, ProveedorAuthUrl, TokenEndpoint, CheckoutUrl, Username, ClientId, NotificationUrl, CollectorId));
            }
            catch (Exception ex)
            {
                
                try
                {
                    logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "payment.log");
                    LogDebug(String.Format("Error en inicialización principal: {0}", ex.ToString()));
                }
                catch (Exception logEx)
                {
                    
                    throw new Exception("Error crítico en la inicialización de PaymentService", ex);
                }
            }
        }

        private static string GetConfigValue(string key)
        {
            string value = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrEmpty(value))
            {
                throw new ConfigurationErrorsException(String.Format("La configuración '{0}' no está presente en el Web.config", key));
            }
            return value;
        }

        private static void LogDebug(string message)
        {
            try
            {
                string logEntry = String.Format("{0:yyyy-MM-dd HH:mm:ss} - {1}\n", DateTime.Now, message);
                File.AppendAllText(logPath, logEntry);
            }
            catch { /* Ignorar errores de logging */ }
        }

        public static string CreateCheckout(string token, string opcion)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    throw new ArgumentNullException("token");
                }

                var transaccionData = TransaccionData.GetTransaccionByToken(token);
                if (transaccionData == null)
                {
                    throw new Exception("No se encontró la transacción");
                }

                //Obtener token de acceso y grabarlo
                string accessToken = GetAccessToken();
                LogDebug("Token de acceso obtenido, procediendo a grabarlo en la base de datos");
                SaveTokenToDatabase(accessToken);

                
                // Determinar el nombre del tributo según el TasaID
                string nombreTributo = ObtenerNombreTributoPorId(transaccionData.Tasa);
                string descripcion = string.Format(
                    "Tasa: {0} - Cuenta: {1} - Identificación: {2} - Recibo: {3}",
                    nombreTributo,
                    transaccionData.NumeroDocumento.TrimStart('0'),
                    transaccionData.NombreContribuyente,
                    transaccionData.ReferenciaExterna
                );

                LogDebug(string.Format("Tasa utilizado como concept_id: {0}", transaccionData.Tasa));
                var details = new[] 
                {
                    new
                    {
                        external_reference = transaccionData.ReferenciaExterna,
                        concept_id = transaccionData.Tasa,
                        concept_description = descripcion,
                        amount = transaccionData.Monto
                    }
                };

                var payer = new
                {
                    name = transaccionData.NombreContribuyente,
                    email = transaccionData.Email ?? "sin-email@example.com",
                    identification = new
                    {
                        type = MapearTipoDocumento(null),
                        number = transaccionData.DocumentoContribuyente,
                        country = "ARG"
                    }
                };

                var presets = new { type = GetPresetType(opcion) };

                var checkoutRequest = new
                {
                    collector_id = CollectorId,
                    currency_id = "ARS",
                    external_transaction_id = transaccionData.DocumentoContribuyente + "-" + DateTime.Now.Ticks,
                    due_date = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd'T'HH:mm:ss-0300"),
                    presets,
                    details,
                    payer,
                    return_url = "",
                    notification_url = NotificationUrl
                };

                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                var checkoutUrl = ProveedorBaseUrl.TrimEnd('/') + CheckoutUrl;
                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    client.Headers[HttpRequestHeader.Authorization] = "Bearer " + accessToken;
                    
                    var jsonRequest = jsonSerializer.Serialize(checkoutRequest);
                    LogDebug(String.Format("Request de checkout: {0}", jsonRequest));
                    
                    var response = client.UploadString(checkoutUrl, jsonRequest);
                    LogDebug(String.Format("Respuesta de checkout: {0}", response));
                    
                    var responseDict = jsonSerializer.Deserialize<Dictionary<string, object>>(response);
                    
                    // Grabar los datos de la transacción en la base de datos
                    LogDebug("Respuesta de Proveedor recibida, procediendo a grabar en la base de datos");
                    SaveTransactionDataToDatabase(responseDict, transaccionData, opcion, accessToken, response);
                    
                    if (responseDict.ContainsKey("form_url"))
                        return responseDict["form_url"].ToString();
                    if (responseDict.ContainsKey("url"))
                        return responseDict["url"].ToString();
                        
                    throw new Exception("No se encontró la URL en la respuesta");
                }
            }
            catch (WebException ex)
            {
                var response = ex.Response as HttpWebResponse;
                if (response != null)
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        var error = reader.ReadToEnd();
                        LogDebug(String.Format("Error de Proveedor: {0}", error));
                        throw new Exception("Error de Proveedor: " + error);
                    }
                }
                throw;
            }
            catch (Exception ex)
            {
                LogDebug(String.Format("Error en CreateCheckout: {0}", ex.ToString()));
                throw;
            }
        }

        private static string GetAccessToken()
        {
            try
            {
                LogDebug("Obteniendo token de acceso de Proveedor");

                var authUrl = ProveedorAuthUrl.TrimEnd('/') + TokenEndpoint;
                LogDebug(String.Format("URL de autenticación: {0}", authUrl));

                ServicePointManager.SecurityProtocol = (SecurityProtocolType)30;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    
                    var formData = new Dictionary<string, string>
                    {
                        { "grant_type", "password" },
                        { "username", Username },
                        { "password", Password },
                        { "client_id", ClientId },
                        { "client_secret", ClientSecret }
                    };

                    var formDataString = String.Join("&", formData.Select(kvp => 
                        String.Format("{0}={1}", 
                            HttpUtility.UrlEncode(kvp.Key), 
                            HttpUtility.UrlEncode(kvp.Value)
                        )
                    ).ToArray());

                    LogDebug(String.Format("Request de token: {0}", formDataString));
                    var response = client.UploadString(authUrl, formDataString);
                    LogDebug(String.Format("Respuesta de token: {0}", response));

                    var tokenResponse = jsonSerializer.Deserialize<Dictionary<string, object>>(response);
                    return tokenResponse["access_token"].ToString();
                }
            }
            catch (Exception ex)
            {
                LogDebug(String.Format("Error en GetAccessToken: {0}", ex.ToString()));
                throw;
            }
        }

        private static string GetPresetType(string medioPago)
        {
            if (medioPago == null)
            {
                LogDebug("Tipo de preset null, usando online por defecto");
                return "online";
            }

            switch (medioPago.ToLower())
            {
                case "6":
                    return "online";
                case "3":
                    return "qr";
                case "2":
                    return "debin";
                default:
                    LogDebug(String.Format("Tipo de preset no reconocido: {0}, usando online por defecto", medioPago));
                    return "online";
            }
        }

        private static string MapearTipoDocumento(string tipoDocumento)
        {
           
            return "DNI_ARG";
        }

        private static void RegisterTransaction(string externalTransactionId, string numeroContribuyente, string periodo, decimal monto, string medioPago, string nombre, string email, string tipoDocumento, string numeroDocumento)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                using (var command = new SqlCommand("sp_int", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@NumeroContribuyente", numeroContribuyente != null ? numeroContribuyente : "0");
                    command.Parameters.AddWithValue("@Periodo", periodo != null ? periodo : "0");
                    command.Parameters.AddWithValue("@MontoAPagar", monto);
                    command.Parameters.AddWithValue("@MedioPago", GetMedioPagoString(medioPago));
                    command.Parameters.AddWithValue("@PayerNombre", nombre != null ? nombre : "N/A");
                    command.Parameters.AddWithValue("@PayerEmail", email != null ? email : "contribuyente@ejemplo.com");
                    command.Parameters.AddWithValue("@PayerTipoDocumento", tipoDocumento != null ? tipoDocumento : "DNI");
                    command.Parameters.AddWithValue("@PayerNumeroDocumento", numeroDocumento != null ? numeroDocumento : "0");
                    command.Parameters.AddWithValue("@ExternalTransactionId", externalTransactionId);

                    command.ExecuteNonQuery();
                    LogDebug(String.Format("Transacción registrada en la base de datos. ID: {0}", externalTransactionId));
                }
            }
        }

        private static string GetMedioPagoString(string medioPago)
        {
            switch (medioPago)
            {
                case "6": return "ONLINE";
                case "3": return "QR";
                case "2": return "DEBIN";
                default: return "ONLINE";
            }
        }

        private static void SaveTokenToDatabase(string accessToken)
        {
            try
            {
                LogDebug("Guardando token en base");
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    
                    var sql = @"
                        INSERT INTO base
                        (
                            TokenAcceso,
                            TipoToken,
                            TiempoExpiracion,
                            RefreshToken,
                            Alcance,
                            FechaCreacion
                        )
                        VALUES
                        (
                            @TokenAcceso,
                            @TipoToken,
                            @TiempoExpiracion,
                            @RefreshToken,
                            @Alcance,
                            @FechaCreacion
                        )";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@TokenAcceso", accessToken);
                        command.Parameters.AddWithValue("@TipoToken", "Bearer");
                        command.Parameters.AddWithValue("@TiempoExpiracion", 3000); // 50 minutos
                        command.Parameters.AddWithValue("@RefreshToken", DBNull.Value);
                        command.Parameters.AddWithValue("@Alcance", "email profile");
                        command.Parameters.AddWithValue("@FechaCreacion", DateTime.Now);

                        command.ExecuteNonQuery();
                        LogDebug("Token guardado exitosamente en base");
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebug(String.Format("Error al guardar token en la base de datos: {0}", ex.Message));
                throw;
            }
        }

        private static void SaveTransactionDataToDatabase(Dictionary<string, object> responseDict, TransaccionData transaccionData, string opcion, string accessToken, string ProveedorResponse)
        {
            try
            {
                LogDebug("Guardando datos de transacción en la base de datos");
                
                // Extraer datos de la respuesta de Proveedor
                string checkoutId = responseDict.ContainsKey("id") ? responseDict["id"].ToString() : "";
                string externalTransactionId = responseDict.ContainsKey("external_transaction_id") ? responseDict["external_transaction_id"].ToString() : "";
                string status = responseDict.ContainsKey("status") ? responseDict["status"].ToString() : "pending";
                string email = transaccionData.Email ?? "sin-email@example.com";
                decimal monto = transaccionData.Monto;
                string nombreTasa;
                switch (transaccionData.Tasa)
                {
                    case "1":
                        nombreTasa = "Inmuebles";
                        break;
                    case "2":
                        nombreTasa = "Comercios";
                        break;
                    case "5":
                        nombreTasa = "Vehículos";
                        break;
                    default:
                        nombreTasa = "Tasas Varias";
                        break;
                }
                string descripcion = string.Format(
                    "Tasa: {0} - Cuenta: {1} - Identificación: {2} - Recibo: {3}",
                    nombreTasa,
                    transaccionData.NumeroDocumento,
                    transaccionData.NombreContribuyente,
                    transaccionData.ReferenciaExterna
                );
                string medioPago = GetPresetType(opcion);

                int tokenId = GetLatestTokenId();
              
                string tokenInterno = transaccionData.Token;
           
                string numeroRecibo = transaccionData.ReferenciaExterna;

                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                 
                    LogDebug("Insertando en base");
                    var sqlTransaccion = @"
                        INSERT INTO base
                        (
                            CheckoutID,
                            Email,
                            Monto,
                            Estado,
                          
                            TokenResult
                        )
                        VALUES
                        (
                            @CheckoutId,
                            @Email,
                            @Monto,
                          
                            @ExternalReference,
                            @TokenResult
                        )";

                    using (var command = new SqlCommand(sqlTransaccion, connection))
                    {
                        command.Parameters.AddWithValue("@CheckoutId", checkoutId);
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Monto", monto);
                     
                        command.Parameters.AddWithValue("@TokenResult", tokenInterno);
                        command.ExecuteNonQuery();
                        LogDebug("Transacción guardada exitosamente en base");
                    }
                }

                LogDebug("Datos de transacción guardados exitosamente en todas las tablas");
            }
            catch (Exception ex)
            {
                LogDebug(String.Format("Error al guardar datos de transacción en la base de datos: {0}", ex.Message));
                throw;
            }
        }

        private static int GetLatestTokenId()
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    
                    var sql = "SELECT TOP 1 Id FROM base ORDER BY Fecha_Creacion DESC";
                    using (var command = new SqlCommand(sql, connection))
                    {
                        var result = command.ExecuteScalar();
                        return result != null ? Convert.ToInt32(result) : 0;
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebug(String.Format("Error al obtener el último TokenID: {0}", ex.Message));
                return 0;
            }
        }

        
    }

    public class TokenResponse
    {
        public string accesstoken { get; set; }
    }

    public class CheckoutResponse
    {
        public string url { get; set; }
    }
} 