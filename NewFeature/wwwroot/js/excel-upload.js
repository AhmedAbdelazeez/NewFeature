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

    // Must match MaxUploadBytes in Program.cs.
    var UPLOAD_LIMIT_MB = 200;

    function formatMb(bytes) {
        return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
    }

    // Shown for as long as the server is reading the file. A month of Operations lines takes the
    // server the better part of a minute to read; with nothing on screen that looked exactly like
    // "the upload does nothing".
    function showProcessing(file) {
        var modal = ensureModal();
        document.getElementById(MODAL_ID + 'Title').textContent = 'جاري رفع الملف';
        document.getElementById(MODAL_ID + 'Body').innerHTML =
            '<div class="text-center py-4">' +
            '  <div class="spinner-border text-info mb-3" role="status"></div>' +
            '  <div class="fw-bold mb-1">' + escapeHtml(file.name) + ' (' + formatMb(file.size) + ')</div>' +
            '  <div class="text-muted small">جاري رفع الملف والتحقق من كل صف. الملفات الكبيرة قد تستغرق دقيقة أو أكثر، يرجى عدم إغلاق الصفحة.</div>' +
            '  <div class="text-muted small mt-2" id="' + MODAL_ID + 'Elapsed"></div>' +
            '</div>';

        var started = Date.now();
        var timer = setInterval(function () {
            var el = document.getElementById(MODAL_ID + 'Elapsed');
            if (el) el.textContent = 'الوقت المنقضي: ' + Math.round((Date.now() - started) / 1000) + ' ثانية';
        }, 1000);

        if (window.bootstrap && window.bootstrap.Modal) {
            window.bootstrap.Modal.getOrCreateInstance(modal).show();
        }
        return function () { clearInterval(timer); };
    }

    // ASP.NET's own validation failures (e.g. the request-form reader refusing the file) come back
    // as ProblemDetails, whose "errors" is an object of arrays rather than our row-error list.
    function problemDetailsMessage(payload) {
        var parts = [];
        if (payload.errors && !Array.isArray(payload.errors)) {
            Object.keys(payload.errors).forEach(function (key) {
                [].concat(payload.errors[key]).forEach(function (m) { parts.push(m); });
            });
        }
        if (parts.length === 0 && payload.detail) parts.push(payload.detail);
        if (parts.length === 0 && payload.title) parts.push(payload.title);
        return parts.join(' ');
    }

    // Turns any response - the import result, a ProblemDetails, a plain-text error, an HTML error
    // or login page - into something the reporter can show. Nothing is ever swallowed.
    async function readResponse(res, file) {
        if (res.status === 413) {
            return { ok: false, result: { message: 'حجم الملف (' + formatMb(file.size) + ') أكبر من الحد المسموح به للرفع (' + UPLOAD_LIMIT_MB + ' MB). احذف الصفوف الفارغة أسفل البيانات في الإكسل أو قسّم الملف ثم أعد الرفع.' } };
        }
        if (res.status === 401 || res.status === 403 || (res.redirected && /\/Account\/(Login|AccessDenied)/i.test(res.url))) {
            return { ok: false, result: { message: 'ليس لديك صلاحية رفع هذا الملف، أو انتهت جلسة الدخول. سجّل الدخول مرة أخرى أو تواصل مع مسؤول النظام لمنحك الصلاحية.' } };
        }

        var contentType = res.headers.get('content-type') || '';
        if (contentType.indexOf('json') >= 0) {
            var payload = await res.json();
            if (Array.isArray(payload.errors)) {
                // The import result. A rejected template reports success: false even if a caller
                // forgot to map that to an error status.
                return { ok: res.ok && payload.success !== false, result: payload };
            }
            var message = payload.message || problemDetailsMessage(payload);
            return { ok: false, result: { message: message || ('تعذّر رفع الملف (رمز الخطأ ' + res.status + ').') } };
        }

        var text = (await res.text()).trim();
        var looksLikeHtml = /^<!doctype|^<html/i.test(text);
        if (!res.ok || looksLikeHtml) {
            var fallback = res.status >= 500
                ? 'حدث خطأ في الخادم أثناء معالجة الملف (رمز الخطأ ' + res.status + '). يرجى إعادة المحاولة أو التواصل مع الدعم الفني.'
                : 'تعذّر رفع الملف (رمز الخطأ ' + res.status + ').';
            return { ok: false, result: { message: (!looksLikeHtml && text) ? text : fallback } };
        }
        return { ok: true, result: { message: text || 'تمت معالجة الملف.' } };
    }

    // onDone runs whether the upload succeeded or failed, so a page always refreshes its tables and
    // KPI tiles - a partially accepted file still changed the data behind them.
    window.rawahelExcelUpload = async function (event, url, onDone) {
        var input = event.target;
        var file = input.files && input.files[0];
        if (!file) return;

        if (file.size > UPLOAD_LIMIT_MB * 1024 * 1024) {
            input.value = '';
            render({ message: 'حجم الملف (' + formatMb(file.size) + ') أكبر من الحد المسموح به للرفع (' + UPLOAD_LIMIT_MB + ' MB). احذف الصفوف الفارغة أسفل البيانات في الإكسل أو قسّم الملف ثم أعد الرفع.' }, false);
            return;
        }

        var formData = new FormData();
        formData.append('file', file);

        var stopProcessing = showProcessing(file);
        try {
            var res = await fetch(url, { method: 'POST', body: formData });
            var outcome = await readResponse(res, file);
            stopProcessing();
            render(outcome.result, outcome.ok);
        } catch (err) {
            stopProcessing();
            console.error('[Excel upload]', err);
            render({ message: 'تعذّر الاتصال بالخادم أثناء رفع الملف. تأكد من أن الخادم يعمل ثم أعد المحاولة.' }, false);
        } finally {
            // Always clear the input so re-picking the same file fires change again.
            input.value = '';
            if (typeof onDone === 'function') onDone();
        }
    };
})();
