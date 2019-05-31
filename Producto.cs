using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PaginaGabriel.Models
{
    [Table("Productos")]

    public class Producto
    {
        [Key]
        public int ProductoId { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una Categoría")]
        [Display(Name = "Categoría")]
        [Index("IX_Producto_Descripcion", Order = 1, IsUnique = true)]
        public int CategoriaId { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una SubCategoría")]
        [Display(Name = "SubCategoría")]
        [Index("IX_Producto_Descripcion", Order = 2, IsUnique = true)]
        public int SubCategoriaId { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una Marca")]
        [Display(Name = "Marca")]
        [Index("IX_Producto_Descripcion", Order = 3, IsUnique = true)]
        public int MarcaId { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(100, ErrorMessage = "El campo debe tener entre {2} y {1} caracteres", MinimumLength = 2)]
        [Index("IX_Producto_Descripcion", Order = 4, IsUnique = true)]
        [Display(Name = "Producto")]
        public string Descripcion { get; set; }


        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Range(1, double.MaxValue, ErrorMessage = "Debe ingresar un precio entre {1} y {2}")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal Precio { get; set; }

        //[Required(ErrorMessage = "El campo {0} es requerido")]
        //[Range(1, double.MaxValue, ErrorMessage = "Debe ingresar un precio entre {1} y {2}")]
        //[DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        //public decimal PrecioUsuarioRegistrado { get; set; }

        [DataType(DataType.ImageUrl)]
        public string Imagen { get; set; }

        [Display(Name = "Descripción")]
        [DataType(DataType.MultilineText)]
        public string Observaciones { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Debe ingresar un precio entre {1} y {2}")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int Stock { get; set; }

        [NotMapped]
        [Display(Name = "Cantidad")]
        [Range(0, int.MaxValue, ErrorMessage = "Debe ingresar un precio entre {1} y {2}")]        
        public int CantidadComprar { get; set; }

        [Display(Name = "Activo")]
        public bool Estado { get; set; }

        public virtual Categoria Categoria { get; set; }
        public virtual SubCategoria SubCategoria { get; set; }
        public virtual Marca Marca { get; set; }

        public virtual ICollection<ProductosPedidos> ProductosPedidos { get; set; }
    }
}