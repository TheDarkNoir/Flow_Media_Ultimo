<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MenuCliente.aspx.cs" Inherits="FlowMediaWeb.ACliente.MenuCliente" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>Menu Cliente</title>
    <link href="~/Estilos/MenuClient.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <header class="mc-header">
            <div class="mc-header-inner">
                <div class="feature-icon"></div>
                <button type="button" class="mc-back">&#8592;</button>
                <button type="button" class="mc-menu">&#9776;</button>
                <div class="mc-search">
                    <input id="TxtBarraSearch" runat="server" type="text" class="mc-search-input" placeholder="Buscar pelicula, videojuego, y más..." />
                    <button type="submit" class="mc-search-btn">🔍</button>
                </div>
                <div class="mc-icons">
                    <button type="button" class="mc-cart">🛒</button>
                    <button type="button" class="mc-user">👤</button>
                </div>
            </div>
        </header>

        <section class="mc-hero">
            <div class="mc-hero-inner">
                <h1>TU ENTRETENIMIENTO, <span>SIEMPRE</span> DISPONIBLE.</h1>
                <p class="mc-hero-sub">La plataforma líder en Colombia para la renta y compra de los mejores videojuegos y películas. Calidad premium, precios justos y entrega digital.</p>
            </div>
        </section>

        <!-- Features first -->
        <section class="mc-features">
            <div class="mc-features-inner">
                <article class="feature">
                    <div class="feature-icon">⏱</div>
                    <h3>Rentas Flexibles</h3>
                    <p>Disfruta de tus títulos favoritos por 7 días a precios increíbles. Renueva fácilmente desde tu perfil.</p>
                </article>
                <article class="feature">
                    <div class="feature-icon">🔒</div>
                    <h3>Compra Segura</h3>
                    <p>Adquiere tus juegos y películas de forma definitiva con total garantía y soporte técnico especializado.</p>
                </article>
                <article class="feature">
                    <div class="feature-icon">🎮</div>
                    <h3>Catálogo Premium</h3>
                    <p>Desde clásicos de PS2 hasta los últimos estrenos de PS4 y ediciones especiales de colección.</p>
                </article>
            </div>
        </section>

        <!-- Categories placed below features -->
        <section class="mc-categories">
            <div class="mc-categories-header">
                <h2>Categorías Destacadas</h2>
                <a class="mc-viewall" href="#">Ver todo</a>
            </div>
            <div class="mc-cards">
                <a class="mc-card videogames" href="#"><div class="mc-card-img">&nbsp;</div><div class="mc-card-title">Videojuegos</div></a>
                <a class="mc-card peliculas" href="#"><div class="mc-card-img">&nbsp;</div><div class="mc-card-title">Películas</div></a>
                <a class="mc-card ps4" href="#"><div class="mc-card-img">&nbsp;</div><div class="mc-card-title">PS4 Premium</div></a>
                <a class="mc-card estrenos" href="#"><div class="mc-card-img">&nbsp;</div><div class="mc-card-title">Estrenos</div></a>
            </div>
        </section>

        <footer class="mc-footer">
            <div class="mc-footer-inner">
                <div class="mc-footer-left">
                    <div class="logo-mark small"></div>
                    <div class="copyright">© <%: DateTime.Now.Year %> FLOW MEDIA. TODOS LOS DERECHOS RESERVADOS.<br />Tu entretenimiento, siempre disponible.<br /><span class="meta">contacto@flowmedia.com | Colombia</span></div>
                </div>
                <div class="mc-footer-right">
                    <asp:Button ID="bTNcONTAC" runat="server" Text="¡CONTACTANOS!" CssClass="contact-btn" />
                    <div class="footer-icons">
                        <span class="fi">👥</span>
                        <span class="fi">◻</span>
                        <span class="fi">◻</span>
                    </div>
                </div>
            </div>
        </footer>

        <button class="mc-chat">💬</button>
    </form>
</body>
</html>
