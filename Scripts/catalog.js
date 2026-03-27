// Comportamiento central del catálogo: delegación de eventos y manejo de filtros
(function(){
    function onCardClick(e){
        var card = e.target.closest('.catalog-card');
        if(!card) return;
        var id = card.dataset.id;
        // abrir detalle del ítem o modal
        alert('Abrir detalle item ' + id);
    }

    function extractCatalogHtml(html) {
        try {
            var parser = new DOMParser();
            var doc = parser.parseFromString(html, 'text/html');
            var grid = doc.getElementById('catalogGrid');
            if (grid) return grid.innerHTML;
            // opción alternativa: si la partial devuelve directamente la lista interior
            return html;
        } catch (e) {
            return html;
        }
    }

    function onFilterClick(e){
        var btn = e.target.closest('.filter-btn');
        if(!btn) return;
        var filtro = btn.dataset.filter;
        var url = (window.catalogIndexUrl || '/Catalog') + '?filter=' + encodeURIComponent(filtro);
        var target = document.getElementById('catalogGrid') || document.getElementById('mainContent');
        if(!target) return;
        target.innerHTML = '<div class="compras-empty">Cargando catálogo...</div>';
        fetch(url, { credentials: 'same-origin' })
            .then(function(r){ if(!r.ok) throw new Error(r.statusText); return r.text(); })
            .then(function(html){
                var content = extractCatalogHtml(html);
                target.innerHTML = content;
            })
            .catch(function(err){ target.innerHTML = '<div class="compras-empty">Error al cargar catálogo.</div>'; console.error(err); });
    }

    document.addEventListener('click', function(e){
        onCardClick(e);
        onFilterClick(e);
    });

})();
