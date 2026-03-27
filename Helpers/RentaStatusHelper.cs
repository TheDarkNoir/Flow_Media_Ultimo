using System;

namespace FlowMediaWebMVC.Helpers
{
    /// <summary>
    /// Estados posibles para una renta.
    /// </summary>
    public enum RentaState
    {
        Desconocido = 0,
        EnCurso = 1,
        Vencida = 2,
        Devuelta = 3
    }

    public static class RentaStatusHelper
    {
        /// <summary>
        /// Determina si una renta está vencida comparando la fecha de devolución con la fecha actual.
        /// </summary>
        public static bool IsRentaVencida(DateTime fechaDevolucion)
        {
            return fechaDevolucion < DateTime.Now;
        }

        /// <summary>
        /// Devuelve el estado calculado de una renta.
        /// - Si el campo estado_renta es "Devuelta" devuelve Devuelta.
        /// - Si la fecha de devolución es anterior a ahora devuelve Vencida.
        /// - Si la fecha actual está entre fecha_renta y fecha_devolucion_renta devuelve EnCurso.
        /// - En otro caso devuelve Desconocido.
        /// </summary>
        public static RentaState GetRentaState(FlowMediaWebMVC.Renta renta)
        {
            if (renta == null) return RentaState.Desconocido;

            try
            {
                if (!string.IsNullOrEmpty(renta.estado_renta) && renta.estado_renta.Equals("Devuelta", StringComparison.OrdinalIgnoreCase))
                {
                    return RentaState.Devuelta;
                }

                var now = DateTime.Now;

                // Si la fecha de devolución es anterior al momento actual => vencida
                if (renta.fecha_devolucion_renta < now)
                {
                    return RentaState.Vencida;
                }

                // Si estamos entre fecha_renta y fecha_devolucion_renta => en curso
                if (renta.fecha_renta <= now && renta.fecha_devolucion_renta >= now)
                {
                    return RentaState.EnCurso;
                }

                return RentaState.Desconocido;
            }
            catch
            {
                return RentaState.Desconocido;
            }
        }

        /// <summary>
        /// Conveniencia: indica si la renta se considera "activa" (en curso).
        /// </summary>
        public static bool IsRentaActiva(FlowMediaWebMVC.Renta renta)
        {
            return GetRentaState(renta) == RentaState.EnCurso;
        }
    }
}
