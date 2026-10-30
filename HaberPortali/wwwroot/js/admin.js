// ===== Yönetim paneli ortak JavaScript kodları =====
// AJAX istekleri jQuery ile gönderilir. Her POST isteğine anti-forgery anahtarı
// "RequestVerificationToken" başlığıyla eklenir; sunucudaki [ValidateAntiForgeryToken] bunu doğrular.

const Admin = {
    token: function () {
        return $('#antiForgeryForm input[name="__RequestVerificationToken"]').val();
    },

    post: function (url, data) {
        return $.ajax({
            url: url,
            type: 'POST',
            data: data,
            headers: { 'RequestVerificationToken': Admin.token() }
        });
    },

    // Sağ altta kısa bilgi mesajı gösterir.
    toast: function (message, type) {
        type = type || 'success';
        const icon = type === 'success' ? 'bi-check-circle' : 'bi-exclamation-triangle';
        const $toast = $('<div class="toast align-items-center border-0" role="status" aria-live="polite" aria-atomic="true">')
            .addClass('text-bg-' + type)
            .append(
                $('<div class="d-flex">').append(
                    $('<div class="toast-body">').append($('<i class="bi me-2">').addClass(icon)).append(document.createTextNode(message)),
                    $('<button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Kapat">')
                )
            );
        $('#toastContainer').append($toast);
        const toast = new bootstrap.Toast($toast[0], { delay: 3000 });
        $toast.on('hidden.bs.toast', function () { $toast.remove(); });
        toast.show();
    },

    // Kenar çubuğundaki ve sayfadaki "onay bekleyen yorum" sayılarını günceller.
    updatePendingCount: function (count) {
        if (typeof count !== 'number') return;
        $('.js-pending-count').text(count).toggleClass('d-none', count === 0);
        $('.js-pending-number').text(count);
    },

    // Yorum onaylama ve silme düğmeleri (gösterge paneli ve yorumlar sayfası).
    bindCommentActions: function (approveUrl, deleteUrl) {
        $(document).on('click', '.js-approve', function () {
            const $btn = $(this);
            const $row = $btn.closest('tr');
            $btn.prop('disabled', true);

            Admin.post(approveUrl, { id: $btn.data('id') })
                .done(function (res) {
                    if (!res.success) {
                        Admin.toast(res.message, 'danger');
                        $btn.prop('disabled', false);
                        return;
                    }
                    Admin.toast(res.message);
                    Admin.updatePendingCount(res.pendingCount);

                    if (String($btn.data('remove-row')) === 'true') {
                        $row.fadeOut(250, function () { $(this).remove(); });
                        return;
                    }

                    // "Tümü" sekmesinde satır kalır, yalnızca görünümü değişir.
                    $row.find('.js-status')
                        .toggleClass('text-bg-success', res.value)
                        .toggleClass('text-bg-warning', !res.value)
                        .text(res.value ? 'Onaylı' : 'Bekliyor');
                    $btn.toggleClass('btn-success', !res.value)
                        .toggleClass('btn-outline-secondary', res.value)
                        .html(res.value ? '<i class="bi bi-x-lg"></i> Onayı kaldır' : '<i class="bi bi-check-lg"></i> Onayla')
                        .prop('disabled', false);
                })
                .fail(function () {
                    Admin.toast('İşlem sırasında bir hata oluştu.', 'danger');
                    $btn.prop('disabled', false);
                });
        });

        $(document).on('click', '.js-delete-comment', function () {
            if (!confirm('Bu yorum kalıcı olarak silinsin mi?')) return;

            const $btn = $(this);
            $btn.prop('disabled', true);

            Admin.post(deleteUrl, { id: $btn.data('id') })
                .done(function (res) {
                    if (res.success) {
                        Admin.toast(res.message);
                        Admin.updatePendingCount(res.pendingCount);
                        $btn.closest('tr').fadeOut(250, function () { $(this).remove(); });
                    } else {
                        Admin.toast(res.message, 'danger');
                        $btn.prop('disabled', false);
                    }
                })
                .fail(function () {
                    Admin.toast('Yorum silinirken bir hata oluştu.', 'danger');
                    $btn.prop('disabled', false);
                });
        });
    },

    // Dosya seçildiğinde görsel önizlemesini gösterir.
    bindImagePreview: function (inputSelector, imageSelector) {
        $(inputSelector).on('change', function () {
            const file = this.files && this.files[0];
            if (!file) return;
            const reader = new FileReader();
            reader.onload = function (e) { $(imageSelector).attr('src', e.target.result); };
            reader.readAsDataURL(file);
        });
    }
};

$(function () {
    // Listelerdeki açma/kapama anahtarları (haber yayında/manşet/son dakika, kategori, kullanıcı).
    // Örnek: <input class="js-toggle" data-url="/Admin/News/Toggle" data-id="5" data-field="active">
    $(document).on('change', '.js-toggle', function () {
        const $input = $(this);
        const data = { id: $input.data('id') };
        if ($input.data('field')) {
            data.field = $input.data('field');
        }

        $input.prop('disabled', true);

        Admin.post($input.data('url'), data)
            .done(function (res) {
                if (res.success) {
                    $input.prop('checked', res.value);
                    Admin.toast(res.message);
                } else {
                    $input.prop('checked', !$input.prop('checked'));
                    Admin.toast(res.message, 'danger');
                }
            })
            .fail(function () {
                $input.prop('checked', !$input.prop('checked'));
                Admin.toast('İşlem sırasında bir hata oluştu.', 'danger');
            })
            .always(function () {
                $input.prop('disabled', false);
            });
    });

    // Silme formlarında onay sor.
    $(document).on('submit', 'form.js-confirm', function (e) {
        if (!confirm($(this).data('confirm'))) {
            e.preventDefault();
        }
    });

    // Mobilde kenar çubuğunu aç/kapat.
    $('#sidebarToggle').on('click', function () {
        $('body').toggleClass('sidebar-open');
    });
    $('#sidebarBackdrop').on('click', function () {
        $('body').removeClass('sidebar-open');
    });
});
