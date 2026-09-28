// Çarşı Dekor - sayfa içi fotoğraf galerisi (lightbox)
// Kullanım: bir tetikleyici öğeye data-lightbox="/resim/yolu.jpg" ekle.
// Aynı grup içindeki resimler arasında ok tuşlarıyla / butonlarla gezinilir:
// data-lightbox-group="ayni-deger-olan-butun-ogeler-tek-galeri-sayilir"
// İsteğe bağlı: data-lightbox-title, data-lightbox-alt, data-lightbox-link ("Projeyi incele" bağlantısı)
(function () {
    var overlay, imgEl, titleEl, linkEl, counterEl, prevBtn, nextBtn;
    var items = [];
    var index = 0;

    function build() {
        overlay = document.createElement('div');
        overlay.className = 'cd-lightbox';
        overlay.setAttribute('aria-hidden', 'true');
        overlay.innerHTML =
            '<button type="button" class="cd-lb-close" aria-label="Kapat">&times;</button>' +
            '<button type="button" class="cd-lb-prev" aria-label="Önceki fotoğraf">&#10094;</button>' +
            '<div class="cd-lb-stage">' +
                '<img class="cd-lb-img" alt="" />' +
                '<div class="cd-lb-caption">' +
                    '<span class="cd-lb-title"></span>' +
                    '<a class="cd-lb-link" href="#">Projeyi incele</a>' +
                '</div>' +
                '<span class="cd-lb-counter"></span>' +
            '</div>' +
            '<button type="button" class="cd-lb-next" aria-label="Sonraki fotoğraf">&#10095;</button>';
        document.body.appendChild(overlay);

        imgEl = overlay.querySelector('.cd-lb-img');
        titleEl = overlay.querySelector('.cd-lb-title');
        linkEl = overlay.querySelector('.cd-lb-link');
        counterEl = overlay.querySelector('.cd-lb-counter');
        prevBtn = overlay.querySelector('.cd-lb-prev');
        nextBtn = overlay.querySelector('.cd-lb-next');

        overlay.querySelector('.cd-lb-close').addEventListener('click', close);
        overlay.addEventListener('click', function (e) { if (e.target === overlay) close(); });
        prevBtn.addEventListener('click', function () { show(index - 1); });
        nextBtn.addEventListener('click', function () { show(index + 1); });

        document.addEventListener('keydown', function (e) {
            if (!overlay.classList.contains('is-open')) return;
            if (e.key === 'Escape') close();
            if (e.key === 'ArrowLeft') show(index - 1);
            if (e.key === 'ArrowRight') show(index + 1);
        });
    }

    function show(i) {
        if (!items.length) return;
        index = (i + items.length) % items.length;
        var it = items[index];

        imgEl.src = it.src;
        imgEl.alt = it.alt || '';
        titleEl.textContent = it.title || '';

        if (it.link) {
            linkEl.href = it.link;
            linkEl.style.display = '';
        } else {
            linkEl.style.display = 'none';
        }

        counterEl.textContent = items.length > 1 ? (index + 1) + ' / ' + items.length : '';
        var multi = items.length > 1;
        prevBtn.style.display = multi ? '' : 'none';
        nextBtn.style.display = multi ? '' : 'none';
    }

    function open(list, startIndex) {
        items = list;
        show(startIndex);
        overlay.classList.add('is-open');
        overlay.setAttribute('aria-hidden', 'false');
        document.body.classList.add('cd-lb-lock');
    }

    function close() {
        overlay.classList.remove('is-open');
        overlay.setAttribute('aria-hidden', 'true');
        document.body.classList.remove('cd-lb-lock');
        imgEl.src = '';
    }

    function readGroup(group) {
        var selector = group
            ? '[data-lightbox][data-lightbox-group="' + group.replace(/"/g, '\\"') + '"]'
            : '[data-lightbox]';
        var nodes = document.querySelectorAll(selector);
        return Array.prototype.map.call(nodes, function (n) {
            return {
                node: n,
                src: n.getAttribute('data-lightbox'),
                alt: n.getAttribute('data-lightbox-alt') || '',
                title: n.getAttribute('data-lightbox-title') || '',
                link: n.getAttribute('data-lightbox-link') || ''
            };
        }).filter(function (it) { return !!it.src; });
    }

    document.addEventListener('DOMContentLoaded', function () {
        build();

        document.addEventListener('click', function (e) {
            var trigger = e.target.closest('[data-lightbox]');
            if (!trigger) return;

            var src = trigger.getAttribute('data-lightbox');
            if (!src) return;

            e.preventDefault();

            var group = trigger.getAttribute('data-lightbox-group');
            var list = readGroup(group);
            var startIndex = 0;
            for (var i = 0; i < list.length; i++) {
                if (list[i].node === trigger) { startIndex = i; break; }
            }

            open(list, startIndex);
        });
    });
})();
