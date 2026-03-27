using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services;
using System.Web.Script.Services;

namespace FlowMediaWeb.ACliente
{
    public partial class Catalogo : System.Web.UI.Page
    {

        /* ==========================================
           cargar catalogo y filtros al cargar la pagina No lo toque angelly
        ==========================================*/
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadCatalog();
                LoadFilters();
            }
        }


        /* ==========================================
           CArgar catalogo completo (sin filtro) al repeater
        ==========================================*/
        private void LoadCatalog()
        {
            var items = CatalogRepository.GetItems();

            rptCatalog.DataSource = items;
            rptCatalog.DataBind();
        }


        /* ==========================================
           Cargar filtros (categorias) para el dropdown, se obtiene de los items del catalogo
        ==========================================*/
        private void LoadFilters()
        {
            var items = CatalogRepository.GetItems();

            var filters = new List<object>
            {
                new { Key = "all", Name = "Todo" }
            };

            filters.AddRange(
                items
                .Select(x => new
                {
                    Key = x.FilterKey,
                    Name = x.Category
                })
                .Distinct()
            );

            rptFilters.DataSource = filters;
            rptFilters.DataBind();
        }


        /* ==========================================
           AJAX metodo (SPA DETAIL LOAD)
        ==========================================*/
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static ItemDetailsDto GetItemDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            var item = CatalogRepository
                .GetItems()
                .FirstOrDefault(x => x.Id.ToString() == id);

            if (item == null)
                return null;


            /* ===== REVIEWS MOCK ===== */

            var reviews = new List<ReviewDto>
            {
                new ReviewDto
                {
                    User="Carlos",
                    Rating=5,
                    Comment="Excelente contenido!"
                },
                new ReviewDto
                {
                    User="Laura",
                    Rating=4,
                    Comment="Muy recomendado."
                }
            };


            /* ===== Precio opciones ===== */

            var culture =
                System.Globalization.CultureInfo
                .GetCultureInfo("es-CO");

            var prices = new List<PriceOptionDto>
            {
                new PriceOptionDto
                {
                    Label="Renta 3 días",
                    Price=item.Price*0.6m,
                    PriceFormatted=
                        string.Format(culture,"{0:C0}",item.Price*0.6m)
                },

                new PriceOptionDto
                {
                    Label="Renta 7 días",
                    Price=item.Price,
                    PriceFormatted=
                        string.Format(culture,"{0:C0}",item.Price)
                },

                new PriceOptionDto
                {
                    Label="Compra definitiva",
                    Price=item.Price*3,
                    PriceFormatted=
                        string.Format(culture,"{0:C0}",item.Price*3)
                }
            };


            return new ItemDetailsDto
            {
                Id = item.Id,
                Title = item.Title,
                Category = item.Category,
                Description = item.Description,
                ImageUrl = item.ImageUrl,
                AverageRating =
                    reviews.Any()
                    ? Math.Round(reviews.Average(r => r.Rating), 1)
                    : 0,
                Reviews = reviews,
                PriceOptions = prices
            };
        }
    }



    /* ==========================================
       REPOSITORY (SIMULA BASE DATOS) ya que Jair No la  a hecho
    ==========================================*/
    public static class CatalogRepository
    {
        public static List<CatalogItem> GetItems()
        {
            return new List<CatalogItem>
            {
                new CatalogItem{
                    Id=1,
                    Title="Grand Theft Auto V",
                    Category="Videojuego",
                    FilterKey="Videojuego",
                    Description="Explora Los Santos y vive el crimen.",
                    Price=49300,
                    ImageUrl=
                    VirtualPathUtility.ToAbsolute("~/Imagenes/GTAV.png")
                },

                new CatalogItem{
                    Id=2,
                    Title="Minecraft",
                    Category="Videojuego",
                    FilterKey="Videojuego",
                    Description="Construye y explora mundos infinitos.",
                    Price=28850,
                    ImageUrl=
                    VirtualPathUtility.ToAbsolute("~/Imagenes/Minecraft.jpg")
                },

                new CatalogItem{
                    Id=3,
                    Title="The Matrix Trilogy",
                    Category="Pelicula",
                    FilterKey="Pelicula",
                    Description="Neo descubre la verdad.",
                    Price=5400,
                    ImageUrl=
                    VirtualPathUtility.ToAbsolute("~/Imagenes/Matrix.jpg")
                },

                new CatalogItem{
                    Id=4,
                    Title="The Addams Family",
                    Category="Pelicula",
                    FilterKey="Pelicula",
                    Description="Comedia oscura familiar.",
                    Price=5400,
                    ImageUrl=
                    VirtualPathUtility.ToAbsolute("~/Imagenes/Addams.jpg")
                }
            };
        }
    }



    /* ==========================================
       MODELOS Jair chambea con esto, no se preocupen
    ==========================================*/

    public class CatalogItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string FilterKey { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
    }


    public class ItemDetailsDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public double AverageRating { get; set; }
        public List<ReviewDto> Reviews { get; set; }
        public List<PriceOptionDto> PriceOptions { get; set; }
    }


    public class ReviewDto
    {
        public string User { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }


    public class PriceOptionDto
    {
        public string Label { get; set; }
        public decimal Price { get; set; }
        public string PriceFormatted { get; set; }
    }

}