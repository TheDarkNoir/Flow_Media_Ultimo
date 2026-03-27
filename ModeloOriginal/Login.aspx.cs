using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FlowMediaWeb
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

   

        protected void BtnIniciar_Click(object sender, EventArgs e, SqlConnection sqlConnection)
        {
            string correo = TxtCorreo.Text.Trim();
            string contrasena = TextContrasena.Text.Trim();

            using (SqlConnection conn = sqlConnection)
            {
                string query = @"select tipo_usuario  from Usuario  where email_usuario = @email_usuario  and Contraseña_usuario = @Contraseña_usuario";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@email_usuario", correo);
                cmd.Parameters.AddWithValue("@Contraseña_usuario", contrasena);

                conn.Open();

                object resultado = cmd.ExecuteScalar();

                if (resultado != null) // si encontró usuario
                {
                    string rol = resultado.ToString();

                    Session["Usuario"] = correo;
                    Session["Rol"] = rol;

                    if (rol == "Admin")
                    {
                        Response.Redirect("~/Admin/PanelAdmin.aspx");
                    }
                    else if (rol == "Cliente")
                    {
                        Response.Redirect("~/Cliente/PanelCliente.aspx");
                    }
                    else
                    {
                        Response.Write("Rol no reconocido.");
                    }
                }
                else
                {
                    Response.Write("Correo o contraseña incorrectos");
                }
            }
        }

        protected void LinkButton1_Click(object sender, EventArgs e)
        {

        }
    }
}