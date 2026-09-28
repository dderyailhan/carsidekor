// Çarşı Dekor - Kategoriler sayfası: sol menü (2 seviye) <-> sağ panel (her seviye için bir panel)
(function () {
    // Bir paneli açar; sol menüde onun temsilcisi olan satırı (en fazla 2. seviye) işaretler.
    function go(panelId) {
        var panel = document.getElementById(panelId);
        if (!panel) return;

        var navId = panel.getAttribute('data-nav') || panelId;
        var btn = document.querySelector('.proj-cat-btn[data-target="' + navId + '"]');

        document.querySelectorAll('[data-cat-panel]').forEach(function (p) {
            p.classList.toggle('is-active', p === panel);
        });
        document.querySelectorAll('.proj-cat-btn').forEach(function (b) {
            b.classList.toggle('is-active', b === btn);
        });

        // Seçilen satırın kendisi ve üstündeki gruplar açık, diğerleri kapalı
        var keep = [];
        var node = btn ? btn.closest('.proj-cat') : null;
        for (var n = node; n; n = n.parentElement ? n.parentElement.closest('.proj-cat') : null) {
            keep.push(n);
        }
        document.querySelectorAll('.proj-cat').forEach(function (c) {
            c.classList.toggle('is-open', keep.indexOf(c) !== -1);
        });
    }

    function scrollToContent() {
        var content = document.querySelector('.proj-content');
        if (content && content.getBoundingClientRect().top < 0) {
            content.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
    }

    document.addEventListener('DOMContentLoaded', function () {
        document.addEventListener('click', function (e) {
            // Sol menü satırı
            var btn = e.target.closest('.proj-cat-btn');
            if (btn) {
                var target = btn.getAttribute('data-target');
                var node = btn.closest('.proj-cat');
                var hasKids = !!node.querySelector(':scope > .proj-subcats');
                var panelShown = document.getElementById(target).classList.contains('is-active');

                // Zaten açık olan üst kategoriye tekrar basınca alt listeyi aç/kapa
                if (btn.classList.contains('is-active') && panelShown && hasKids) {
                    node.classList.toggle('is-open');
                    return;
                }
                go(target);
                return;
            }

            // Sağdaki alt kategori kartı veya yol çubuğundaki bağlantı
            var link = e.target.closest('[data-goto]');
            if (link) {
                go(link.getAttribute('data-goto'));
                scrollToContent();
            }
        });

        // Anasayfadaki kart "/Projects?kategori=slug" ile geldiyse o kategoriyi aç
        var slug = new URLSearchParams(window.location.search).get('kategori');
        if (slug) {
            var match = Array.prototype.find.call(
                document.querySelectorAll('[data-cat-panel]'),
                function (p) { return p.getAttribute('data-slug') === slug; });
            if (match) go(match.id);
        }
    });
})();
