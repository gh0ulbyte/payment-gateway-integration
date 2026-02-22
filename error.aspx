<%@ Page Language="C#" AutoEventWireup="true" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Error - Aplicación</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        .error-container { max-width: 800px; margin: 0 auto; }
        .error-message { background-color: #ffebee; border: 1px solid #ffcdd2; padding: 15px; margin: 10px 0; }
        .error-details { background-color: #f5f5f5; border: 1px solid #e0e0e0; padding: 15px; margin: 10px 0; white-space: pre-wrap; }
        h2 { color: #d32f2f; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="error-container">
            <h2>Error en el Procesamiento</h2>
            <div class="error-message">
                Ha ocurrido un error inesperado. Por favor, revise los logs para más detalles.
            </div>
            <div class="error-details">
                <% 
                try 
                {
                    string logPath = Server.MapPath("~/App_Data/error.log");
                    if (System.IO.File.Exists(logPath))
                    {
                        string[] lastLines = System.IO.File.ReadAllLines(logPath)
                            .Reverse()
                            .Take(10)
                            .Reverse()
                            .ToArray();
                            
                        Response.Write("Últimas entradas del log:\n\n");
                        foreach (string line in lastLines)
                        {
                            Response.Write(Server.HtmlEncode(line) + "\n");
                        }
                    }
                    else
                    {
                        Response.Write("No se encontró el archivo de log.");
                    }
                }
                catch (Exception ex)
                {
                    Response.Write("Error al leer el log: " + ex.Message);
                }
                %>
            </div>
            <div style="text-align: center; margin-top: 20px;">
                <button type="button" onclick="history.back();" style="padding: 10px 20px;">Volver</button>
            </div>
        </div>
    </form>
</body>
</html> 