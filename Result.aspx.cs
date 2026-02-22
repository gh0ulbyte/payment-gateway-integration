using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Proveedor.V1;

public partial class ResultProveedor : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.ContentType = "text/html; charset=utf-8";
        try
        {
            if (!Page.IsPostBack)
            {
                if (Request.QueryString["tokenResult"] != null)
                {
                    string token = Request.QueryString["tokenResult"];
                    MostrarDebug("Token recibido en querystring: " + token);
                    
                    // Buscar la transacción en BAse
                    var transaccion = BuscarTransaccionPorToken(token);
                    if (transaccion != null)
                    {
                        MostrarDebug("Transacción encontrada en base");
                        MostrarTransaccion(transaccion);
                    }
                    else
                    {
                        MostrarError("No se encontró la transacción en la base de datos. Es posible que aún esté siendo procesada.");
                        MostrarDebug("No se encontró transacción para el token: " + token);
                        MostrarTransaccionBasica(token);
                    }
                }
                else
                {
                    MostrarError("No se encontró información de transacción.");
                    MostrarDebug("No se recibió tokenResult en la querystring");
                }
            }
        }
        catch (Exception ex)
        {
            MostrarError("Error al procesar la solicitud: " + ex.Message);
            MostrarDebug("Excepción en Page_Load: " + ex.Message);
        }
    }

    private void MostrarTransaccion(DataRow transaccion)
    {
        lblTasa.Text = transaccion.Table.Columns.Contains("Tasa") && transaccion["Tasa"] != DBNull.Value ? transaccion["Tasa"].ToString() : "-";
        lblCuenta.Text = transaccion.Table.Columns.Contains("Cuenta") && transaccion["Cuenta"] != DBNull.Value ? transaccion["Cuenta"].ToString() : "-";
        lblRazon.Text = transaccion.Table.Columns.Contains("Razon") && transaccion["Razon"] != DBNull.Value ? transaccion["Razon"].ToString() : "-";
        lblRecibo.Text = transaccion.Table.Columns.Contains("Recibo") && transaccion["Recibo"] != DBNull.Value ? transaccion["Recibo"].ToString() : "-";
        lblFecha.Text = transaccion.Table.Columns.Contains("Fecha_Vencimiento") && transaccion["Fecha_Vencimiento"] != DBNull.Value ? Convert.ToDateTime(transaccion["Fecha_Vencimiento"]).ToString("dd/MM/yyyy") : "-";
        lblImporte.Text = transaccion.Table.Columns.Contains("Monto") && transaccion["Monto"] != DBNull.Value ? Convert.ToDecimal(transaccion["Monto"]).ToString("C") : "-";
        lblFormaPago.Text = transaccion.Table.Columns.Contains("FormaPago") && transaccion["FormaPago"] != DBNull.Value ? transaccion["FormaPago"].ToString() : "-";
        lblResultado.Text = transaccion.Table.Columns.Contains("Estado") && transaccion["Estado"] != DBNull.Value ? transaccion["Estado"].ToString() : "-";
    }

    private void MostrarTransaccionBasica(string token)
    {
        lblImporte.Text = "-";
        lblFecha.Text = DateTime.Now.ToString("dd/MM/yyyy");
        MostrarError("La transaccion esta siendo procesada. Por favor, recargue la pagina en unos minutos.");
    }

    private DataRow BuscarTransaccionPorToken(string token)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            
            string query = @"
                SELECT 
                
                FROM BAse t
                WHERE t. = 
                ORDER BY t. DESC";
            
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Token", token);
                
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    
                    if (dt.Rows.Count > 0)
                    {
                        return dt.Rows[0];
                    }
                }
            }
        }
        
        return null;
    }

    private void MostrarError(string mensaje)
    {
        lblError.Text = mensaje;
        lblError.Visible = true;
    }

    protected void btnContinuar_Click(object sender, EventArgs e)
    {
        Response.Write("<script>window.close();</script>");
    }

    private void MostrarDebug(string mensaje)
    {
        // Solo mostrar en desarrollo o si está habilitado
        if (ConfigurationManager.AppSettings["MostrarDebug"] == "true")
        {
            lblDebug.Text += mensaje + "<br/>";
            lblDebug.Visible = true;
        }
    }
} 