using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlowMediaWebMVC.Modelo
{
    public class ReporteMulta
    {
        public string nombre_usuario { get; set; }

        public string email_usuario { get; set; }

        public string descripcion_multa { get; set; }

        public double monto_multa { get; set; }

        public string estado_multa { get; set; }

        public string fecha_multa { get; set; }
    }
}