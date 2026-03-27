using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlowMediaWebMVC.Modelo
{
    public class ReporteUsuarios
    {
        public string nombre_usuario { get; set; }

        public string email_usuario { get; set; }

        public long telefono_usuario { get; set; }

        public string direccion_usuario { get; set; }
    }
}