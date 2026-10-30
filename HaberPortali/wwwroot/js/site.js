// ===== Site geneli JavaScript =====
$(function () {
    // Haber detayındaki "Bağlantıyı kopyala" düğmesi
    $(document).on('click', '.js-copy-link', function () {
        const $btn = $(this);
        const url = $btn.data('url');
        if (!navigator.clipboard) {
            window.prompt('Bağlantıyı kopyalayın:', url);
            return;
        }
        navigator.clipboard.writeText(url).then(function () {
            const original = $btn.html();
            $btn.html('<i class="bi bi-check2"></i> Kopyalandı');
            setTimeout(function () { $btn.html(original); }, 1800);
        });
    });
});
