using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Excel;
using PagedList;
using PaginaGabriel.Classes;
using PaginaGabriel.Models;
using PaginaGabriel.ViewModels;

namespace PaginaGabriel.Controllers
{
    public class ProductosController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Productos
        public ActionResult Index()
        {
       

            var productos = db.Productos.Include(p => p.Categoria).Include(p => p.Marca).Include(p => p.SubCategoria);
            return View(productos.ToList());
        }

        // GET: Productos
        public ActionResult Store(string orden, string currentFilter, string searchString, int? page, int paginaCantidad = 50)
        {

            //var productos = db.Productos.Include(p => p.Categoria).Include(p => p.Marca).Include(p => p.SubCategoria).Where(p => p.Estado == true);
            //return View(productos.ToList());


            //    //Asignamos la cantidad de registros a mostrar para evitar errores en la vista.
            ViewBag.Cantidad = paginaCantidad;



            //Realizamos la busqueda.
            var productos = db.Productos.Include(p => p.Categoria)
                .Include(p => p.Marca)
                .Include(p => p.SubCategoria)
                .Where(p => p.Estado == true);
            //.OrderBy(p => p.Categoria.Nombre);

            //        .OrderBy(p => p.Descripcion);
            //        //.ToPagedList(paginaNumero, paginaCantidad);

            // manejamos la busqueda
            if (searchString != null)
            {
                page = 1;
                searchString = searchString.ToUpper();
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            if (!String.IsNullOrEmpty(searchString))
            {
                productos = productos.Where(s => s.Categoria.Nombre.ToUpper().Contains(searchString)
                                       || s.Marca.Nombre.ToUpper().Contains(searchString)
                                       || s.SubCategoria.Nombre.ToUpper().Contains(searchString)
                                       || s.Descripcion.ToUpper().Contains(searchString)
                                       || s.Observaciones.ToUpper().Contains(searchString)
                                       || s.Precio.ToString().Contains(searchString)
                                       || s.Stock.ToString().Contains(searchString));
            }

            //Agregamos la Paginacion                
            int paginaNumero = (page ?? 1);
            ViewBag.Documentos = productos
                .OrderBy(x => x.Categoria.Nombre)
                .ToPagedList(paginaNumero, paginaCantidad);
            return View();
        }
    


    // GET: Productos/Details/5
    public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Producto producto = db.Productos.Find(id);
            if (producto == null)
            {
                return HttpNotFound();
            }
            return View(producto);
        }

        // GET: Productos/Create
        public ActionResult Create()
        {
            var productoViewModel = new ProductoViewModel();
            productoViewModel.Estado = true;

            ViewBag.CategoriaId = new SelectList(CombosHelper.GetCategorias(), "CategoriaId", "Nombre");
            ViewBag.MarcaId = new SelectList(CombosHelper.GetMarcas(), "MarcaId", "Nombre");
            ViewBag.SubCategoriaId = new SelectList(CombosHelper.GetSubCategorias(0), "SubCategoriaId", "Nombre");
            return View(productoViewModel);
        }

        // POST: Productos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProductoViewModel productoViewModel)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = db.Database.BeginTransaction())
                {

                    try
                    {
                        var producto = ToModel(productoViewModel);
                        db.Productos.Add(producto);
                        db.SaveChanges();

                        if (productoViewModel.ArchivoImagen != null)
                        {
                            var folder = "~/Content/Productos";
                            var file = $"{producto.ProductoId}.jpg";

                            var response = FileHelper.UploadPhoto(productoViewModel.ArchivoImagen, folder, file);
                            if (response)
                            {
                                var pic = $"{folder}/{file}";
                                producto.Imagen = pic;
                                db.Entry(producto).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        transaction.Commit();

                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException != null &&
                            ex.InnerException.InnerException != null &&
                            ex.InnerException.InnerException.Message.Contains("IX"))
                        {
                            transaction.Rollback();
                            ModelState.AddModelError(string.Empty, "Este Producto ya existe.");
                        }
                        else
                        {
                            transaction.Rollback();
                            ModelState.AddModelError(string.Empty, ex.Message);
                            ViewBag.CategoriaId = new SelectList(CombosHelper.GetCategorias(), "CategoriaId", "Nombre", productoViewModel.CategoriaId);
                            ViewBag.MarcaId = new SelectList(CombosHelper.GetMarcas(), "MarcaId", "Nombre", productoViewModel.MarcaId);
                            ViewBag.SubCategoriaId = new SelectList(CombosHelper.GetSubCategorias(productoViewModel.CategoriaId), "SubCategoriaId", "Nombre", productoViewModel.SubCategoriaId);
                            return View(productoViewModel);
                        }


                    }
                }
            }

            ViewBag.CategoriaId = new SelectList(CombosHelper.GetCategorias(), "CategoriaId", "Nombre", productoViewModel.CategoriaId);
            ViewBag.MarcaId = new SelectList(CombosHelper.GetMarcas(), "MarcaId", "Nombre", productoViewModel.MarcaId);
            ViewBag.SubCategoriaId = new SelectList(CombosHelper.GetSubCategorias(productoViewModel.CategoriaId), "SubCategoriaId", "Nombre", productoViewModel.SubCategoriaId);
            return View(productoViewModel);
        }

        private Producto ToModel(ProductoViewModel productoViewModel)
        {
            return new Producto
            {
                ProductoId = productoViewModel.ProductoId,
                CategoriaId = productoViewModel.CategoriaId,
                SubCategoriaId = productoViewModel.SubCategoriaId,
                MarcaId = productoViewModel.MarcaId,
                Descripcion = productoViewModel.Descripcion,
                Imagen = productoViewModel.Imagen,
                Precio = productoViewModel.Precio,
                Stock = productoViewModel.Stock,
                Observaciones = productoViewModel.Observaciones,
                Estado = productoViewModel.Estado
            };
        }

        // GET: Productos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Producto producto = db.Productos.Find(id);
            if (producto == null)
            {
                return HttpNotFound();
            }

            var productoViewModel = ToViewModel(producto);

            ViewBag.CategoriaId = new SelectList(CombosHelper.GetCategorias(), "CategoriaId", "Nombre", producto.CategoriaId);
            ViewBag.MarcaId = new SelectList(CombosHelper.GetMarcas(), "MarcaId", "Nombre", producto.MarcaId);
            ViewBag.SubCategoriaId = new SelectList(CombosHelper.GetSubCategorias(producto.CategoriaId), "SubCategoriaId", "Nombre", producto.SubCategoriaId);
            return View(productoViewModel);
        }

        private ProductoViewModel ToViewModel(Producto producto)
        {
            return new ProductoViewModel
            {
                ProductoId = producto.ProductoId,
                CategoriaId = producto.CategoriaId,
                SubCategoriaId = producto.SubCategoriaId,
                MarcaId = producto.MarcaId,
                Descripcion = producto.Descripcion,
                Imagen = producto.Imagen,
                Precio = producto.Precio,
                Stock = producto.Stock,
                Observaciones = producto.Observaciones,
                Estado = producto.Estado
            };
        }

        // POST: Productos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProductoViewModel productoViewModel)
        {
            if (ModelState.IsValid)
            {

                try
                {
                    var producto = ToModel(productoViewModel);

                    if (productoViewModel.ArchivoImagen != null)
                    {
                        var pic = string.Empty;
                        var folder = "~/Content/Productos";
                        var file = $"{producto.ProductoId}.jpg";
                        var response = FileHelper.UploadPhoto(productoViewModel.ArchivoImagen, folder, file);

                        if (response)
                        {
                            pic = $"{folder}/{file}";
                            producto.Imagen = pic;
                        }
                    }

                    db.Entry(producto).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null &&
                        ex.InnerException.InnerException != null &&
                        ex.InnerException.InnerException.Message.Contains("IX"))
                    {
                        ModelState.AddModelError(string.Empty, "Este Producto ya existe.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.Message);
                    }
                }

            }
            ViewBag.CategoriaId = new SelectList(CombosHelper.GetCategorias(), "CategoriaId", "Nombre", productoViewModel.CategoriaId);
            ViewBag.MarcaId = new SelectList(CombosHelper.GetMarcas(), "MarcaId", "Nombre", productoViewModel.MarcaId);
            ViewBag.SubCategoriaId = new SelectList(CombosHelper.GetSubCategorias(productoViewModel.CategoriaId), "SubCategoriaId", "Nombre", productoViewModel.SubCategoriaId);
            return View(productoViewModel);
        }

        // GET: Productos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Producto producto = db.Productos.Find(id);
            if (producto == null)
            {
                return HttpNotFound();
            }
            return View(producto);
        }

        // POST: Productos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Producto producto = db.Productos.Find(id);
            db.Productos.Remove(producto);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult ImportarProductos()
        {
            var lista = new List<Producto>();
            return View(lista);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ImportarProductos(HttpPostedFileBase upload)
        {
            var lista = new List<Producto>();
            try
            {
                if (ModelState.IsValid)
                {

                    if (upload != null && upload.ContentLength > 0)
                    {
                        // ExcelDataReader works with the binary Excel file, so it needs a FileStream
                        // to get started. This is how we avoid dependencies on ACE or Interop:
                        Stream stream = upload.InputStream;

                        // We return the interface, so that
                        IExcelDataReader reader = null;


                        // Determinamos el tipo de documento
                        if (upload.FileName.EndsWith(".xls"))
                        {
                            reader = ExcelReaderFactory.CreateBinaryReader(stream);
                        }
                        else if (upload.FileName.EndsWith(".xlsx"))
                        {
                            reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                        }
                        else
                        {
                            ModelState.AddModelError("File", "Formato de Archivo no soportado");
                            return View(lista);
                        }

                        reader.IsFirstRowAsColumnNames = true;

                        DataSet result = reader.AsDataSet();
                        reader.Close();

                        var tabla = result.Tables[0];

                        foreach (DataRow row in tabla.Rows)
                        {
                            //string codigoEmpresa = Convert.ToString(row["Empresa"]);
                            //var empresa = db.Empresas.SingleOrDefault(x => x.CodigoEmpresa == codigoEmpresa);
                            //int? empresaId = null;
                            //if (empresa != null) empresaId = empresa.EmpresaID;

                            string codigoCategoria = Convert.ToString(row["Categoría"]);
                            var categoria = db.Categorias.SingleOrDefault(x => x.Nombre == codigoCategoria);
                            //int categoriaId = null;
                            //if (categoria != null) 
                            int categoriaId = categoria.CategoriaId;

                            string codigoSubCategoria = Convert.ToString(row["SubCategoría"]);
                            var subCategoria = db.SubCategorias.SingleOrDefault(x => x.Nombre == codigoSubCategoria);
                            //int subCategoriaId = null;
                            //if (subCategoria != null)
                            int subCategoriaId = subCategoria.SubCategoriaId;

                            string codigoMarca = Convert.ToString(row["Marca"]);
                            var marca = db.Marcas.SingleOrDefault(x => x.Nombre == codigoMarca);
                            //int marcaId = null;
                            //if (marca != null)
                            int marcaId = marca.MarcaId;


                            //string cuenta = Convert.ToString(row["Cuenta"]);
                            //var usuariocuenta = db.Usuarios.SingleOrDefault(x => x.Cuenta == cuenta);

                            //if (usuariocuenta == null)
                            //{
                                var producto = new Producto
                                {
                                    Categoria = categoria,
                                    CategoriaId = categoriaId,

                                    SubCategoria = subCategoria,
                                    SubCategoriaId = subCategoriaId,

                                    Marca = marca,
                                    MarcaId = marcaId,

                                    Descripcion = Convert.ToString(row["Producto"]),
                                    Precio = Convert.ToDecimal(row["Precio"]),
                                    Stock = Convert.ToInt32(row["Stock"]),
                                    Observaciones = Convert.ToString(row["Descripción"]),
                                    Estado = Convert.ToBoolean(row["Estado"]),
                                    Imagen = Convert.ToString(row["Imagen"]),

                                };
                                lista.Add(producto);
                                db.Productos.Add(producto);
                            //}
                        }
                        db.SaveChanges();
                        return View(lista);
                    }
                    else
                    {
                        ModelState.AddModelError("File", "Por favor suba un archivo");
                    }
                }
                return View(lista);
            }
            catch (Exception ex)
            {
                Exception realerror = ex;
                //while (realerror.InnerException != null)
                //    realerror = realerror.InnerException;
                //ViewBag.Error = "Ocurrio un error inesperado: " + realerror.Message;
                //lista = new List<Usuario>();
                return View(lista);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public JsonResult GetSubCategorias(int categoriaId)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var subCategorias = db.SubCategorias.Where(m => m.CategoriaId == categoriaId);
            return Json(subCategorias);
        }
    }
}
