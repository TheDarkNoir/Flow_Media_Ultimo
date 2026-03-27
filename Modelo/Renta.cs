using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FlowMediaWebMVC.Modelo
{
    public class Renta
    {
        [Key]
        public int id_renta { get; set; }

        public DateTime fecha_renta { get; set; }

        public DateTime fecha_devolucion_renta { get; set; }

        public string estado_renta { get; set; }

        public long numero_identificacion_usuario { get; set; }
    }
}