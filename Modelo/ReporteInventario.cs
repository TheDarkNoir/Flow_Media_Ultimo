using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlowMediaWebMVC.Modelo
{
    public class ReporteInventario
    {
        public int codigo_inventario { get; set; }

        public int cantidad_inventario { get; set; }

        public string ubicacion_producto_inventario { get; set; }

        public string nombre_producto { get; set; }

        public string tipo_producto { get; set; }

        public double precio_producto { get; set; }

        public int cantidad_disponible_producto { get; set; }
    }
}