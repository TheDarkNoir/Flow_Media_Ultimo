using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using FlowMediaWebMVC.Models;
using FlowMediaWebMVC.Helpers;

namespace FlowMediaWebMVC.Controllers
{
    public class UsuarioController : Controller
    {
        private FlowMediaEntities db = new FlowMediaEntities();

        // GET: Usuario
        public ActionResult Index()
        {
            return View();
        }

        // GET: Usuario/UsuarioMenu
        public ActionResult UsuarioMenu()
        {
            // La vista real se encuentra en Views/Cliente/UsuarioMenu.cshtml
            return View("~/Views/Cliente/UsuarioMenu.cshtml");
        }

        // GET: Usuario/Carrito
        public ActionResult Carrito()
        {
            try
            {
                var docObj = Session["Documento"];
                if (docObj == null)
                {
                    // No hay sesión: pasar lista vacía para que la vista muestre mensaje y enlace al catálogo
                    return View("~/Views/Cliente/Carrito.cshtml", model: new List<Carrito_Detalle>());
                }

                long docIdLong;
                if (!long.TryParse(docObj.ToString(), out docIdLong))
                {
                    // si no es numérico, intentar buscar por email
                    var usuarioStr = docObj.ToString();
                    var usuario = db.Usuario.FirstOrDefault(u => u.email_usuario == usuarioStr);
                    if (usuario == null)
                    {
                        return View("~/Views/Cliente/Carrito.cshtml", model: new List<Carrito_Detalle>());
                    }
                    docIdLong = usuario.numero_identificacion_usuario;
                }

                // Cargar carrito y sus detalles incluyendo información del producto
                var carrito = db.Carrito_Compra
                                .Include(c => c.Carrito_Detalle.Select(d => d.Producto_Multimedia))
                                .FirstOrDefault(c => c.numero_identificacion_usuario == docIdLong);

                if (carrito == null || carrito.Carrito_Detalle == null || !carrito.Carrito_Detalle.Any())
                {
                    return View("~/Views/Cliente/Carrito.cshtml", model: new List<Carrito_Detalle>());
                }

                var detalles = carrito.Carrito_Detalle.ToList();
                return View("~/Views/Cliente/Carrito.cshtml", model: detalles);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Carrito error: " + ex);
                return View("~/Views/Cliente/Carrito.cshtml", model: new List<Carrito_Detalle>());
            }
        }

        // GET: Usuario/PerfilCliente
        public ActionResult PerfilCliente()
        {
            // Intentar obtener identificador desde session
            try
            {
                var docObj = Session["Documento"];
                if (docObj == null)
                {
                    // No hay sesión, redirigir a login
                    return RedirectToAction("Login", "Account");
                }

                Usuario usuario = null;

                long docIdLong;
                if (!long.TryParse(docObj.ToString(), out docIdLong))
                {
                    // Si no es numérico, intentar comparar como string identificador
                    var usuarioStr = docObj.ToString();
                    usuario = db.Usuario.FirstOrDefault(u => u.email_usuario == usuarioStr || (u.numero_identificacion_usuario.ToString() == usuarioStr));
                }
                else
                {
                    usuario = db.Usuario.FirstOrDefault(u => u.numero_identificacion_usuario == docIdLong);
                }

                if (usuario == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Calcular contadores: rentas activas y compras totales
                try
                {
                    var userId = usuario.numero_identificacion_usuario;

                    // Contar rentas activas usando helper IsRentaActiva
                    var rentas = db.Renta.Where(r => r.numero_identificacion_usuario == userId).ToList();
                    var rentasActivas = rentas.Count(r => RentaStatusHelper.IsRentaActiva(r));

                    // Contar compras del usuario
                    var comprasTotales = db.Compra.Count(c => c.numero_identificacion_usuario == userId);

                    ViewBag.RentasCount = rentasActivas;
                    ViewBag.ComprasCount = comprasTotales;
                }
                catch (Exception ex)
                {
                    // si falla la consulta, asignar 0
                    System.Diagnostics.Debug.WriteLine("Error calculando contadores de usuario: " + ex);
                    ViewBag.RentasCount = 0;
                    ViewBag.ComprasCount = 0;
                }

                return View("~/Views/Cliente/PerfilCliente.cshtml", usuario);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("PerfilCliente error: " + ex);
                return View("~/Views/Cliente/PerfilCliente.cshtml");
            }
        }

        // Partial: Perfil
        public ActionResult Perfil()
        {
            // En un escenario real obtén datos desde la BD usando el usuario autenticado
            var nombre = Session["Usuario"] as string ?? "Usuario de ejemplo";
            return PartialView("_Perfil", model: nombre);
        }

        // Partial: Pedidos
        public ActionResult Pedidos()
        {
            // Datos de ejemplo; reemplazar por consultas a la BD
            var pedidos = new List<Pedido>
            {
                new Pedido { Id = 101, Item = "Renta - Juego A", Fecha = DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd") },
                new Pedido { Id = 102, Item = "Compra - Película B", Fecha = DateTime.Now.AddDays(-10).ToString("yyyy-MM-dd") }
            };

            // Si la petición es AJAX (desde la vista de perfil que carga contenido dinámicamente), devolver la partial.
            if (Request.IsAjaxRequest())
            {
                return PartialView("_Pedidos", pedidos);
            }

            // Si se accede directamente por URL (no-AJAX), devolver la vista completa que contiene el layout cliente.
            // La vista `MisRentas.cshtml` usa AJAX para cargar el contenido inicial desde esta misma acción.
            return View("~/Views/Cliente/MisRentas.cshtml");
        }

        // GET: Usuario/Compras
        public ActionResult Compras()
        {
            var compras = new List<Pedido>
            {
                new Pedido { Id = 201, Item = "Compra - The Amazing Spiderman", Fecha = DateTime.Now.AddMonths(-2).ToString("yyyy-MM-dd") },
                new Pedido { Id = 202, Item = "Compra - Prototype 2", Fecha = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd") }
            };

            // same logic as Pedidos: return partial for AJAX, full view for direct access
            if (Request.IsAjaxRequest())
            {
                return PartialView("_Compras", compras);
            }
            // FIX: return MisCompras view when not AJAX (was incorrectly returning MisRentas)
            return View("~/Views/Cliente/MisCompras.cshtml");
        }

        // Partial: Ajustes (formulario)
        public ActionResult Ajustes()
        {
            // En caso necesario se podría pasar un modelo con datos actuales
            return PartialView("_Ajustes");
        }

        // POST: Usuario/GuardarAjustes (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GuardarAjustes(string nombre, string email)
        {
            // Simulación de guardado. Reemplazar por lógica DB real.
            bool exito = true;
            string mensaje = "Ajustes guardados correctamente.";

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(nombre))
            {
                exito = false;
                mensaje = "Nombre y correo son requeridos.";
            }

            return Json(new { success = exito, message = mensaje });
        }

        // GET: Usuario/ChatBot
        public ActionResult ChatBot()
        {
            return View("~/Views/Cliente/ChatBot.cshtml");
        }

        [HttpGet]
        public JsonResult ListConversations()
        {
            var list = Session["Conversations"] as List<Conversation>;
            if (list == null) list = new List<Conversation>();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateConversation()
        {
            var list = Session["Conversations"] as List<Conversation>;
            if (list == null) list = new List<Conversation>();
            var c = new Conversation { Id = Guid.NewGuid().ToString(), Title = "Nueva conversación", CreatedAt = DateTime.Now, UserIdentifier = (Session["Usuario"] as string) ?? "anonymous" };
            list.Insert(0, c);
            Session["Conversations"] = list;
            return Json(c);
        }

        [HttpPost]
        public JsonResult DeleteConversation(string id)
        {
            var list = Session["Conversations"] as List<Conversation>;
            if (list == null) list = new List<Conversation>();
            var found = list.FirstOrDefault(x => x.Id == id);
            if (found != null) list.Remove(found);
            Session["Conversations"] = list;
            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult SendMessage(SendMessageDto dto)
        {
            // Demo reply logic - in real app call AI or business logic
            var reply = "Gracias por tu mensaje. En breve un agente te responderá o revisa nuestra sección de ayuda.";
            return Json(new { reply = reply });
        }

        public class SendMessageDto { public string user { get; set; } public string text { get; set; } }

        // GET: Usuario/EditarPerfil
        public ActionResult EditarPerfil()
        {
            try
            {
                var docObj = Session["Documento"];
                if (docObj == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                long docIdLong;
                if (!long.TryParse(docObj.ToString(), out docIdLong))
                {
                    var usuarioStr = docObj.ToString();
                    var usuarioAlt = db.Usuario.FirstOrDefault(u => u.email_usuario == usuarioStr || u.numero_identificacion_usuario.ToString() == usuarioStr);
                    if (usuarioAlt == null) return RedirectToAction("Login", "Account");
                    return View("~/Views/Cliente/EditarPerfil.cshtml", usuarioAlt);
                }

                var usuario = db.Usuario.FirstOrDefault(u => u.numero_identificacion_usuario == docIdLong);
                if (usuario == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                return View("~/Views/Cliente/EditarPerfil.cshtml", usuario);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("EditarPerfil GET error: " + ex);
                return RedirectToAction("PerfilCliente");
            }
        }

        // POST: Usuario/EditarPerfil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarPerfil(Usuario model, string nueva_contraseña, string confirmar_contraseña)
        {
            try
            {
                var docObj = Session["Documento"];
                if (docObj == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                long docIdLong;
                if (!long.TryParse(docObj.ToString(), out docIdLong))
                {
                    return RedirectToAction("Login", "Account");
                }

                var usuario = db.Usuario.FirstOrDefault(u => u.numero_identificacion_usuario == docIdLong);
                if (usuario == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Update fields
                usuario.nombre_usuario = model.nombre_usuario;
                usuario.email_usuario = model.email_usuario;
                usuario.telefono_usuario = model.telefono_usuario;
                usuario.direccion_usuario = model.direccion_usuario;

                // Update password only if a new one was provided
                if (!string.IsNullOrWhiteSpace(nueva_contraseña))
                {
                    if (nueva_contraseña != confirmar_contraseña)
                    {
                        TempData["Error"] = "Las contraseñas no coinciden.";
                        return View("~/Views/Cliente/EditarPerfil.cshtml", usuario);
                    }
                    usuario.contraseña_usuario = nueva_contraseña;
                }

                db.SaveChanges();

                // Update session name in case it changed
                Session["Usuario"] = usuario.nombre_usuario;

                TempData["Mensaje"] = "Perfil actualizado correctamente.";
                return RedirectToAction("PerfilCliente");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("EditarPerfil POST error: " + ex);
                TempData["Error"] = "Error al guardar los cambios: " + ex.Message;
                return RedirectToAction("EditarPerfil");
            }
        }
    }
}
