using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FlowMediaWebMVC.Modelo
{
    public class Compra
    {
        [Key]
        public int id_compra { get; set; }

        public DateTime fecha_compra { get; set; }

        public string metodopago_compra { get; set; }

        public double precio_compra { get; set; }

        public long numero_identificacion_usuario { get; set; }
    }
}