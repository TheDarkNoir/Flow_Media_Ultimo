using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FlowMediaWebMVC.Models;
using FlowMediaWebMVC.Helpers;

namespace FlowMediaWebMVC.Controllers
{
    public class CatalogController : Controller
    {
        // ===============================
        // GET: Catalog (LISTA PRODUCTOS)
        // ===============================
        public ActionResult Index(string filter = null, string q = null)
        {
            try
            {
                using (var db = new FlowMediaEntities())
                {
                    var query =
                        db.Producto_Multimedia
                        .AsQueryable();

                    string filtroNormalizado =
                        null;

                    if (!string.IsNullOrWhiteSpace(filter))
                    {
                        filtroNormalizado =
                            filter.Trim()
                            .ToLower();
                    }

                    if (!string.IsNullOrWhiteSpace(q))
                    {
                        var textoBusqueda =
                            q.Trim()
                            .ToLower();

                        query = query.Where(p =>
                            (p.nombre_producto != null &&
                             p.nombre_producto
                             .ToLower()
                             .Contains(textoBusqueda))

                            || (p.descripcion_producto != null &&
                                p.descripcion_producto
                                .ToLower()
                                .Contains(textoBusqueda))

                            || (p.tipo_producto != null &&
                                p.tipo_producto
                                .ToLower()
                                .Contains(textoBusqueda))
                        );

                        filtroNormalizado =
                            "todo";
                    }

                    if (!string.IsNullOrEmpty(
                        filtroNormalizado)
                        && filtroNormalizado != "todo")
                    {
                        query = query.Where(p =>
                            (p.tipo_producto != null &&
                             p.tipo_producto
                             .ToLower()
                             == filtroNormalizado)

                            || (p.clasificacion_producto != null &&
                                p.clasificacion_producto
                                .ToLower()
                                == filtroNormalizado)
                        );
                    }

                    var data = query
                        .OrderBy(p =>
                            p.codigo_producto)
                        .Select(p => new
                        {
                            p.codigo_producto,
                            p.nombre_producto,
                            p.tipo_producto,
                            p.descripcion_producto,
                            p.imagen_producto,
                            p.precio_producto,
                            p.precio_producto_renta,
                            p.clasificacion_producto
                        })
                        .ToList();

                    var items = data
                        .Select(p =>
                            new CatalogItem
                            {
                                Id =
                                    p.codigo_producto,

                                Title =
                                    p.nombre_producto,

                                Category =
                                    p.tipo_producto,

                                Description =
                                    p.descripcion_producto,

                                ImageUrl =
                                    Url.NormalizeImagePath(
                                        p.imagen_producto
                                        ?? string.Empty),

                                FilterKey =
                                    p.tipo_producto,

                                Price =
                                    p.precio_producto_renta > 0
                                    ? p.precio_producto_renta
                                    : p.precio_producto
                            })
                        .ToList();

                    ViewBag.Filter =
                        filter;

                    ViewBag.Query =
                        q;

                    return PartialView(
                        "~/Views/Cliente/_Catalog.cshtml",
                        items);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "CatalogController.Index error: "
                    + ex);

                Response.StatusCode =
                    500;

                return Content(
                    "<p>Error al leer el catálogo.</p>");
            }
        }

        public ActionResult Catalogo(
            string filter = null,
            string q = null)
        {
            if (!string.IsNullOrWhiteSpace(q))
            {
                filter =
                    "Todo";
            }

            ViewBag.Filter =
                filter;

            ViewBag.Query =
                q;

            return View(
                "~/Views/Cliente/Catalogo.cshtml");
        }

        public ActionResult Detalles(
            int id)
        {
            try
            {
                using (var db =
                    new FlowMediaEntities())
                {
                    var producto =
                        db.Producto_Multimedia
                        .FirstOrDefault(p =>
                            p.codigo_producto
                            == id);

                    if (producto == null)
                    {
                        return HttpNotFound();
                    }

                    return View(
                        "~/Views/Cliente/DetallesProducto.cshtml",
                        producto);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "CatalogController.Detalles error: "
                    + ex);

                return HttpNotFound();
            }
        }

        // ===============================
        // RENTAR PRODUCTO
        // ===============================

        [HttpPost]
        public JsonResult RentarProducto(
            int id,
            int dias)
        {
            try
            {
                using (var db =
                    new FlowMediaEntities())
                {
                    if (Session["UsuarioId"] == null)
                    {
                        return Json(new
                        {
                            success = false,
                            mensaje =
                            "Debe iniciar sesión"
                        });
                    }

                    long usuarioId =
                        Convert.ToInt64(
                            Session["UsuarioId"]
                        );

                    if (dias <= 0)
                    {
                        return Json(
                            new
                            {
                                success = false,
                                mensaje =
                                "Días inválidos"
                            });
                    }

                    var producto =
                        db.Producto_Multimedia
                        .FirstOrDefault(p =>
                            p.codigo_producto
                            == id);

                    if (producto == null)
                    {
                        return Json(
                            new
                            {
                                success = false,
                                mensaje =
                                "Producto no encontrado"
                            });
                    }

                    if (producto
                        .cantidad_disponible_producto
                        <= 0)
                    {
                        return Json(
                            new
                            {
                                success = false,
                                mensaje =
                                "Producto sin stock"
                            });
                    }

                    DateTime fechaRenta =
                        DateTime.Now;

                    DateTime fechaDevolucion =
                        fechaRenta
                        .AddDays(dias);

                    var nuevaRenta =
                        new Renta
                        {
                            fecha_renta =
                                fechaRenta,

                            fecha_devolucion_renta =
                                fechaDevolucion,

                            estado_renta =
                                "Activa",

                            numero_identificacion_usuario =
                                usuarioId
                        };

                    db.Renta.Add(
                        nuevaRenta);

                    db.SaveChanges();

                    return Json(
                        new
                        {
                            success = true,
                            mensaje =
                            "Producto rentado correctamente"
                        });
                }
            }
            catch (Exception ex)
            {
                return Json(
                    new
                    {
                        success = false,
                        mensaje =
                        "Error al rentar: "
                        + ex.Message
                    });
            }
        }

        // ===============================
        // COMPRAR PRODUCTO
        // ===============================

        [HttpPost]
        public JsonResult ComprarProducto(
            int id)
        {
            try
            {
                using (var db =
                    new FlowMediaEntities())
                {
                    if (Session["UsuarioId"] == null)
                    {
                        return Json(new
                        {
                            success = false,
                            mensaje =
                            "Debe iniciar sesión"
                        });
                    }

                    long usuarioId =
                        Convert.ToInt64(
                            Session["UsuarioId"]
                        );

                    var producto =
                        db.Producto_Multimedia
                        .FirstOrDefault(p =>
                            p.codigo_producto
                            == id);

                    if (producto == null)
                    {
                        return Json(
                            new
                            {
                                success = false,
                                mensaje =
                                "Producto no encontrado"
                            });
                    }

                    if (producto
                        .cantidad_disponible_producto
                        <= 0)
                    {
                        return Json(
                            new
                            {
                                success = false,
                                mensaje =
                                "Producto sin stock"
                            });
                    }

                    var nuevaCompra =
                        new Compra
                        {
                            fecha_compra =
                                DateTime.Now,

                            metodopago_compra =
                                "Efectivo",

                            precio_compra =
                                producto.precio_producto,

                            numero_identificacion_usuario =
                                usuarioId
                        };

                    producto
                        .cantidad_disponible_producto--;

                    db.Compra.Add(
                        nuevaCompra);

                    db.SaveChanges();

                    return Json(
                        new
                        {
                            success = true,
                            mensaje =
                            "Producto comprado correctamente"
                        });
                }
            }
            catch (Exception ex)
            {
                return Json(
                    new
                    {
                        success = false,
                        mensaje =
                        "Error al comprar: "
                        + ex.Message
                    });
            }
        }
    }
}