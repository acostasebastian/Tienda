using PaginaGabriel.Classes;
using PaginaGabriel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using MercadoPago;
using MercadoPago.Resources;
using MercadoPago.DataStructures.Preference;


namespace PaginaGabriel.Controllers
{
    public class CheckOutController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        const String PromoCode = "FREE";
        // GET: CheckOut
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Pago(int? id)
        {

            using (var tran = db.Database.BeginTransaction())
            {

                var cart = ShoppingCart.GetCart(this.HttpContext);
                var productosCarritos = cart.GetCartItems();

                //var idCarrito = cart.ShoppingCartId;
                var idCarrito = User.Identity.Name;

                var totalCarrito = cart.GetTotal();


                try
                {
                    foreach (var item in productosCarritos)
                    {
                        int cantidad = item.Cantidad;
                    
                    
                        var stockProducto = db.Productos.Find(item.ProductoId).Stock;

                        var nombre = db.Productos.Find(item.ProductoId).Descripcion;
                        var Productos = db.Productos.FirstOrDefault(p => p.ProductoId == item.Producto.ProductoId);


                        if (cantidad > stockProducto)
                        {
                            tran.Rollback();

                            TempData["mensaje"] = "No hay Stock suficiente de " + item.Producto.Descripcion;

                            return RedirectToAction("IndexError", "ShoppingCart");

                        }

                        else
                        {
                            stockProducto = stockProducto - cantidad;

                            var model = new PedidosCliente();

                            model.Cantidad = totalCarrito;
                            ViewData["Cant"] = totalCarrito;

                            if (idCarrito != "")
                            {
                                var nombreUsuario = db.Users.SingleOrDefault(c => c.Email == idCarrito).Nombres;
                                var apellidoUsuario = db.Users.SingleOrDefault(c => c.Email == idCarrito).Apellido;
                                var telefonoUsuario = db.Users.SingleOrDefault(c => c.Email == idCarrito).PhoneNumber;
                                var DNIUsuario = db.Users.SingleOrDefault(c => c.Email == idCarrito).DNI;

                                model.Email = idCarrito;
                                model.Nombres = nombreUsuario;
                                model.Apellido = apellidoUsuario;
                                model.Telefono = telefonoUsuario;
                                model.DNI = DNIUsuario;


                                ViewData["Usuario"] = User.Identity.Name;
                                ViewData["Nombre"] = nombreUsuario;
                                ViewData["Apellido"] = apellidoUsuario;
                                ViewData["Telefono"] = telefonoUsuario;
                                ViewData["DNI"] = DNIUsuario;
                                                                                                                           

                            }
                            else
                            {

                                model.Email = idCarrito;
                                model.Nombres = "";
                                model.Apellido = "";
                                model.Telefono = "";
                                model.DNI = "";
                            }

                            //Mercado Pago

                            MercadoPago.SDK.ClientId = "1782351866703164";
                            MercadoPago.SDK.ClientSecret = "6nNax7XMsv6y4qiKxDhEJ4zmZZlCmlCN";

                            // Create a preference object
                            Preference preference = new Preference();
                            //# Adding an item object
                            preference.Items.Add(
                              new Item()
                              {
                                  Id = model.Id.ToString(),
                                  Title = "Registrar Pago de: " + model.NombreCompleto,
                                  Quantity = 1,
                                  CurrencyId = 0,
                                  UnitPrice = totalCarrito
                              }
                            );
                            // Setting a payer object as value for Payer property
                            preference.Payer = new Payer()
                            {
                                Email = model.Email,
                                Name = model.Nombres,
                                Surname = model.Apellido,
                                Phone = new Phone
                                {
                                    Number = model.Telefono
                                },

                                Date_created = model.FechaCreacion,


                                Identification = new Identification
                                {
                                    Type = "DNI",
                                    Number = model.DNI,
                                }

                            };
                            // Save and posti   ng preference
                            preference.Save();

                            model.UrlMercadoPago = preference.InitPoint;
                            ViewData["MP"] = model.UrlMercadoPago;



                            db.SaveChanges();
                            tran.Commit();

                            return View();
                        }
                    }

                  

                    

                }


                catch (Exception ex)
                {

                    if (ex.InnerException != null &&
                       ex.InnerException.InnerException != null &&
                       ex.InnerException.InnerException.Message.Contains("No"))
                    {
                        ModelState.AddModelError(string.Empty, "The field already exists");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "The field already exists 2");
                    }

                    return RedirectToAction("Index", "ShoppingCart");

                }
                tran.Rollback();
                return View();


            }
        }

        [HttpPost]
        public ActionResult Pago(FormCollection values)
        {
            using (var tran = db.Database.BeginTransaction())
            {

                var order = new PedidosCliente();

                TryUpdateModel(order);

                try
                {
                    //if (string.Equals(values["PromoCode"], PromoCode, StringComparison.OrdinalIgnoreCase) == false)
                    //{
                    //    return View(order);
                    //}
                    //else
                    //{

                    var masErrores = false;

                    if (String.IsNullOrEmpty(order.Apellido))
                    {
                        ModelState.AddModelError(string.Empty, "");
                        masErrores = true;
                    }

                    else if (String.IsNullOrEmpty(order.Nombres))
                    {
                        ModelState.AddModelError(string.Empty, "");
                        masErrores = true;
                    }

                    else if (String.IsNullOrEmpty(order.Email))
                    {
                        ModelState.AddModelError(string.Empty, "");
                        masErrores = true;
                    }

                    else if (String.IsNullOrEmpty(order.DNI))
                    {
                        ModelState.AddModelError(string.Empty, "");
                        masErrores = true;
                    }

                    if (masErrores)
                    {

                        return View("Pago", order);
                    }

                    order.FechaCreacion = DateTime.Now;
                    order.Estado = true;

                   var cart = ShoppingCart.GetCart(this.HttpContext);

                    var productosCarritos = cart.GetCartItems();
                    order.Cantidad = cart.GetTotal();
                    
                    if (User.Identity.Name != "")
                    {

                        var nombreUsuario = db.Users.SingleOrDefault(c => c.Email == User.Identity.Name).Nombres;
                        var apellidoUsuario = db.Users.SingleOrDefault(c => c.Email == User.Identity.Name).Apellido;
                        var telefonoUsuario = db.Users.SingleOrDefault(c => c.Email == User.Identity.Name).PhoneNumber;
                        var DNIUsuario = db.Users.SingleOrDefault(c => c.Email == User.Identity.Name).DNI;

                        order.Email = User.Identity.Name;
                        order.Nombres = nombreUsuario;
                        order.Apellido = apellidoUsuario;
                        order.Telefono = telefonoUsuario;
                        order.DNI = DNIUsuario;

                    }

                    MailMessage correo = new MailMessage();


                    string texto = "Detalle de su compra: " + Environment.NewLine;
                    foreach (var item in productosCarritos)
                    {

                        var Productos = db.Productos.FirstOrDefault(p => p.ProductoId == item.Producto.ProductoId);

                        int cantidadProducto = item.Cantidad;
                        decimal precio = Productos.Precio;

                        texto = texto + Environment.NewLine + "-" + Productos.Descripcion.ToString() + "- Cantidad: " + cantidadProducto + "- Precio: " + precio;

                    }

                    texto = texto + Environment.NewLine + Environment.NewLine + "Total a pagar: " + order.Cantidad;
                    correo.Body = texto;

                    db.PedidosCliente.Add(order);
                    db.SaveChanges();

                    cart.CreateOrder(order);


                    correo.From = new MailAddress("maurosebastiandesarrollo@gmail.com");
                    correo.To.Add(order.Email);
                    correo.Bcc.Add("sebastian_acosta85@hotmail.com");

                    int id = order.Id;
                    correo.Subject = "Compra Nº " + order.Id;

                    correo.IsBodyHtml = false;
                    correo.Priority = MailPriority.Normal;

                    SmtpClient smtp = new SmtpClient();

                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 25;
                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = true;
                    string sCuentaCorreo = "maurosebastiandesarrollo@gmail.com";
                    string sPasswordCorreo = "mauseba.";
                    smtp.Credentials = new System.Net.NetworkCredential(sCuentaCorreo, sPasswordCorreo);

                    smtp.Send(correo);
                    ViewBag.Mensaje = "Correo enviado";



                    db.SaveChanges();//we have received the total amount lets update it
                    tran.Commit();

                    return RedirectToAction("Completo", new { id = order.Id });

                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    ex.InnerException.ToString();
                    return View(order);
                }
            }
        }

        public ActionResult Completo(int id)
        {
            bool isValid = false;

            if (User.Identity.Name != "")
            {
                isValid =
                    db.PedidosCliente.Any(
                o => o.Id == id &&
                     o.Email == User.Identity.Name
                );


            }

            else
            {
                isValid =
                    db.PedidosCliente.Any(
                o => o.Id == id
                );


            }

            if (isValid == true)
            {
                return View(id);
            }
            else
            {
                return View("Error");
            }


        }

        public JsonResult GetCiudades(int provinciaId)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var ciudades = db.Ciudades.Where(m => m.ProvinciaId == provinciaId);
            return Json(ciudades);
        }

    }
}