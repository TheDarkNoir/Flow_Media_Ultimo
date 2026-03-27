<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CrearCuenta.aspx.cs" Inherits="FlowMediaWeb.CrearCuenta" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>Crear cuenta</title>
    <link href="Estilos/CrearCuenta.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="page-frame">
            <div class="topbar">Crear cuenta</div>
            <asp:Button ID="BtnCancelar" runat="server" CssClass="btn-cancel" Text="Cancelar" />

            <div class="center-wrap">
                <div class="create-card">
                    <div class="avatar" aria-hidden="true">
                        <div class="avatar-icon"></div>
                    </div>

                    <h2 class="card-title">Crear cuenta</h2>

                    <div class="form-grid">
                        <div class="col">
                            <label for="TxtNumDocumento">Número de documento</label>
                            <asp:TextBox ID="TxtNumDocumento" runat="server" CssClass="input" placeholder="Numero de documento"></asp:TextBox>

                            <label for="CBOTipoDoc">Tipo de identificación</label>
                            <asp:DropDownList ID="CBOTipoDoc" runat="server" CssClass="select">
                                <asp:ListItem Text="Seleccionar" Value="" />
                                <asp:ListItem Text="DNI" Value="DNI" />
                                <asp:ListItem Text="Pasaporte" Value="PAS" />
                                <asp:ListItem Text="Cedula de Ciudadania" Value="CC" />
                                <asp:ListItem Text="Cedula de Extrajeria" Value="CE" />
                            </asp:DropDownList>

                            <label for="TxtCorreo">Correo</label>
                            <asp:TextBox ID="TxtCorreo" runat="server" CssClass="input" placeholder="Ingrese su correo"></asp:TextBox>

                            <label for="TxtTelefono">Teléfono</label>
                            <asp:TextBox ID="TxtTelefono" runat="server" CssClass="input" placeholder="Ingrese su numero de telefono"></asp:TextBox>
                        </div>

                        <div class="col">
                            <label for="TxtNombres">Nombre/s</label>
                            <asp:TextBox ID="TxtNombres" runat="server" CssClass="input" placeholder="Ingrese su nombre"></asp:TextBox>

                            <label for="TxtApellidos">Apellidos</label>
                            <asp:TextBox ID="TxtApellidos" runat="server" CssClass="input" placeholder="Ingrese sus apellidos"></asp:TextBox>

                            <label for="TxtContrasena">Contraseña</label>
                            <asp:TextBox ID="TxtContrasena" runat="server" CssClass="input" TextMode="Password" placeholder="Ingrese su contraseña"></asp:TextBox>

                            <label for="TxtDireccion">Dirección</label>
                            <asp:TextBox ID="TxtDireccion" runat="server" CssClass="input" placeholder="Ingrese su dirección"></asp:TextBox>
                        </div>
                    </div>

                    <div class="actions">
                        <asp:Button ID="BtnRegistrar" runat="server" CssClass="btn-register" Text="Registrar" />
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>