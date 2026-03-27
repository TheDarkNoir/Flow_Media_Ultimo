<%@ Page Language="C#" MasterPageFile="~/General.Master"
    AutoEventWireup="true"
    CodeBehind="Catalogo.aspx.cs"
    Inherits="FlowMediaWeb.ACliente.Catalogo" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    <div id="catalogApp">

    <!-- ========================= -->
    <!-- ====== CATALOGO ====== -->
    <!-- ========================= -->
    <section id="catalogSection">

        <div class="catalog-header">

            <h1 class="catalog-title">EXPLORAR CATÁLOGO</h1>

            <div class="catalog-controls">

                <div class="catalog-search">
                    🔍
                    <input id="txtSearchCatalog"
                           type="text"
                           placeholder="Buscar juegos o películas..." />
                </div>

                <div class="catalog-filters">
                    <asp:Repeater ID="rptFilters" runat="server">
                        <ItemTemplate>
                            <button type="button"
                                class="filter-btn"
                                data-filter='<%# Eval("Key") %>'>
                                <%# Eval("Name") %>
                            </button>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>

            </div>8
        </div>


        <!-- GRID -->
        <div class="catalog-grid" id="catalogGrid">

            <asp:Repeater ID="rptCatalog" runat="server">
                <ItemTemplate>

                    <div class="catalog-card"
                         data-id='<%# Eval("Id") %>'
                         data-filter='<%# Eval("FilterKey") %>'>

                        <div class="thumb">
                            <img src='<%# Eval("ImageUrl") %>'
                                 alt='<%# Eval("Title") %>' />
                            <div class="badge">
                                <%# Eval("FilterKey") %>
                            </div>
                        </div>

                        <div class="meta">

                            <div class="category">
                                <%# Eval("Category") %>
                            </div>

                            <h3><%# Eval("Title") %></h3>

                            <p>
                                <%# Eval("Description") %>
                            </p>

                            <div class="price">
                                <span>Renta desde</span>
                                <span>
                                <%# String.Format(
                                System.Globalization.CultureInfo
                                .GetCultureInfo("es-CO"),
                                "{0:C0}", Eval("Price")) %>
                                </span>
                            </div>

                        </div>

                    </div>

                </ItemTemplate>
            </asp:Repeater>

        </div>

    </section>



    <!-- ========================= -->
    <!-- ====== DETALLE ====== -->
    <!-- ========================= -->
    <section id="detailSection">

        <button id="btnBack" class="back-btn">
            ← Volver
        </button>

        <div class="detail-inner">

            <div class="detail-image">
                <img id="detailImage" src="" alt="Detalle" />
            </div>

            <div class="detail-info">

                <h2 id="detailTitle"></h2>

                <p id="detailCategory"
                   class="category small"></p>

                <p id="detailDescription"></p>

                <div id="detailPrices"
                     class="price-options"></div>

                <div class="detail-actions">
                    <button id="btnPrimaryAction"
                        class="primary-action">
                        Comprar / Rentar
                    </button>
                </div>

                <div class="detail-reviews">
                    <h3>Opiniones de la comunidad</h3>

                    <div id="detailRating">
                        Sin calificaciones
                    </div>

                    <div id="detailReviewsList"></div>

                </div>

            </div>

        </div>

    </section>

</div>



<script>

    (function () {

        const app = document.getElementById("catalogApp");

        /* ============================
           CLICK CARD
        ============================ */

        document.addEventListener("click", function (e) {

            const card = e.target.closest(".catalog-card");
            if (!card) return;

            const id = card.dataset.id;

            PageMethods.GetItemDetails(
                id,
                function (data) {

                    loadDetail(data);

                    app.classList.add("app-show-detail");

                    history.pushState(
                        { id: data.Id },
                        "",
                        "?item=" + data.Id
                    );
                });
        });


        /* ============================
           BOTON VOLVER
        ============================ */

        document
            .getElementById("btnBack")
            .addEventListener("click", function () {

                app.classList.remove("app-show-detail");
                history.back();
            });


        /* ============================
           LOAD DETAIL
        ============================ */

        function loadDetail(data) {

            document.getElementById("detailImage").src =
                data.ImageUrl;

            document.getElementById("detailTitle")
                .textContent = data.Title;

            document.getElementById("detailCategory")
                .textContent = data.Category;

            document.getElementById("detailDescription")
                .textContent = data.Description;


            /* PRECIOS */

            const priceWrap =
                document.getElementById("detailPrices");

            priceWrap.innerHTML = "";

            data.PriceOptions.forEach(p => {

                const div = document.createElement("div");
                div.className = "price-option";

                div.innerHTML =
                    `<strong>${p.Label}</strong>
             <span>${p.PriceFormatted}</span>`;

                priceWrap.appendChild(div);
            });


            /* REVIEWS */

            const reviews =
                document.getElementById("detailReviewsList");

            reviews.innerHTML = "";

            if (data.Reviews.length) {

                data.Reviews.forEach(r => {

                    reviews.innerHTML += `
            <div class="review-item">
                <b>${r.User}</b>
                ⭐ ${r.Rating}/5
                <p>${r.Comment}</p>
            </div>`;
                });

                document.getElementById("detailRating")
                    .textContent =
                    data.AverageRating +
                    " basado en " +
                    data.Reviews.length +
                    " reseñas";
            }
        }


        /* ============================
           BOTON ATRAS NAVEGADOR
        ============================ */

        window.onpopstate = function () {
            app.classList.remove("app-show-detail");
        };

    })();

</script>

</asp:Content>