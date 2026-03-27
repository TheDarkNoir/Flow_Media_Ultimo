using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlowMediaWebMVC.Modelo
{
    public class ReporteRenta
    {
        public string nombre_usuario { get; set; }

        public long telefono_usuario { get; set; }

        public DateTime fecha_renta { get; set; }

        public DateTime fecha_devolucion_renta { get; set; }

        public string estado_renta { get; set; }
    }
}