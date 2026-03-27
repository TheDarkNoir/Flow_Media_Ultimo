<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="FlowMediaWeb.Login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>Login</title>
    <link runat="server" href="~/Estilos/Login.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="login-page">
            <div class="login-panel">
                <h1 class="welcome">BIENVENIDO</h1>
                <p class="subtitle">Ingresa para gestionar tus rentas y compras</p>

                <div class="login-form">
                    <label class="field-label">EMAIL</label>
                    <div class="input-wrap">
                        <span class="icon icon-mail">✉</span>
                        <asp:TextBox ID="TxtCorreo" runat="server" CssClass="input" Placeholder="tu@email.com"></asp:TextBox>
                    </div>

                    <label class="field-label">CONTRASEÑA</label>
                    <div class="input-wrap">
                        <span class="icon icon-lock">🔒</span>
                        <asp:TextBox ID="TextContrasena" runat="server" CssClass="input" TextMode="Password" Placeholder="Ingrese su contraseña"></asp:TextBox>
                    </div>

                    <asp:Button ID="BtnIniciar" runat="server" CssClass="btn-primary" Text="Ingresar →" OnClick="BtnIniciar_Click" />

                    <div class="register">
                        ¿No tienes cuenta? <asp:LinkButton ID="LinkButton1" runat="server" OnClick="LinkButton1_Click">Regístrate aquí</asp:LinkButton>
                    </div>
                </div>
            </div>
        </div>

        <footer class="login-footer">
            <div class="footer-inner">
                <div class="logo-mark small"></div>
                <div class="copyright">© <%: DateTime.Now.Year %> FLOW MEDIA. TODOS LOS DERECHOS RESERVADOS.<br/>Tu entretenimiento, siempre disponible.</div>
                <asp:Button ID="BtnSalir" runat="server" CssClass="btn-exit-footer" Text="Salir" />
            </div>
        </footer>
    </form>
</body>
</html>
