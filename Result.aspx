<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Proveedor.aspx.cs" Inherits="Proveedor" ResponseEncoding="utf-8" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <title>Resultado de Proveedor</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }
        .container { max-width: 600px; margin: 0 auto; background: white; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); overflow: hidden; }
        .header { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; }
        .header h1 { margin: 0; font-size: 24px; font-weight: bold; }
        .header .subtitle { margin-top: 5px; opacity: 0.9; font-size: 14px; }
        .content { padding: 30px; }
        .info-email { background: #e9f7ef; color: #155724; border: 1px solid #b7e1cd; border-radius: 5px; padding: 12px; margin-bottom: 20px; text-align: center; font-size: 15px; }
        .receipt-item { display: flex; justify-content: space-between; align-items: center; padding: 15px 0; border-bottom: 1px solid #eee; }
        .receipt-item:last-child { border-bottom: none; }
        .receipt-label { font-weight: bold; color: #333; min-width: 120px; }
        .receipt-value { color: #666; text-align: right; flex: 1; }
        .amount { font-size: 24px; font-weight: bold; color: #28a745; }
        .status { padding: 8px 16px; border-radius: 20px; font-weight: bold; font-size: 14px; text-transform: uppercase; }
        .status.success { background-color: #d4edda; color: #155724; }
        .status.pending { background-color: #fff3cd; color: #856404; }
        .status.error { background-color: #f8d7da; color: #721c24; }
        .footer { background-color: #f8f9fa; padding: 20px 30px; text-align: center; border-top: 1px solid #eee; }
        .btn { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; border: none; padding: 12px 30px; border-radius: 25px; cursor: pointer; font-size: 16px; text-decoration: none; display: inline-block; margin: 5px; }
        .btn:hover { opacity: 0.9; }
        .print-button { background-color: #6c757d; }
        .print-button:hover { background-color: #5a6268; }
        .error-message { background-color: #f8d7da; color: #721c24; padding: 15px; border-radius: 5px; margin-bottom: 20px; border: 1px solid #f5c6cb; }
        @media print { body { background: white; } .container { box-shadow: none; } .btn { display: none; } .info-email { display: none; } }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <div class="header">
                <h1>RESULTADO DE Proveedor</h1>
                <div class="subtitle">Sistema de Pagos Electronicos </div>
            </div>
            <div class="content">
                <div class="info-email">
                    El resultado de Proveedor llegara al email proporcionado.<br />
                    Tambien puede descargarlo en PDF desde aqui.
                </div>
                <asp:Label ID="lblError" runat="server" CssClass="error-message" Visible="false"></asp:Label>
                <div class="receipt-item">
                    <span class="receipt-label">Tasa:</span>
                    <span class="receipt-value"><asp:Label ID="lblTasa" runat="server"></asp:Label></span>
                </div>
                <div class="receipt-item">
                    <span class="receipt-label">Cuenta:</span>
                    <span class="receipt-value"><asp:Label ID="lblCuenta" runat="server"></asp:Label></span>
                </div>
                <div class="receipt-item">
                    <span class="receipt-label">Identificacion:</span>
                    <span class="receipt-value"><asp:Label ID="lblRazon" runat="server"></asp:Label></span>
                </div>
                <div class="receipt-item">
                    <span class="receipt-label">Recibo:</span>
                    <span class="receipt-value"><asp:Label ID="lblRecibo" runat="server"></asp:Label></span>
                </div>
                <div class="receipt-item">
                    <span class="receipt-label">Fecha de Vencimiento:</span>
                    <span class="receipt-value"><asp:Label ID="lblFecha" runat="server"></asp:Label></span>
                </div>
                <div class="receipt-item">
                    <span class="receipt-label">Importe:</span>
                    <span class="receipt-value"><asp:Label ID="lblImporte" runat="server" CssClass="amount"></asp:Label></span>
                </div>
                <div class="receipt-item">
                    <span class="receipt-label">Forma de pago:</span>
                    <span class="receipt-value"><asp:Label ID="lblFormaPago" runat="server"></asp:Label></span>
                </div>
                <hr />
                <div class="receipt-item">
                    <span class="receipt-label">Resultado:</span>
                    <span class="receipt-value"><asp:Label ID="lblResultado" runat="server"></asp:Label></span>
                </div>
            </div>
            <div class="footer">
                <button type="button" class="btn print-button" onclick="window.print();">Descargar PDF</button>
                <asp:Button ID="btnContinuar" runat="server" Text="Finalizar" CssClass="btn" OnClick="btnContinuar_Click" />
            </div>
        </div>
    </form>
</body>
</html> 
</html> 