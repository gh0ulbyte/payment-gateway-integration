<%@ Page Language="C#" %>

<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            try
            {
                string token = Request.QueryString["token"];
                string type = Request.QueryString["type"];
                if (type == null) type = "online";

                if (string.IsNullOrEmpty(token))
                {
                    lblMessage.Text = "Token no válido o no proporcionado.";
                    return;
                }

                string urlPago = String.Format(
                    "https://tu-dominio.com/ruta/Default.aspx?token={0}&type={1}", 
                    Server.UrlEncode(token), 
                    Server.UrlEncode(type)
                );
                
                string logPath = Server.MapPath("~/log.txt");
                System.IO.File.AppendAllText(logPath, String.Format("URL de redirección: {0}\n", urlPago));
                
                Response.Redirect(urlPago, true);
            }
            catch (Exception ex)
            {
                lblMessage.Text = String.Format("Error: {0}", ex.Message);
            }
        }
    }
</script>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Redireccionando al sistema de pago...</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel="stylesheet" />
    <style>
        .loader-container {
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
            flex-direction: column;
        }
        .spinner-border {
            width: 3rem;
            height: 3rem;
        }
        .message {
            margin-top: 1rem;
            font-size: 1.2rem;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <div class="loader-container">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Cargando...</span>
                </div>
                <div class="message">
                    <asp:Label ID="lblMessage" runat="server" Text="Redireccionando al sistema de pago..."></asp:Label>
                </div>
            </div>
        </div>
    </form>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html> 