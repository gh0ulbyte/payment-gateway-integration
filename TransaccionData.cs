using System;
using System.IO;
using System.Data.SqlClient;
using System.Configuration;
using System.Web;

namespace Proveedor.V1
{
    public class TransaccionData
    {
        private static string logPath;

        static TransaccionData()
        {
            try
            {
                string appDataPath = Path.Combine(HttpContext.Current.Server.MapPath("~/"), "App_Data");
                if (!Directory.Exists(appDataPath))
                {
                    Directory.CreateDirectory(appDataPath);
                }
                logPath = Path.Combine(appDataPath, "transaccion.log");
                LogDebug("TransaccionData inicializado correctamente");
            }
            catch (Exception ex)
            {
                logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "transaccion.log");
                try
                {
                    LogDebug("TransaccionData inicializado con ruta alternativa");
                }
                catch { }
            }
        }

        private static void LogDebug(string message)
        {
            try
            {
                string logEntry = String.Format("{0:yyyy-MM-dd HH:mm:ss} - {1}\n", DateTime.Now, message);
                File.AppendAllText(logPath, logEntry);
            }
            catch (Exception ex) 
            { 
                try
                {
                    string altLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "error.log");
                    File.AppendAllText(altLogPath, String.Format("{0:yyyy-MM-dd HH:mm:ss} - Error al escribir log: {1}\n", DateTime.Now, ex.Message));
                }
                catch { }
            }
        }

        public decimal Monto { get; set; }
        public string Email { get; set; }
        public string Nombre { get; set; }
        public string NumeroDocumento { get; set; }
        public string ReferenciaExterna { get; set; }
        public string Token { get; set; }
        public DateTime FechaTransaccion { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string Referencia { get { return ReferenciaExterna; } }
        public string NombreContribuyente { get { return Nombre; } }
        public string DocumentoContribuyente { get { return NumeroDocumento; } }
        public string Tasa { get; set; }
      
        public static TransaccionData GetTransaccionByToken(string token)
        {
            return ObtenerPorToken(token);
        }

        public static TransaccionData ObtenerPorToken(string token)
        {
            LogDebug(String.Format("Buscando transaccion con token: {0}", token));

            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            LogDebug("Usando conexión DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    LogDebug("Conexion a base de datos abierta");

                    using (var command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = @"
                            SELECT TOP 1
                                Transaccion_Tasa as Tasa,
                                Transaccion_Valor as Monto,
                          
                                DATEADD(day, 30, Transaccion_Fecha) as FechaVencimiento
                            FROM BASE
                            WHERE Transaccion = @token
                            ORDER BY TransaccionFecha DESC";

                        command.Parameters.AddWithValue("@token", token);
                        LogDebug(String.Format("Ejecutando query: {0}", command.CommandText));
                        LogDebug(String.Format("Parametro @token: {0}", token));

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var transaccion = new TransaccionData
                                {
                                    Tasa = reader.IsDBNull(reader.GetOrdinal("Tasa")) ? null : reader.GetString(reader.GetOrdinal("Tasa")),
                                    Monto = reader.GetDecimal(reader.GetOrdinal("Monto")),
                                    Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                                    
                                    FechaVencimiento = reader.GetDateTime(reader.GetOrdinal("FechaVencimiento"))
                                };

                                LogDebug(String.Format("Transaccion encontrada: Monto={0}, Nombre={1}, NumeroDocumento={2}, Token={3}", 
                                    transaccion.Monto, transaccion.Nombre, transaccion.NumeroDocumento, transaccion.Token));

                                // Guardar el token en Base como Token_Result
                                Guardarbase(token);

                                return transaccion;
                            }

                            LogDebug("No se encontro la transaccion en la base de datos");
                            return null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogDebug(String.Format("Error al obtener transaccion: {0}\nStackTrace: {1}", ex.Message, ex.StackTrace));
                    throw;
                }
            }
        }

        public static void GuardarTokenEnbase(string token)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                        UPDATE base
                        SET TokenResult = @Token
                        WHERE TokenResult IS NULL AND Checkout_ID IS NOT NULL
                    ";
                    command.Parameters.AddWithValue("@Token", token);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void InsertarBASE(string checkoutId)
        {
            LogDebug(String.Format("Insertando en BASE: PreferenceID={0}", checkoutId));
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (var command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = @"
                            INSERT INTO BASE
                                (Transaccion_PreferenceID)
                            VALUES
                                (@PreferenceID)
                        ";
                        command.Parameters.AddWithValue("@PreferenceID", checkoutId);
                        int rows = command.ExecuteNonQuery();
                        LogDebug(String.Format("Filas insertadas: {0}", rows));
                    }
                }
                catch (Exception ex)
                {
                    LogDebug(String.Format("Error al insertar: {0}", ex.Message));
                    throw;
                }
            }
        }
    }
} 