// Proje detayında küçük fotoğrafa tıklanınca büyük fotoğrafı değiştirir.
document.addEventListener('click', function (e) {
    var thumb = e.target.closest('[data-gallery-thumb]');
    if (!thumb) return;

    var main = document.getElementById('mainImage');
    if (main) main.src = thumb.dataset.src;

    document.querySelectorAll('[data-gallery-thumb]').forEach(function (t) {
        t.classList.remove('active');
    });
    thumb.classList.add('active');
});
