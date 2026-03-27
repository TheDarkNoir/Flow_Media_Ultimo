using FlowMediaWebMVC;
using FlowMediaWebMVC.Modelo;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.Web.Mvc;

namespace FlowMediaWebMVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly string conexion;

        public AdminController()
        {
            conexion = ConfigurationManager.ConnectionStrings["conexion"].ConnectionString;
        }

        //========================
        // USUARIOS
        //========================

        public ActionResult PanelAdmin(string filtro)
        {
            var lista = new List<Usuario>();

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = "SELECT * FROM Usuario WHERE tipo_usuario = 'Cliente' ";

                if (!string.IsNullOrEmpty(filtro))
                {
                    query = @"SELECT * FROM Usuario
                              WHERE nombre_usuario LIKE @filtro
                              OR email_usuario LIKE @filtro";
                }

                SqlCommand cmd = new SqlCommand(query, con);

                if (!string.IsNullOrEmpty(filtro))
                    cmd.Parameters.AddWithValue("@filtro", "%" + filtro + "%");

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    lista.Add(new Usuario
                    {
                        numero_identificacion_usuario = Convert.ToInt64(dr["numero_identificacion_usuario"]),
                        tipo_identificacion_usuario = dr["tipo_identificacion_usuario"].ToString(),
                        nombre_usuario = dr["nombre_usuario"].ToString(),
                        email_usuario = dr["email_usuario"].ToString(),
                        telefono_usuario = Convert.ToInt64(dr["telefono_usuario"]),
                        direccion_usuario = dr["direccion_usuario"].ToString()
                    });
                }
            }

            return View(lista);
        }

        public ActionResult CrearUsuario()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CrearUsuario(Usuario u)
        {
            // ========== VALIDACIONES ==========

            // Validar campos obligatorios
            if (string.IsNullOrWhiteSpace(u.nombre_usuario))
                ModelState.AddModelError("nombre_usuario", "El nombre es obligatorio");

            if (string.IsNullOrWhiteSpace(u.email_usuario))
                ModelState.AddModelError("email_usuario", "El email es obligatorio");

            if (string.IsNullOrWhiteSpace(u.contraseña_usuario))
                ModelState.AddModelError("contraseña_usuario", "La contraseña es obligatoria");

            // Validar formato de email
            if (!string.IsNullOrWhiteSpace(u.email_usuario) && !u.email_usuario.Contains("@"))
                ModelState.AddModelError("email_usuario", "Ingrese un email válido");

            // Validar teléfono (si se proporciona)
            if (u.telefono_usuario.ToString().Length != 10)
                ModelState.AddModelError("telefono_usuario", "El teléfono debe tener 10 dígitos");

            // Validar documento no duplicado
            using (SqlConnection con = new SqlConnection(conexion))
            {
                con.Open();
                string checkQuery = "SELECT COUNT(*) FROM Usuario WHERE numero_identificacion_usuario = @id";
                SqlCommand checkCmd = new SqlCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@id", u.numero_identificacion_usuario);
                int existe = (int)checkCmd.ExecuteScalar();

                if (existe > 0)
                    ModelState.AddModelError("numero_identificacion_usuario", "Ya existe un usuario con este documento");
            }

            if (ModelState.IsValid)
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    string query = @"INSERT INTO Usuario
                                    (numero_identificacion_usuario, tipo_identificacion_usuario, nombre_usuario, 
                                     telefono_usuario, email_usuario, direccion_usuario, contraseña_usuario, tipo_usuario)
                                    VALUES 
                                    (@id, @tipo, @nombre, @telefono, @correo, @direccion, @password, 'Cliente')";

                    SqlCommand cmd = new SqlCommand(query, con);

                    cmd.Parameters.AddWithValue("@id", u.numero_identificacion_usuario);
                    cmd.Parameters.AddWithValue("@tipo", u.tipo_identificacion_usuario);
                    cmd.Parameters.AddWithValue("@nombre", u.nombre_usuario);
                    cmd.Parameters.AddWithValue("@telefono", u.telefono_usuario);
                    cmd.Parameters.AddWithValue("@correo", u.email_usuario);
                    cmd.Parameters.AddWithValue("@direccion", u.direccion_usuario);
                    cmd.Parameters.AddWithValue("@password", u.contraseña_usuario);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Usuario creado exitosamente";
                return RedirectToAction("PanelAdmin");
            }

            return View(u);
        }

        public ActionResult EliminarUsuario(long id)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                con.Open();

                // Verificar si tiene rentas
                string verificar = "SELECT COUNT(*) FROM Renta WHERE numero_identificacion_usuario=@id";
                SqlCommand cmdVerificar = new SqlCommand(verificar, con);
                cmdVerificar.Parameters.AddWithValue("@id", id);

                int cantidad = (int)cmdVerificar.ExecuteScalar();

                if (cantidad > 0)
                {
                    TempData["Error"] = "No se puede eliminar el usuario porque tiene rentas activas.";
                    return RedirectToAction("PanelAdmin");
                }

                string query = "DELETE FROM Usuario WHERE numero_identificacion_usuario=@id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", id);

                cmd.ExecuteNonQuery();
            }

            TempData["SuccessMessage"] = "Usuario eliminado exitosamente";
            return RedirectToAction("PanelAdmin");
        }


        //========================
        // INVENTARIO - CRUD COMPLETO
        //========================

        // GET: EditarInventario
        [HttpGet]
        public ActionResult EditarInventario(int id)
        {
            Inventario inventario = null;

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = "SELECT * FROM Inventario WHERE codigo_inventario = @id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", id);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    inventario = new Inventario
                    {
                        codigo_inventario = Convert.ToInt32(dr["codigo_inventario"]),
                        cantidad_inventario = Convert.ToInt32(dr["cantidad_inventario"]),
                        fecha_ingreso_inventario = dr["fecha_ingreso_inventario"].ToString(),
                        ubicacion_producto_inventario = dr["ubicacion_producto_inventario"].ToString()
                    };
                }
            }

            if (inventario == null)
            {
                TempData["ErrorMessage"] = "Inventario no encontrado";
                return RedirectToAction("Inventario");
            }

            ViewBag.Title = "EDITAR INVENTARIO";
            return View(inventario);
        }

        // POST: EditarInventario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarInventario(Inventario inventario)
        {
            // Validaciones
            if (inventario.cantidad_inventario < 0)
                ModelState.AddModelError("cantidad_inventario", "La cantidad no puede ser negativa");

            if (string.IsNullOrWhiteSpace(inventario.ubicacion_producto_inventario))
                ModelState.AddModelError("ubicacion_producto_inventario", "La ubicación es obligatoria");

            if (ModelState.IsValid)
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    string query = @"UPDATE Inventario SET 
                                    cantidad_inventario = @cantidad,
                                    fecha_ingreso_inventario = @fecha,
                                    ubicacion_producto_inventario = @ubicacion
                                    WHERE codigo_inventario = @id";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", inventario.codigo_inventario);
                    cmd.Parameters.AddWithValue("@cantidad", inventario.cantidad_inventario);
                    cmd.Parameters.AddWithValue("@fecha", inventario.fecha_ingreso_inventario);
                    cmd.Parameters.AddWithValue("@ubicacion", inventario.ubicacion_producto_inventario);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Inventario actualizado exitosamente";
                return RedirectToAction("Inventario");
            }

            ViewBag.Title = "EDITAR INVENTARIO";
            return View(inventario);
        }

        public ActionResult Inventario()
        {
            var lista = new List<Inventario>();

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = "SELECT * FROM Inventario";

                SqlCommand cmd = new SqlCommand(query, con);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    lista.Add(new Inventario
                    {
                        codigo_inventario = Convert.ToInt32(dr["codigo_inventario"]),
                        cantidad_inventario = Convert.ToInt32(dr["cantidad_inventario"]),
                        fecha_ingreso_inventario = dr["fecha_ingreso_inventario"].ToString(),
                        ubicacion_producto_inventario = dr["ubicacion_producto_inventario"].ToString()
                    });
                }
            }

            return View(lista);
        }

        public ActionResult CrearInventario()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CrearInventario(Inventario i)
        {
            // ========== VALIDACIONES ==========

            // Validar código no duplicado
            using (SqlConnection con = new SqlConnection(conexion))
            {
                con.Open();
                string checkQuery = "SELECT COUNT(*) FROM Inventario WHERE codigo_inventario = @codigo";
                SqlCommand checkCmd = new SqlCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@codigo", i.codigo_inventario);
                int existe = (int)checkCmd.ExecuteScalar();

                if (existe > 0)
                    ModelState.AddModelError("codigo_inventario", "Ya existe un inventario con este código");
            }

            // Validar cantidad no negativa
            if (i.cantidad_inventario < 0)
                ModelState.AddModelError("cantidad_inventario", "La cantidad no puede ser negativa");

            // Validar ubicación no vacía
            if (string.IsNullOrWhiteSpace(i.ubicacion_producto_inventario))
                ModelState.AddModelError("ubicacion_producto_inventario", "La ubicación es obligatoria");

            if (ModelState.IsValid)
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    string query = @"INSERT INTO Inventario
                                    (codigo_inventario, fecha_ingreso_inventario, cantidad_inventario, ubicacion_producto_inventario)
                                    VALUES 
                                    (@codigo, @fecha, @cantidad, @ubicacion)";

                    SqlCommand cmd = new SqlCommand(query, con);

                    cmd.Parameters.AddWithValue("@codigo", i.codigo_inventario);
                    cmd.Parameters.AddWithValue("@fecha", i.fecha_ingreso_inventario);
                    cmd.Parameters.AddWithValue("@cantidad", i.cantidad_inventario);
                    cmd.Parameters.AddWithValue("@ubicacion", i.ubicacion_producto_inventario);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Inventario creado exitosamente";
                return RedirectToAction("Inventario");
            }

            return View(i);
        }

        // ===========================
        // Metodo Eliminar inventario
        // ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult EliminarInventario(int id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    con.Open();

                    // Verificar si tiene productos asociados
                    string verificar = "SELECT COUNT(*) FROM Producto_Multimedia WHERE codigo_inventario = @id";
                    SqlCommand cmdVerificar = new SqlCommand(verificar, con);
                    cmdVerificar.Parameters.AddWithValue("@id", id);

                    int cantidad = (int)cmdVerificar.ExecuteScalar();

                    if (cantidad > 0)
                    {
                        return Json(new { success = false, message = "No se puede eliminar porque tiene productos asociados" });
                    }

                    // Eliminar inventario
                    string query = "DELETE FROM Inventario WHERE codigo_inventario = @id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", id);

                    int filasAfectadas = cmd.ExecuteNonQuery();

                    if (filasAfectadas > 0)
                    {
                        return Json(new { success = true, message = "Inventario eliminado exitosamente" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Inventario no encontrado" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar: " + ex.Message });
            }
        }

        //========================
        // PRODUCTOS (CON VALIDACIONES)
        //========================

        public ActionResult Productos(string search = null)
        {
            var lista = new List<Producto_Multimedia>();

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = @"SELECT p.*, i.ubicacion_producto_inventario 
                        FROM Producto_Multimedia p
                        LEFT JOIN Inventario i ON p.codigo_inventario = i.codigo_inventario
                        ORDER BY p.nombre_producto";

                if (!string.IsNullOrEmpty(search))
                {
                    query = @"SELECT p.*, i.ubicacion_producto_inventario 
                     FROM Producto_Multimedia p
                     LEFT JOIN Inventario i ON p.codigo_inventario = i.codigo_inventario
                     WHERE p.nombre_producto LIKE @search 
                        OR p.descripcion_producto LIKE @search 
                        OR p.tipo_producto LIKE @search
                     ORDER BY p.nombre_producto";
                }

                SqlCommand cmd = new SqlCommand(query, con);

                if (!string.IsNullOrEmpty(search))
                {
                    cmd.Parameters.AddWithValue("@search", "%" + search + "%");
                }

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    var producto = new Producto_Multimedia
                    {
                        codigo_producto = Convert.ToInt32(dr["codigo_producto"]),
                        nombre_producto = dr["nombre_producto"].ToString(),
                        descripcion_producto = dr["descripcion_producto"]?.ToString(),
                        tipo_producto = dr["tipo_producto"].ToString(),
                        precio_producto = Convert.ToDouble(dr["precio_producto"]),
                        cantidad_disponible_producto = Convert.ToInt32(dr["cantidad_disponible_producto"]),
                        precio_producto_renta = Convert.ToDouble(dr["precio_producto_renta"]),
                        clasificacion_producto = dr["clasificacion_producto"]?.ToString(),
                        año_producto = dr["año_producto"]?.ToString(),
                        codigo_inventario = Convert.ToInt32(dr["codigo_inventario"])
                    };

                    lista.Add(producto);
                }
            }

            ViewBag.Title = "PRODUCTOS";
            return View(lista);
        }

        [HttpGet]
        public ActionResult CrearProducto()
        {
            ViewBag.Title = "CREAR PRODUCTO";
            ViewBag.Inventarios = ObtenerInventarios();
            return View(new Producto_Multimedia());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearProducto(Producto_Multimedia producto)
        {
            // ========== VALIDACIONES ==========

            // Validación 1: Nombre del producto no vacío
            if (string.IsNullOrWhiteSpace(producto.nombre_producto))
            {
                ModelState.AddModelError("nombre_producto", "El nombre del producto es obligatorio");
            }

            // Validación 2: Tipo de producto seleccionado
            if (string.IsNullOrWhiteSpace(producto.tipo_producto))
            {
                ModelState.AddModelError("tipo_producto", "Debe seleccionar un tipo de producto");
            }

            // Validación 3: Precio de renta mayor a 0
            if (producto.precio_producto_renta <= 0)
            {
                ModelState.AddModelError("precio_producto_renta", "El precio de renta debe ser mayor a 0");
            }

            // Validación 4: Precio de venta mayor a 0
            if (producto.precio_producto <= 0)
            {
                ModelState.AddModelError("precio_producto", "El precio de venta debe ser mayor a 0");
            }

            // Validación 5: Precio de renta debe ser menor que precio de venta
            if (producto.precio_producto_renta > 0 && producto.precio_producto > 0 && producto.precio_producto_renta >= producto.precio_producto)
            {
                ModelState.AddModelError("precio_producto_renta", "El precio de renta debe ser menor que el precio de venta");
            }

            // Validación 6: Cantidad disponible no negativa
            if (producto.cantidad_disponible_producto < 0)
            {
                ModelState.AddModelError("cantidad_disponible_producto", "La cantidad disponible no puede ser negativa");
            }

            // Validación 7: Código de inventario seleccionado
            if (producto.codigo_inventario == 0)
            {
                ModelState.AddModelError("codigo_inventario", "Debe seleccionar una ubicación en inventario");
            }

            // Validación 8: Año (si se proporciona) debe tener formato válido
            if (!string.IsNullOrEmpty(producto.año_producto))
            {
                int año;
                if (!int.TryParse(producto.año_producto, out año) || año < 1900 || año > DateTime.Now.Year + 5)
                {
                    ModelState.AddModelError("año_producto", "Ingrese un año válido (1900 - " + (DateTime.Now.Year + 5) + ")");
                }
            }

            if (ModelState.IsValid)
            {
                // Limitar la descripción a 50 caracteres si excede
                if (producto.descripcion_producto != null && producto.descripcion_producto.Length > 50)
                {
                    producto.descripcion_producto = producto.descripcion_producto.Substring(0, 50);
                    TempData["WarningMessage"] = "La descripción ha sido truncada a 50 caracteres.";
                }

                using (SqlConnection con = new SqlConnection(conexion))
                {
                    con.Open();

                    // Obtener el último código
                    string getLastIdQuery = "SELECT ISNULL(MAX(codigo_producto), 0) FROM Producto_Multimedia";
                    SqlCommand getLastIdCmd = new SqlCommand(getLastIdQuery, con);
                    int ultimoCodigo = (int)getLastIdCmd.ExecuteScalar();
                    int nuevoCodigo = ultimoCodigo + 1;

                    string query = @"INSERT INTO Producto_Multimedia 
                            (codigo_producto, nombre_producto, descripcion_producto, tipo_producto, 
                             precio_producto, cantidad_disponible_producto, 
                             precio_producto_renta, clasificacion_producto, 
                             año_producto, codigo_inventario)
                            VALUES 
                            (@codigo, @nombre, @descripcion, @tipo, 
                             @precio, @cantidad, @precio_renta, 
                             @clasificacion, @año, @codigo_inventario)";

                    SqlCommand cmd = new SqlCommand(query, con);

                    cmd.Parameters.AddWithValue("@codigo", nuevoCodigo);
                    cmd.Parameters.AddWithValue("@nombre", producto.nombre_producto);
                    cmd.Parameters.AddWithValue("@descripcion", (object)producto.descripcion_producto ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@tipo", producto.tipo_producto);
                    cmd.Parameters.AddWithValue("@precio", producto.precio_producto);
                    cmd.Parameters.AddWithValue("@cantidad", producto.cantidad_disponible_producto);
                    cmd.Parameters.AddWithValue("@precio_renta", producto.precio_producto_renta);
                    cmd.Parameters.AddWithValue("@clasificacion", (object)producto.clasificacion_producto ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@año", (object)producto.año_producto ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@codigo_inventario", producto.codigo_inventario);

                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Producto creado exitosamente";
                return RedirectToAction("Productos");
            }

            ViewBag.Title = "CREAR PRODUCTO";
            ViewBag.Inventarios = ObtenerInventarios();
            return View(producto);
        }

        [HttpGet]
        public ActionResult EditarProducto(int id)
        {
            Producto_Multimedia producto = null;

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = "SELECT * FROM Producto_Multimedia WHERE codigo_producto = @id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", id);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    producto = new Producto_Multimedia
                    {
                        codigo_producto = Convert.ToInt32(dr["codigo_producto"]),
                        nombre_producto = dr["nombre_producto"].ToString(),
                        descripcion_producto = dr["descripcion_producto"]?.ToString(),
                        tipo_producto = dr["tipo_producto"].ToString(),
                        precio_producto = Convert.ToDouble(dr["precio_producto"]),
                        cantidad_disponible_producto = Convert.ToInt32(dr["cantidad_disponible_producto"]),
                        precio_producto_renta = Convert.ToDouble(dr["precio_producto_renta"]),
                        clasificacion_producto = dr["clasificacion_producto"]?.ToString(),
                        año_producto = dr["año_producto"]?.ToString(),
                        codigo_inventario = Convert.ToInt32(dr["codigo_inventario"])
                    };
                }
                con.Close();
            }

            if (producto == null)
            {
                TempData["ErrorMessage"] = "Producto no encontrado";
                return RedirectToAction("Productos");
            }

            ViewBag.Title = "EDITAR PRODUCTO";
            ViewBag.Inventarios = ObtenerInventarios();
            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarProducto(Producto_Multimedia producto)
        {
            // ========== VALIDACIONES ==========

            // Validación 1: Nombre del producto no vacío
            if (string.IsNullOrWhiteSpace(producto.nombre_producto))
            {
                ModelState.AddModelError("nombre_producto", "El nombre del producto es obligatorio");
            }

            // Validación 2: Tipo de producto seleccionado
            if (string.IsNullOrWhiteSpace(producto.tipo_producto))
            {
                ModelState.AddModelError("tipo_producto", "Debe seleccionar un tipo de producto");
            }

            // Validación 3: Precio de renta mayor a 0
            if (producto.precio_producto_renta <= 0)
            {
                ModelState.AddModelError("precio_producto_renta", "El precio de renta debe ser mayor a 0");
            }

            // Validación 4: Precio de venta mayor a 0
            if (producto.precio_producto <= 0)
            {
                ModelState.AddModelError("precio_producto", "El precio de venta debe ser mayor a 0");
            }

            // Validación 5: Precio de renta debe ser menor que precio de venta
            if (producto.precio_producto_renta > 0 && producto.precio_producto > 0 && producto.precio_producto_renta >= producto.precio_producto)
            {
                ModelState.AddModelError("precio_producto_renta", "El precio de renta debe ser menor que el precio de venta");
            }

            // Validación 6: Cantidad disponible no negativa
            if (producto.cantidad_disponible_producto < 0)
            {
                ModelState.AddModelError("cantidad_disponible_producto", "La cantidad disponible no puede ser negativa");
            }

            // Validación 7: Código de inventario seleccionado
            if (producto.codigo_inventario == 0)
            {
                ModelState.AddModelError("codigo_inventario", "Debe seleccionar una ubicación en inventario");
            }

            // Validación 8: Año (si se proporciona) debe tener formato válido
            if (!string.IsNullOrEmpty(producto.año_producto))
            {
                int año;
                if (!int.TryParse(producto.año_producto, out año) || año < 1900 || año > DateTime.Now.Year + 5)
                {
                    ModelState.AddModelError("año_producto", "Ingrese un año válido (1900 - " + (DateTime.Now.Year + 5) + ")");
                }
            }

            if (ModelState.IsValid)
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    string query = @"UPDATE Producto_Multimedia SET 
                                    nombre_producto = @nombre,
                                    descripcion_producto = @descripcion,
                                    tipo_producto = @tipo,
                                    precio_producto = @precio,
                                    cantidad_disponible_producto = @cantidad,
                                    precio_producto_renta = @precio_renta,
                                    clasificacion_producto = @clasificacion,
                                    año_producto = @año,
                                    codigo_inventario = @codigo_inventario
                                    WHERE codigo_producto = @id";

                    SqlCommand cmd = new SqlCommand(query, con);

                    cmd.Parameters.AddWithValue("@id", producto.codigo_producto);
                    cmd.Parameters.AddWithValue("@nombre", producto.nombre_producto);
                    cmd.Parameters.AddWithValue("@descripcion", (object)producto.descripcion_producto ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@tipo", producto.tipo_producto);
                    cmd.Parameters.AddWithValue("@precio", producto.precio_producto);
                    cmd.Parameters.AddWithValue("@cantidad", producto.cantidad_disponible_producto);
                    cmd.Parameters.AddWithValue("@precio_renta", producto.precio_producto_renta);
                    cmd.Parameters.AddWithValue("@clasificacion", (object)producto.clasificacion_producto ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@año", (object)producto.año_producto ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@codigo_inventario", producto.codigo_inventario);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Producto actualizado exitosamente";
                return RedirectToAction("Productos");
            }

            ViewBag.Title = "EDITAR PRODUCTO";
            ViewBag.Inventarios = ObtenerInventarios();
            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult EliminarProducto(int id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    con.Open();

                    // Verificar si la tabla Carrito_Detalle existe
                    string checkTableQuery = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Carrito_Detalle'";
                    SqlCommand checkTableCmd = new SqlCommand(checkTableQuery, con);
                    int tableExists = (int)checkTableCmd.ExecuteScalar();

                    if (tableExists > 0)
                    {
                        // Verificar si tiene rentas asociadas
                        string verificar = "SELECT COUNT(*) FROM Carrito_Detalle WHERE codigo_producto = @id";
                        SqlCommand cmdVerificar = new SqlCommand(verificar, con);
                        cmdVerificar.Parameters.AddWithValue("@id", id);
                        int cantidad = (int)cmdVerificar.ExecuteScalar();

                        if (cantidad > 0)
                        {
                            return Json(new { success = false, message = "No se puede eliminar porque tiene rentas asociadas" });
                        }
                    }

                    // Eliminar producto
                    string query = "DELETE FROM Producto_Multimedia WHERE codigo_producto = @id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", id);
                    int filasAfectadas = cmd.ExecuteNonQuery();

                    if (filasAfectadas > 0)
                    {
                        return Json(new { success = true, message = "Producto eliminado exitosamente" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Producto no encontrado" });
                    }
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("SQL Error: " + ex.Message);
                return Json(new { success = false, message = "Error de base de datos: " + ex.Message });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("General Error: " + ex.Message);
                return Json(new { success = false, message = "Error al eliminar: " + ex.Message });
            }
        }

        public ActionResult DetalleProducto(int id)
        {
            Producto_Multimedia producto = null;
            string ubicacionInventario = "No asignada";

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = @"SELECT p.*, i.ubicacion_producto_inventario, i.cantidad_inventario
                                FROM Producto_Multimedia p
                                LEFT JOIN Inventario i ON p.codigo_inventario = i.codigo_inventario
                                WHERE p.codigo_producto = @id";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", id);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    producto = new Producto_Multimedia
                    {
                        codigo_producto = Convert.ToInt32(dr["codigo_producto"]),
                        nombre_producto = dr["nombre_producto"].ToString(),
                        descripcion_producto = dr["descripcion_producto"]?.ToString(),
                        tipo_producto = dr["tipo_producto"].ToString(),
                        precio_producto = Convert.ToDouble(dr["precio_producto"]),
                        cantidad_disponible_producto = Convert.ToInt32(dr["cantidad_disponible_producto"]),
                        precio_producto_renta = Convert.ToDouble(dr["precio_producto_renta"]),
                        clasificacion_producto = dr["clasificacion_producto"]?.ToString(),
                        año_producto = dr["año_producto"]?.ToString(),
                        codigo_inventario = Convert.ToInt32(dr["codigo_inventario"])
                    };

                    if (dr["ubicacion_producto_inventario"] != DBNull.Value)
                    {
                        ubicacionInventario = dr["ubicacion_producto_inventario"].ToString();
                    }
                }
            }

            if (producto == null)
            {
                return HttpNotFound();
            }

            ViewBag.Title = "DETALLE PRODUCTO";
            ViewBag.UbicacionInventario = ubicacionInventario;
            return View(producto);
        }

        // Método auxiliar para obtener lista de inventarios
        private List<SelectListItem> ObtenerInventarios()
        {
            var inventarios = new List<SelectListItem>();

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = "SELECT codigo_inventario, ubicacion_producto_inventario FROM Inventario ORDER BY ubicacion_producto_inventario";
                SqlCommand cmd = new SqlCommand(query, con);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                // Agregar opción por defecto
                inventarios.Add(new SelectListItem
                {
                    Value = "",
                    Text = "-- Seleccione una ubicación --"
                });

                while (dr.Read())
                {
                    inventarios.Add(new SelectListItem
                    {
                        Value = dr["codigo_inventario"].ToString(),
                        Text = dr["ubicacion_producto_inventario"].ToString()
                    });
                }
            }

            return inventarios;
        }

        //========================
        // RENTAS
        //========================

        public ActionResult Rentas()
        {
            var lista = new List<Renta>();

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = @"SELECT r.*, u.nombre_usuario 
                                FROM Renta r
                                INNER JOIN Usuario u ON r.numero_identificacion_usuario = u.numero_identificacion_usuario
                                ORDER BY r.fecha_renta DESC";

                SqlCommand cmd = new SqlCommand(query, con);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    lista.Add(new Renta
                    {
                        id_renta = Convert.ToInt32(dr["id_renta"]),
                        fecha_renta = Convert.ToDateTime(dr["fecha_renta"]),
                        fecha_devolucion_renta = Convert.ToDateTime(dr["fecha_devolucion_renta"]),
                        estado_renta = dr["estado_renta"].ToString(),
                        numero_identificacion_usuario = Convert.ToInt64(dr["numero_identificacion_usuario"])
                    });
                }
            }

            ViewBag.Title = "RENTAS Y DEVOLUCIONES";
            return View(lista);
        }

        [HttpGet]
        public ActionResult CrearRenta()
        {
            ViewBag.Title = "CREAR RENTA";
            ViewBag.Usuarios = ObtenerUsuarios();
            ViewBag.Productos = ObtenerProductos();
            return View();
        }

        [HttpPost]
        public ActionResult CrearRenta(Renta r)
        {
            // ========== VALIDACIONES ==========

            // Validar que la fecha de devolución sea posterior a la fecha de renta
            if (r.fecha_devolucion_renta <= r.fecha_renta)
                ModelState.AddModelError("fecha_devolucion_renta", "La fecha de devolución debe ser posterior a la fecha de renta");

            // Validar que el usuario exista
            using (SqlConnection con = new SqlConnection(conexion))
            {
                con.Open();
                string checkQuery = "SELECT COUNT(*) FROM Usuario WHERE numero_identificacion_usuario = @id";
                SqlCommand checkCmd = new SqlCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@id", r.numero_identificacion_usuario);
                int existe = (int)checkCmd.ExecuteScalar();

                if (existe == 0)
                    ModelState.AddModelError("numero_identificacion_usuario", "El usuario no existe");
            }

            // Validar estado válido
            string[] estadosValidos = { "Activa", "Devuelta", "Vencida" };
            if (!estadosValidos.Contains(r.estado_renta))
                ModelState.AddModelError("estado_renta", "Estado no válido");

            if (ModelState.IsValid)
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    string query = @"INSERT INTO Renta 
                                    (fecha_renta, fecha_devolucion_renta, estado_renta, numero_identificacion_usuario)
                                    VALUES 
                                    (@fecha, @devolucion, @estado, @usuario)";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@fecha", r.fecha_renta);
                    cmd.Parameters.AddWithValue("@devolucion", r.fecha_devolucion_renta);
                    cmd.Parameters.AddWithValue("@estado", r.estado_renta);
                    cmd.Parameters.AddWithValue("@usuario", r.numero_identificacion_usuario);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Renta creada exitosamente";
                return RedirectToAction("Rentas");
            }

            ViewBag.Title = "CREAR RENTA";
            ViewBag.Usuarios = ObtenerUsuarios();
            ViewBag.Productos = ObtenerProductos();
            return View(r);
        }

        //========================
        // MULTAS
        //========================

        public ActionResult Multas()
        {
            var lista = new List<Multa>();

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = @"SELECT m.*, r.id_renta, u.nombre_usuario 
                                FROM Multa m
                                INNER JOIN Devolucion d ON m.codigo_devolucion = d.codigo_devolucion
                                INNER JOIN Renta r ON d.id_renta = r.id_renta
                                INNER JOIN Usuario u ON r.numero_identificacion_usuario = u.numero_identificacion_usuario
                                ORDER BY m.fecha_multa DESC";

                SqlCommand cmd = new SqlCommand(query, con);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    lista.Add(new Multa
                    {
                        codigo_multa = Convert.ToInt32(dr["codigo_multa"]),
                        fecha_multa = dr["fecha_multa"].ToString(),
                        descripcion_multa = dr["descripcion_multa"].ToString(),
                        monto_multa = Convert.ToDouble(dr["monto_multa"]),
                        estado_multa = dr["estado_multa"].ToString(),
                        codigo_devolucion = Convert.ToInt32(dr["codigo_devolucion"])
                    });
                }
            }

            ViewBag.Title = "MULTAS";
            return View(lista);
        }

        [HttpGet]
        public ActionResult CrearMulta()
        {
            ViewBag.Title = "CREAR MULTA";
            ViewBag.Devoluciones = ObtenerDevoluciones();
            return View();
        }

        [HttpPost]
        public ActionResult CrearMulta(Multa m)
        {
            // ========== VALIDACIONES ==========

            // Validar fecha no vacía
            if (string.IsNullOrEmpty(m.fecha_multa))
                ModelState.AddModelError("fecha_multa", "La fecha es obligatoria");

            // Validar descripción no vacía
            if (string.IsNullOrWhiteSpace(m.descripcion_multa))
                ModelState.AddModelError("descripcion_multa", "La descripción es obligatoria");

            // Validar monto positivo
            if (m.monto_multa <= 0)
                ModelState.AddModelError("monto_multa", "El monto de la multa debe ser mayor a 0");

            // Validar que la devolución exista
            using (SqlConnection con = new SqlConnection(conexion))
            {
                con.Open();
                string checkQuery = "SELECT COUNT(*) FROM Devolucion WHERE codigo_devolucion = @id";
                SqlCommand checkCmd = new SqlCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@id", m.codigo_devolucion);
                int existe = (int)checkCmd.ExecuteScalar();

                if (existe == 0)
                    ModelState.AddModelError("codigo_devolucion", "La devolución no existe");
            }

            // Validar estado válido
            string[] estadosValidos = { "Pendiente", "Pagada", "Cancelada" };
            if (!estadosValidos.Contains(m.estado_multa))
                ModelState.AddModelError("estado_multa", "Estado no válido");

            if (ModelState.IsValid)
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    string query = @"INSERT INTO Multa
                                    (fecha_multa, descripcion_multa, monto_multa, estado_multa, codigo_devolucion)
                                    VALUES 
                                    (@fecha, @descripcion, @monto, @estado, @devolucion)";

                    SqlCommand cmd = new SqlCommand(query, con);

                    cmd.Parameters.AddWithValue("@fecha", m.fecha_multa);
                    cmd.Parameters.AddWithValue("@descripcion", m.descripcion_multa);
                    cmd.Parameters.AddWithValue("@monto", m.monto_multa);
                    cmd.Parameters.AddWithValue("@estado", m.estado_multa);
                    cmd.Parameters.AddWithValue("@devolucion", m.codigo_devolucion);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Multa creada exitosamente";
                return RedirectToAction("Multas");
            }

            ViewBag.Title = "CREAR MULTA";
            ViewBag.Devoluciones = ObtenerDevoluciones();
            return View(m);
        }

        // Métodos auxiliares para dropdowns
        private List<SelectListItem> ObtenerUsuarios()
        {
            var usuarios = new List<SelectListItem>();

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = "SELECT numero_identificacion_usuario, nombre_usuario FROM Usuario WHERE tipo_usuario = 'Cliente' ORDER BY nombre_usuario";
                SqlCommand cmd = new SqlCommand(query, con);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                usuarios.Add(new SelectListItem { Value = "", Text = "-- Seleccione un usuario --" });

                while (dr.Read())
                {
                    usuarios.Add(new SelectListItem
                    {
                        Value = dr["numero_identificacion_usuario"].ToString(),
                        Text = dr["nombre_usuario"].ToString()
                    });
                }
            }

            return usuarios;
        }

        //========================
        // RENTAS - CRUD COMPLETO
        //========================

        // GET: EditarRenta
        [HttpGet]
        public ActionResult EditarRenta(int id)
        {
            Renta renta = null;

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = "SELECT * FROM Renta WHERE id_renta = @id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", id);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    renta = new Renta
                    {
                        id_renta = Convert.ToInt32(dr["id_renta"]),
                        fecha_renta = Convert.ToDateTime(dr["fecha_renta"]),
                        fecha_devolucion_renta = Convert.ToDateTime(dr["fecha_devolucion_renta"]),
                        estado_renta = dr["estado_renta"].ToString(),
                        numero_identificacion_usuario = Convert.ToInt64(dr["numero_identificacion_usuario"])
                    };
                }
            }

            if (renta == null)
            {
                TempData["ErrorMessage"] = "Renta no encontrada";
                return RedirectToAction("Rentas");
            }

            ViewBag.Title = "EDITAR RENTA";
            ViewBag.Usuarios = ObtenerUsuarios();
            return View(renta);
        }

        // POST: EditarRenta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarRenta(Renta renta)
        {
            // Validar que la fecha de devolución sea posterior a la fecha de renta
            if (renta.fecha_devolucion_renta <= renta.fecha_renta)
                ModelState.AddModelError("fecha_devolucion_renta", "La fecha de devolución debe ser posterior a la fecha de renta");

            // Validar que el usuario exista
            using (SqlConnection con = new SqlConnection(conexion))
            {
                con.Open();
                string checkQuery = "SELECT COUNT(*) FROM Usuario WHERE numero_identificacion_usuario = @id";
                SqlCommand checkCmd = new SqlCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@id", renta.numero_identificacion_usuario);
                int existe = (int)checkCmd.ExecuteScalar();

                if (existe == 0)
                    ModelState.AddModelError("numero_identificacion_usuario", "El usuario no existe");
            }

            // Validar estado válido
            string[] estadosValidos = { "Activa", "Devuelta", "Vencida" };
            if (!estadosValidos.Contains(renta.estado_renta))
                ModelState.AddModelError("estado_renta", "Estado no válido");

            if (ModelState.IsValid)
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    string query = @"UPDATE Renta SET 
                                    fecha_renta = @fecha,
                                    fecha_devolucion_renta = @devolucion,
                                    estado_renta = @estado,
                                    numero_identificacion_usuario = @usuario
                                    WHERE id_renta = @id";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", renta.id_renta);
                    cmd.Parameters.AddWithValue("@fecha", renta.fecha_renta);
                    cmd.Parameters.AddWithValue("@devolucion", renta.fecha_devolucion_renta);
                    cmd.Parameters.AddWithValue("@estado", renta.estado_renta);
                    cmd.Parameters.AddWithValue("@usuario", renta.numero_identificacion_usuario);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Renta actualizada exitosamente";
                return RedirectToAction("Rentas");
            }

            ViewBag.Title = "EDITAR RENTA";
            ViewBag.Usuarios = ObtenerUsuarios();
            return View(renta);
        }

        // POST: EliminarRenta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult EliminarRenta(int id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    con.Open();

                    // Verificar si tiene devoluciones asociadas
                    string verificarDevolucion = "SELECT COUNT(*) FROM Devolucion WHERE id_renta = @id";
                    SqlCommand cmdVerificar = new SqlCommand(verificarDevolucion, con);
                    cmdVerificar.Parameters.AddWithValue("@id", id);
                    int cantidadDevoluciones = (int)cmdVerificar.ExecuteScalar();

                    if (cantidadDevoluciones > 0)
                    {
                        return Json(new { success = false, message = "No se puede eliminar porque tiene devoluciones asociadas" });
                    }

                    // Verificar si tiene multas asociadas
                    string verificarMulta = @"SELECT COUNT(*) FROM Multa m 
                                              INNER JOIN Devolucion d ON m.codigo_devolucion = d.codigo_devolucion 
                                              WHERE d.id_renta = @id";
                    SqlCommand cmdVerificarMulta = new SqlCommand(verificarMulta, con);
                    cmdVerificarMulta.Parameters.AddWithValue("@id", id);
                    int cantidadMultas = (int)cmdVerificarMulta.ExecuteScalar();

                    if (cantidadMultas > 0)
                    {
                        return Json(new { success = false, message = "No se puede eliminar porque tiene multas asociadas" });
                    }

                    // Eliminar renta
                    string query = "DELETE FROM Renta WHERE id_renta = @id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", id);
                    int filasAfectadas = cmd.ExecuteNonQuery();

                    if (filasAfectadas > 0)
                    {
                        return Json(new { success = true, message = "Renta eliminada exitosamente" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Renta no encontrada" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar: " + ex.Message });
            }
        }

        //========================
        // MULTAS - CRUD COMPLETO
        //========================

        // GET: EditarMulta
        [HttpGet]
        public ActionResult EditarMulta(int id)
        {
            Multa multa = null;

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = "SELECT * FROM Multa WHERE codigo_multa = @id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", id);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    multa = new Multa
                    {
                        codigo_multa = Convert.ToInt32(dr["codigo_multa"]),
                        fecha_multa = dr["fecha_multa"].ToString(),
                        descripcion_multa = dr["descripcion_multa"].ToString(),
                        monto_multa = Convert.ToDouble(dr["monto_multa"]),
                        estado_multa = dr["estado_multa"].ToString(),
                        codigo_devolucion = Convert.ToInt32(dr["codigo_devolucion"])
                    };
                }
            }

            if (multa == null)
            {
                TempData["ErrorMessage"] = "Multa no encontrada";
                return RedirectToAction("Multas");
            }

            ViewBag.Title = "EDITAR MULTA";
            ViewBag.Devoluciones = ObtenerDevoluciones();
            return View(multa);
        }

        // POST: EditarMulta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarMulta(Multa multa)
        {
            // Validar fecha no vacía
            if (string.IsNullOrEmpty(multa.fecha_multa))
                ModelState.AddModelError("fecha_multa", "La fecha es obligatoria");

            // Validar descripción no vacía
            if (string.IsNullOrWhiteSpace(multa.descripcion_multa))
                ModelState.AddModelError("descripcion_multa", "La descripción es obligatoria");

            // Validar monto positivo
            if (multa.monto_multa <= 0)
                ModelState.AddModelError("monto_multa", "El monto de la multa debe ser mayor a 0");

            // Validar que la devolución exista
            using (SqlConnection con = new SqlConnection(conexion))
            {
                con.Open();
                string checkQuery = "SELECT COUNT(*) FROM Devolucion WHERE codigo_devolucion = @id";
                SqlCommand checkCmd = new SqlCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@id", multa.codigo_devolucion);
                int existe = (int)checkCmd.ExecuteScalar();

                if (existe == 0)
                    ModelState.AddModelError("codigo_devolucion", "La devolución no existe");
            }

            // Validar estado válido
            string[] estadosValidos = { "Pendiente", "Pagada", "Cancelada" };
            if (!estadosValidos.Contains(multa.estado_multa))
                ModelState.AddModelError("estado_multa", "Estado no válido");

            if (ModelState.IsValid)
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    string query = @"UPDATE Multa SET 
                                    fecha_multa = @fecha,
                                    descripcion_multa = @descripcion,
                                    monto_multa = @monto,
                                    estado_multa = @estado,
                                    codigo_devolucion = @devolucion
                                    WHERE codigo_multa = @id";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", multa.codigo_multa);
                    cmd.Parameters.AddWithValue("@fecha", multa.fecha_multa);
                    cmd.Parameters.AddWithValue("@descripcion", multa.descripcion_multa);
                    cmd.Parameters.AddWithValue("@monto", multa.monto_multa);
                    cmd.Parameters.AddWithValue("@estado", multa.estado_multa);
                    cmd.Parameters.AddWithValue("@devolucion", multa.codigo_devolucion);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Multa actualizada exitosamente";
                return RedirectToAction("Multas");
            }

            ViewBag.Title = "EDITAR MULTA";
            ViewBag.Devoluciones = ObtenerDevoluciones();
            return View(multa);
        }

        // POST: EliminarMulta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult EliminarMulta(int id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    con.Open();

                    // Eliminar multa
                    string query = "DELETE FROM Multa WHERE codigo_multa = @id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@id", id);
                    int filasAfectadas = cmd.ExecuteNonQuery();

                    if (filasAfectadas > 0)
                    {
                        return Json(new { success = true, message = "Multa eliminada exitosamente" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Multa no encontrada" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar: " + ex.Message });
            }
        }

        private List<SelectListItem> ObtenerProductos()
        {
            var productos = new List<SelectListItem>();

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = "SELECT codigo_producto, nombre_producto FROM Producto_Multimedia ORDER BY nombre_producto";
                SqlCommand cmd = new SqlCommand(query, con);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                productos.Add(new SelectListItem { Value = "", Text = "-- Seleccione un producto --" });

                while (dr.Read())
                {
                    productos.Add(new SelectListItem
                    {
                        Value = dr["codigo_producto"].ToString(),
                        Text = dr["nombre_producto"].ToString()
                    });
                }
            }

            return productos;
        }

        private List<SelectListItem> ObtenerDevoluciones()
        {
            var devoluciones = new List<SelectListItem>();

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = @"SELECT d.codigo_devolucion, r.id_renta, u.nombre_usuario 
                                FROM Devolucion d
                                INNER JOIN Renta r ON d.id_renta = r.id_renta
                                INNER JOIN Usuario u ON r.numero_identificacion_usuario = u.numero_identificacion_usuario
                                WHERE d.estado_devolucion = 'Pendiente'
                                ORDER BY d.fecha_devolucion DESC";
                SqlCommand cmd = new SqlCommand(query, con);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                devoluciones.Add(new SelectListItem { Value = "", Text = "-- Seleccione una devolución --" });

                while (dr.Read())
                {
                    devoluciones.Add(new SelectListItem
                    {
                        Value = dr["codigo_devolucion"].ToString(),
                        Text = "Devolución #" + dr["codigo_devolucion"].ToString() + " - " + dr["nombre_usuario"].ToString()
                    });
                }
            }

            return devoluciones;
        }

        // ========================
        // EDITAR USUARIO
        // ========================

        public ActionResult EditarUsuario(long id)
        {
            Usuario usuario = null;

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = "SELECT * FROM Usuario WHERE numero_identificacion_usuario=@id";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", id);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    usuario = new Usuario
                    {
                        numero_identificacion_usuario = Convert.ToInt64(dr["numero_identificacion_usuario"]),
                        tipo_identificacion_usuario = dr["tipo_identificacion_usuario"].ToString(),
                        nombre_usuario = dr["nombre_usuario"].ToString(),
                        email_usuario = dr["email_usuario"].ToString(),
                        telefono_usuario = Convert.ToInt64(dr["telefono_usuario"]),
                        direccion_usuario = dr["direccion_usuario"].ToString(),
                        contraseña_usuario = dr["contraseña_usuario"].ToString()
                    };
                }
            }

            return View(usuario);
        }

        [HttpPost]
        public ActionResult EditarUsuario(Usuario u)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = @"UPDATE Usuario SET
                        tipo_identificacion_usuario=@tipo,
                        nombre_usuario=@nombre,
                        email_usuario=@correo,
                        telefono_usuario=@telefono,
                        direccion_usuario=@direccion,
                        contraseña_usuario=@password
                        WHERE numero_identificacion_usuario=@id";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@id", u.numero_identificacion_usuario);
                cmd.Parameters.AddWithValue("@tipo", u.tipo_identificacion_usuario);
                cmd.Parameters.AddWithValue("@nombre", u.nombre_usuario);
                cmd.Parameters.AddWithValue("@correo", u.email_usuario);
                cmd.Parameters.AddWithValue("@telefono", u.telefono_usuario);
                cmd.Parameters.AddWithValue("@direccion", u.direccion_usuario);
                cmd.Parameters.AddWithValue("@password", u.contraseña_usuario);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            TempData["SuccessMessage"] = "Usuario actualizado exitosamente";
            return RedirectToAction("PanelAdmin");
        }

        //========================
        // REPORTES
        //========================

        public ActionResult Reportes()
        {
            return View();
        }

        public ActionResult ReporteUsuarios()
        {
            var panelResult = PanelAdmin(null) as ViewResult;
            var model = panelResult?.Model;
            return View("ReporteUsuarios", model);
        }

        public ActionResult ReporteRentas()
        {
            var rentasResult = Rentas() as ViewResult;
            var model = rentasResult?.Model;
            return View("ReporteRentas", model);
        }

        public ActionResult ReporteMultas()
        {
            var multasResult = Multas() as ViewResult;
            var model = multasResult?.Model;
            return View("ReporteMultas", model);
        }

        public ActionResult ReporteInventario()
        {
            var invResult = Inventario() as ViewResult;
            var model = invResult?.Model;
            return View("ReporteInventario", model);
        }

        public ActionResult ReporteProductos()
        {
            var productosResult = Productos(null) as ViewResult;
            var model = productosResult?.Model;
            return View("ReporteProductos", model);
        }

        public ActionResult DescargarReporteUsuarios()
        {
            ReportDocument rd = new ReportDocument();

            rd.Load(Server.MapPath("~/Reportes/ReporteUsuarios.rpt"));

            List<ReporteUsuarios> lista =
            new List<ReporteUsuarios>();

            using (SqlConnection con =
            new SqlConnection(conexion))
            {
                string query = @"

        SELECT 
        nombre_usuario,
        email_usuario,
        telefono_usuario,
        direccion_usuario

        FROM Usuario

        WHERE tipo_usuario = 'Cliente'";

                SqlCommand cmd =
                new SqlCommand(query, con);

                con.Open();

                SqlDataReader dr =
                cmd.ExecuteReader();

                while (dr.Read())
                {
                    lista.Add(new ReporteUsuarios
                    {
                        nombre_usuario =
                        dr["nombre_usuario"].ToString(),

                        email_usuario =
                        dr["email_usuario"].ToString(),

                        telefono_usuario =
                        Convert.ToInt64(
                        dr["telefono_usuario"]),

                        direccion_usuario =
                        dr["direccion_usuario"]
                        .ToString()
                    });
                }
            }

            rd.SetDataSource(lista);

            Stream stream =
            rd.ExportToStream(
            ExportFormatType.PortableDocFormat);

            return File(
            stream,
            "application/pdf",
            "ReporteUsuarios.pdf");
        }

        public ActionResult DescargarReporteInventario()
        {
            ReportDocument rd = new ReportDocument();

            rd.Load(Server.MapPath("~/Reportes/ReporteInventario.rpt"));

            List<ReporteInventario> lista =
            new List<ReporteInventario>();

            using (SqlConnection con =
            new SqlConnection(conexion))
            {
                string query = @"

        SELECT 
        i.codigo_inventario,
        i.cantidad_inventario,
        i.ubicacion_producto_inventario,

        p.nombre_producto,
        p.tipo_producto,
        p.precio_producto,
        p.cantidad_disponible_producto

        FROM Inventario i

        INNER JOIN Producto_Multimedia p
        ON i.codigo_inventario =
        p.codigo_inventario";

                SqlCommand cmd =
                new SqlCommand(query, con);

                con.Open();

                SqlDataReader dr =
                cmd.ExecuteReader();

                while (dr.Read())
                {
                    lista.Add(new ReporteInventario
                    {
                        codigo_inventario =
                        Convert.ToInt32(
                        dr["codigo_inventario"]),

                        cantidad_inventario =
                        Convert.ToInt32(
                        dr["cantidad_inventario"]),

                        ubicacion_producto_inventario =
                        dr["ubicacion_producto_inventario"]
                        .ToString(),

                        nombre_producto =
                        dr["nombre_producto"]
                        .ToString(),

                        tipo_producto =
                        dr["tipo_producto"]
                        .ToString(),

                        precio_producto =
                        Convert.ToDouble(
                        dr["precio_producto"]),

                        cantidad_disponible_producto =
                        Convert.ToInt32(
                        dr["cantidad_disponible_producto"])
                    });
                }
            }

            rd.SetDataSource(lista);

            Stream stream =
            rd.ExportToStream(
            ExportFormatType.PortableDocFormat);

            return File(
            stream,
            "application/pdf",
            "ReporteInventario.pdf");
        }

        public ActionResult DescargarReporteRentas()
        {
            ReportDocument rd = new ReportDocument();

            rd.Load(Server.MapPath("~/Reportes/ReporteRentas.rpt"));

            List<ReporteRenta> lista =
            new List<ReporteRenta>();

            using (SqlConnection con =
            new SqlConnection(conexion))
            {
                string query = @"

        SELECT 
        u.nombre_usuario,
        u.telefono_usuario,

        r.fecha_renta,
        r.fecha_devolucion_renta,
        r.estado_renta

        FROM Renta r

        INNER JOIN Usuario u
        ON r.numero_identificacion_usuario =
        u.numero_identificacion_usuario

        WHERE r.estado_renta = 'Activa'";

                SqlCommand cmd =
                new SqlCommand(query, con);

                con.Open();

                SqlDataReader dr =
                cmd.ExecuteReader();

                while (dr.Read())
                {
                    lista.Add(new ReporteRenta
                    {
                        nombre_usuario =
                        dr["nombre_usuario"].ToString(),

                        telefono_usuario =
                        Convert.ToInt64(
                        dr["telefono_usuario"]),

                        fecha_renta =
                        Convert.ToDateTime(
                        dr["fecha_renta"]),

                        fecha_devolucion_renta =
                        Convert.ToDateTime(
                        dr["fecha_devolucion_renta"]),

                        estado_renta =
                        dr["estado_renta"].ToString()
                    });
                }
            }

            rd.SetDataSource(lista);

            Stream stream =
            rd.ExportToStream(
            ExportFormatType.PortableDocFormat);

            return File(
            stream,
            "application/pdf",
            "ReporteRentas.pdf");
        }

        public ActionResult DescargarReporteMultas()
        {
            ReportDocument rd = new ReportDocument();

            rd.Load(Server.MapPath("~/Reportes/ReporteMultas.rpt"));

            List<ReporteMulta> lista =
            new List<ReporteMulta>();

            using (SqlConnection con =
            new SqlConnection(conexion))
            {
                string query = @"

        SELECT 
        u.nombre_usuario,
        u.email_usuario,

        m.descripcion_multa,
        m.monto_multa,
        m.estado_multa,
        m.fecha_multa

        FROM Multa m

        INNER JOIN Devolucion d
        ON m.codigo_devolucion =
        d.codigo_devolucion

        INNER JOIN Renta r
        ON d.id_renta =
        r.id_renta

        INNER JOIN Usuario u
        ON r.numero_identificacion_usuario =
        u.numero_identificacion_usuario";

                SqlCommand cmd =
                new SqlCommand(query, con);

                con.Open();

                SqlDataReader dr =
                cmd.ExecuteReader();

                while (dr.Read())
                {
                    lista.Add(new ReporteMulta
                    {
                        nombre_usuario =
                        dr["nombre_usuario"].ToString(),

                        email_usuario =
                        dr["email_usuario"].ToString(),

                        descripcion_multa =
                        dr["descripcion_multa"]
                        .ToString(),

                        monto_multa =
                        Convert.ToDouble(
                        dr["monto_multa"]),

                        estado_multa =
                        dr["estado_multa"].ToString(),

                        fecha_multa =
                        dr["fecha_multa"].ToString()
                    });
                }
            }

            rd.SetDataSource(lista);

            Stream stream =
            rd.ExportToStream(
            ExportFormatType.PortableDocFormat);

            return File(
            stream,
            "application/pdf",
            "ReporteMultas.pdf");

        }
    }
}

