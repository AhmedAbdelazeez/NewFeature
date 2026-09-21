/*
 * Shared Excel bulk-upload reporter — إدارة رفع ملفات الإكسل
 * ---------------------------------------------------------
 * Every department that uploads its approved template gets the same treatment here: the shared
 * ExcelImportResultDto the API returns (inserted / updated / skipped plus one message per rejected
 * row, naming the row number and the offending column) is rendered into a readable panel instead
 * of an alert() that truncates after a handful of lines and can't be scrolled or copied.
 *
 * Usage:  <input type="file" onchange="rawahelExcelUpload(event, '/api/finance/bulk-upload', reload)">
 */
(function () {
    'use strict';

    var MODAL_ID = 'rawahelUploadResultModal';

    function ensureModal() {
        var existing = document.getElementById(MODAL_ID);
        if (existing) return existing;

        var wrapper = document.createElement('div');
        wrapper.innerHTML =
            '<div class="modal fade" id="' + MODAL_ID + '" tabindex="-1" aria-hidden="true">' +
            '  <div class="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable">' +
            '    <div class="modal-content modal-content-glass">' +
            '      <div class="modal-header modal-header-glass">' +
            '        <h5 class="modal-title fw-bold" id="' + MODAL_ID + 'Title">نتيجة رفع الملف</h5>' +
            '        <button type="button" class="close-btn-glass" data-bs-dismiss="modal" aria-label="إغلاق">&times;</button>' +
            '      </div>' +
            '      <div class="modal-body" id="' + MODAL_ID + 'Body"></div>' +
            '      <div class="modal-footer">' +
            '        <button type="button" class="btn btn-neon-outline" data-bs-dismiss="modal">إغلاق</button>' +
            '      </div>' +
            '    </div>' +
            '  </div>' +
            '</div>';

        var modal = wrapper.firstElementChild;
        document.body.appendChild(modal);
        return modal;
    }

    function escapeHtml(value) {
        return String(value == null ? '' : value)
            .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;').replace(/'/g, '&#39;');
    }

    function statTile(label, value, tone) {
        return '<div class="col-6 col-md-3">' +
            '  <div class="glass-card p-3 text-center">' +
            '    <div class="text-muted small">' + escapeHtml(label) + '</div>' +
            '    <div class="fs-4 fw-bold ' + tone + '">' + escapeHtml(value) + '</div>' +
            '  </div>' +
            '</div>';
    }

    function render(result, ok) {
        var modal = ensureModal();
        var title = document.getElementById(MODAL_ID + 'Title');
        var body = document.getElementById(MODAL_ID + 'Body');

        title.textContent = ok ? 'نتيجة رفع الملف' : 'تعذّر رفع الملف';

        var html = '';
        html += '<div class="alert ' + (ok ? 'alert-success' : 'alert-danger') + ' mb-3">' +
                escapeHtml(result.message || (ok ? 'تمت معالجة الملف.' : 'تعذّرت معالجة الملف.')) +
                '</div>';

        // A rejected template never reports row statistics - the file was never read row by row.
        if (ok) {
            html += '<div class="row g-2 mb-3">' +
                statTile('صفوف مقروءة', result.dataRows ?? 0, 'text-white') +
                statTile('صفوف مضافة', result.insertedRows ?? 0, 'text-success') +
                statTile('صفوف محدّثة', result.updatedRows ?? 0, 'text-info') +
                statTile('صفوف متجاهلة', result.skippedRows ?? 0, (result.skippedRows > 0 ? 'text-warning' : 'text-muted')) +
                '</div>';
        }

        var errors = result.errors || [];
        if (errors.length > 0) {
            html += '<h6 class="fw-bold mb-2">تفاصيل الصفوف المتجاهلة (' + errors.length + ')</h6>';
            html += '<div class="table-responsive"><table class="table table-glass table-sm align-middle">' +
                    '<thead><tr><th style="width:90px">الصف</th><th style="width:200px">العمود</th><th>السبب</th></tr></thead><tbody>';
            errors.forEach(function (e) {
                html += '<tr>' +
                    '<td class="fw-bold">' + escapeHtml(e.rowNumber) + '</td>' +
                    '<td>' + escapeHtml(e.column || '—') + '</td>' +
                    '<td>' + escapeHtml(e.message) + '</td>' +
                    '</tr>';
            });
            html += '</tbody></table></div>';
        }

        body.innerHTML = html;

        if (window.bootstrap && window.bootstrap.Modal) {
            window.bootstrap.Modal.getOrCreateInstance(modal).show();
        } else {
            // Bootstrap missing (or still loading) - never swallow the result silently.
            alert(result.message || '');
        }
    }

    // onDone runs whether the upload succeeded or failed, so a page always refreshes its tables and
    // KPI tiles - a partially accepted file still changed the data behind them.
    window.rawahelExcelUpload = async function (event, url, onDone) {
        var input = event.target;
        var file = input.files && input.files[0];
        if (!file) return;

        var formData = new FormData();
        formData.append('file', file);

        try {
            var res = await fetch(url, { method: 'POST', body: formData });
            var contentType = res.headers.get('content-type') || '';
            var payload = contentType.indexOf('application/json') >= 0 ? await res.json() : await res.text();

            if (typeof payload === 'string') {
                render({ message: payload || 'تعذّر رفع الملف.', errors: [] }, false);
            } else {
                render(payload, res.ok);
            }
        } catch (err) {
            console.error('[Excel upload]', err);
            render({ message: 'تعذّر الاتصال بالخادم أثناء رفع الملف. يرجى إعادة المحاولة.', errors: [] }, false);
        } finally {
            // Always clear the input so re-picking the same file fires change again.
            input.value = '';
            if (typeof onDone === 'function') onDone();
        }
    };
})();
