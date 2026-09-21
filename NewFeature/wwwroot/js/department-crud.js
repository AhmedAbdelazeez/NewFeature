/*
 * Department records index + CRUD — جدول السجلات وإضافة/تعديل/حذف
 * -------------------------------------------------------------------
 * One configurable component for every department page (Operations, Maintenance, Finance): a
 * searchable, paged index table plus an add/edit modal and delete, all driven by a column/field
 * config. The server applies the same rules to a hand-entered record as to an uploaded Excel row,
 * and rejects with { message, errors: [{ field, message }] }; each error is shown under its field.
 *
 *   RawahelCrud.mount({
 *     el: '#crud-dispatch', title: 'سجل أوامر التشغيل',
 *     listUrl: '/api/operations/records', itemUrl: id => '/api/operations/records/' + id,
 *     createUrl: '/api/operations/records',
 *     dateFilter: true, searchPlaceholder: '...',
 *     columns: [{ key, label, format? }],
 *     fields:  [{ key, label, type: 'text'|'number'|'date'|'datetime'|'select'|'textarea',
 *                 required?, options?: [{ value, label }], col?: 4|6|12 }]
 *   });
 */
(function () {
    'use strict';

    const instances = [];

    // One-line cells: long values (customer names, fault descriptions) are cut with an ellipsis and
    // shown in full on hover, and the table scrolls sideways inside its card rather than turning
    // every row into a five-line block on a narrow screen.
    (function injectStyles() {
        if (document.getElementById('rawahel-crud-styles')) return;
        const style = document.createElement('style');
        style.id = 'rawahel-crud-styles';
        style.textContent =
            '.rawahel-crud td, .rawahel-crud th { white-space: nowrap; font-size: .85rem; vertical-align: middle; }' +
            '.rawahel-crud .crud-cell { display: inline-block; max-width: 240px; overflow: hidden; text-overflow: ellipsis; vertical-align: bottom; }';
        document.head.appendChild(style);
    })();

    function esc(value) {
        return String(value == null ? '' : value)
            .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;').replace(/'/g, '&#39;');
    }

    function pad(n) { return String(n).padStart(2, '0'); }

    // Dates travel as ISO strings; <input type=date|datetime-local> wants local wall-clock text.
    function toInputValue(type, value) {
        if (value === null || value === undefined || value === '') return '';
        if (type === 'date' || type === 'datetime') {
            const d = new Date(value);
            if (isNaN(d)) return '';
            const day = `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
            return type === 'date' ? day : `${day}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
        }
        return String(value);
    }

    // Blank inputs come back as null, which the submit handler turns into "field not sent".
    function fromInputValue(type, raw) {
        if (raw === '' || raw === null || raw === undefined) return null;
        if (type === 'number') return Number(raw);
        if (type === 'select-int') return parseInt(raw, 10);
        return raw;
    }

    function fmtDate(value) {
        if (!value) return '--';
        const d = new Date(value);
        return isNaN(d) ? '--' : d.toLocaleDateString('en-GB');
    }

    function fmtDateTime(value) {
        if (!value) return '--';
        const d = new Date(value);
        return isNaN(d) ? '--' : `${d.toLocaleDateString('en-GB')} ${pad(d.getHours())}:${pad(d.getMinutes())}`;
    }

    function fmtNumber(value, digits) {
        if (value === null || value === undefined || value === '') return '--';
        return Number(value).toLocaleString('en-US', { maximumFractionDigits: digits == null ? 2 : digits });
    }

    function mount(cfg) {
        const root = document.querySelector(cfg.el);
        if (!root) return null;

        const uid = 'crud' + (instances.length + 1);
        const state = { page: 1, pageSize: cfg.pageSize || 20, editingId: null, totalPages: 1 };

        root.innerHTML = `
            <div class="glass-card p-3 mb-3">
                <div class="row g-2 align-items-center">
                    <div class="${cfg.dateFilter ? 'col-md-5' : 'col-md-9'}">
                        <div class="input-group">
                            <span class="input-group-text bg-transparent border-glass text-muted"><i class="bi bi-search"></i></span>
                            <input type="text" id="${uid}-search" class="form-control form-control-glass" placeholder="${esc(cfg.searchPlaceholder || 'بحث...')}">
                        </div>
                    </div>
                    ${cfg.dateFilter ? `
                    <div class="col-md-2"><input type="date" id="${uid}-from" class="form-control form-control-glass" title="من تاريخ"></div>
                    <div class="col-md-2"><input type="date" id="${uid}-to" class="form-control form-control-glass" title="إلى تاريخ"></div>` : ''}
                    <div class="col-md-3 d-flex gap-2">
                        <button class="btn btn-neon-outline btn-sm flex-grow-1" id="${uid}-apply"><i class="bi bi-funnel"></i> بحث</button>
                        <button class="btn btn-neon btn-sm flex-grow-1" id="${uid}-add"><i class="bi bi-plus-circle"></i> إضافة</button>
                    </div>
                </div>
            </div>

            <div class="glass-card">
                <div class="table-responsive">
                    <table class="table table-glass align-middle mb-0 rawahel-crud">
                        <thead><tr>
                            ${cfg.columns.map(c => `<th>${esc(c.label)}</th>`).join('')}
                            <th class="text-end" style="width:110px">الإجراءات</th>
                        </tr></thead>
                        <tbody id="${uid}-body"></tbody>
                    </table>
                </div>
                <div class="d-flex justify-content-between align-items-center p-3 flex-wrap gap-2">
                    <span class="text-muted small" id="${uid}-info">--</span>
                    <div class="d-flex gap-2">
                        <button class="btn btn-neon-outline btn-sm" id="${uid}-prev">السابق</button>
                        <button class="btn btn-neon-outline btn-sm" id="${uid}-next">التالي</button>
                    </div>
                </div>
            </div>

            <div class="modal fade" id="${uid}-modal" tabindex="-1" aria-hidden="true">
                <div class="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable">
                    <div class="modal-content modal-content-glass">
                        <div class="modal-header modal-header-glass">
                            <h5 class="modal-title fw-bold" id="${uid}-modal-title">إضافة</h5>
                            <button type="button" class="close-btn-glass" data-bs-dismiss="modal" aria-label="إغلاق">&times;</button>
                        </div>
                        <form id="${uid}-form" novalidate>
                            <div class="modal-body">
                                <div class="alert alert-danger d-none" id="${uid}-form-error"></div>
                                <div class="row g-3">
                                    ${cfg.fields.map(f => fieldHtml(uid, f)).join('')}
                                </div>
                                <div class="text-muted small mt-3">* حقل إلزامي</div>
                            </div>
                            <div class="modal-footer">
                                <button type="button" class="btn btn-neon-outline" data-bs-dismiss="modal">إلغاء</button>
                                <button type="submit" class="btn btn-neon" id="${uid}-save">حفظ</button>
                            </div>
                        </form>
                    </div>
                </div>
            </div>`;

        const $ = id => document.getElementById(`${uid}-${id}`);
        const modalEl = $('modal');

        async function load() {
            const body = $('body');
            body.innerHTML = `<tr><td colspan="${cfg.columns.length + 1}" class="text-center py-4 text-muted"><i class="bi bi-arrow-repeat spin me-2"></i> جاري التحميل...</td></tr>`;

            const params = new URLSearchParams({ page: state.page, pageSize: state.pageSize });
            const search = $('search').value.trim();
            if (search) params.set('search', search);
            if (cfg.dateFilter) {
                if ($('from').value) params.set('fromDate', $('from').value);
                if ($('to').value) params.set('toDate', $('to').value);
            }

            try {
                const res = await fetch(`${cfg.listUrl}?${params}`);
                if (!res.ok) throw new Error(res.statusText);
                const data = await res.json();
                const items = data.items || [];
                state.totalPages = Math.max(1, Math.ceil((data.totalCount || 0) / (data.pageSize || state.pageSize)));

                if (items.length === 0) {
                    body.innerHTML = `<tr><td colspan="${cfg.columns.length + 1}" class="text-center py-4 text-muted">لا توجد سجلات. ارفع النموذج أو أضف سجلاً جديداً.</td></tr>`;
                } else {
                    body.innerHTML = items.map(item => `
                        <tr>
                            ${cfg.columns.map(c => { const html = c.format ? c.format(item[c.key], item) : esc(item[c.key] ?? '--'); const tip = String(html).replace(/<[^>]*>/g, ''); return `<td><span class="crud-cell" title="${tip}">${html}</span></td>`; }).join('')}
                            <td class="text-end text-nowrap">
                                <button class="btn btn-neon-outline btn-sm me-1" data-edit="${item.id}" title="تعديل"><i class="bi bi-pencil-square"></i></button>
                                <button class="btn btn-neon-danger btn-sm" data-delete="${item.id}" title="حذف"><i class="bi bi-trash"></i></button>
                            </td>
                        </tr>`).join('');
                }

                $('info').innerText = `صفحة ${data.page} من ${state.totalPages} (${(data.totalCount || 0).toLocaleString('en-US')} سجل)`;
                $('prev').disabled = state.page <= 1;
                $('next').disabled = state.page >= state.totalPages;
            } catch (err) {
                console.error('[RawahelCrud] load failed', err);
                body.innerHTML = `<tr><td colspan="${cfg.columns.length + 1}" class="text-center py-4 text-danger">تعذّر تحميل السجلات.</td></tr>`;
            }
        }

        function clearErrors() {
            $('form-error').classList.add('d-none');
            root.querySelectorAll('.is-invalid').forEach(el => el.classList.remove('is-invalid'));
            root.querySelectorAll('.invalid-feedback').forEach(el => { el.textContent = ''; });
        }

        function showErrors(payload) {
            const errors = (payload && payload.errors) || [];
            let unplaced = [];
            errors.forEach(e => {
                const input = $(`f-${e.field}`);
                const feedback = $(`fe-${e.field}`);
                if (input && feedback) {
                    input.classList.add('is-invalid');
                    feedback.textContent = e.message;
                } else {
                    unplaced.push(e.message);
                }
            });
            const summary = unplaced.length ? unplaced.join(' ') : (payload && payload.message) || 'تعذّر حفظ السجل.';
            $('form-error').textContent = summary;
            $('form-error').classList.remove('d-none');
        }

        function openForm(item) {
            clearErrors();
            state.editingId = item ? item.id : null;
            $('modal-title').innerText = item ? `تعديل ${cfg.itemName || 'السجل'}` : `إضافة ${cfg.itemName || 'سجل'}`;
            cfg.fields.forEach(f => {
                const input = $(`f-${f.key}`);
                const value = item ? item[f.key] : (f.default !== undefined ? f.default : '');
                input.value = toInputValue(f.type, value);
            });
            bootstrap.Modal.getOrCreateInstance(modalEl).show();
        }

        async function edit(id) {
            try {
                const res = await fetch(cfg.itemUrl(id));
                if (!res.ok) throw new Error(res.statusText);
                openForm(await res.json());
            } catch (err) {
                console.error('[RawahelCrud] edit load failed', err);
                alert('تعذّر تحميل السجل.');
            }
        }

        async function remove(id) {
            if (!confirm('هل أنت متأكد من حذف هذا السجل؟ لا يمكن التراجع عن الحذف.')) return;
            try {
                const res = await fetch(cfg.itemUrl(id), { method: 'DELETE' });
                if (res.ok || res.status === 204) { load(); return; }
                const payload = (res.headers.get('content-type') || '').includes('json') ? await res.json() : null;
                alert((payload && payload.message) || 'تعذّر حذف السجل.');
            } catch (err) {
                console.error('[RawahelCrud] delete failed', err);
                alert('تعذّر حذف السجل.');
            }
        }

        $('form').addEventListener('submit', async e => {
            e.preventDefault();
            clearErrors();

            // A blank input is left out of the body entirely: the server then sees its default and
            // answers with the same field message the Excel import gives ("... مطلوب"), instead of
            // failing JSON binding on e.g. an empty date for a required DateTime.
            const dto = {};
            cfg.fields.forEach(f => {
                const value = fromInputValue(f.type, $(`f-${f.key}`).value.trim());
                if (value !== null) dto[f.key] = value;
            });
            if (state.editingId) dto.id = state.editingId;

            const saveBtn = $('save');
            saveBtn.disabled = true;
            try {
                const res = await fetch(state.editingId ? cfg.itemUrl(state.editingId) : cfg.createUrl, {
                    method: state.editingId ? 'PUT' : 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(dto)
                });
                if (res.ok) {
                    bootstrap.Modal.getOrCreateInstance(modalEl).hide();
                    load();
                    return;
                }
                const payload = (res.headers.get('content-type') || '').includes('json') ? await res.json() : null;
                // ASP.NET model-binding failures (e.g. a malformed date) arrive as ProblemDetails.
                if (payload && payload.errors && !Array.isArray(payload.errors)) {
                    showErrors({ message: 'يرجى التحقق من صيغة الحقول المدخلة.', errors: [] });
                } else {
                    showErrors(payload);
                }
            } catch (err) {
                console.error('[RawahelCrud] save failed', err);
                showErrors({ message: 'تعذّر الاتصال بالخادم. يرجى إعادة المحاولة.' });
            } finally {
                saveBtn.disabled = false;
            }
        });

        root.addEventListener('click', e => {
            const editBtn = e.target.closest('[data-edit]');
            if (editBtn) { edit(editBtn.dataset.edit); return; }
            const delBtn = e.target.closest('[data-delete]');
            if (delBtn) remove(delBtn.dataset.delete);
        });
        $('apply').addEventListener('click', () => { state.page = 1; load(); });
        $('search').addEventListener('keydown', e => { if (e.key === 'Enter') { state.page = 1; load(); } });
        $('add').addEventListener('click', () => openForm(null));
        $('prev').addEventListener('click', () => { if (state.page > 1) { state.page--; load(); } });
        $('next').addEventListener('click', () => { if (state.page < state.totalPages) { state.page++; load(); } });

        const instance = { reload: () => { state.page = 1; return load(); } };
        instances.push(instance);
        load();
        return instance;
    }

    function fieldHtml(uid, f) {
        const id = `${uid}-f-${f.key}`;
        const label = `<label class="form-label small" for="${id}">${esc(f.label)}${f.required ? ' <span class="text-danger">*</span>' : ''}</label>`;
        let input;
        if (f.type === 'select' || f.type === 'select-int') {
            input = `<select id="${id}" class="form-select form-control-glass">
                        <option value="">${f.required ? '-- اختر --' : '--'}</option>
                        ${(f.options || []).map(o => `<option value="${esc(o.value)}">${esc(o.label)}</option>`).join('')}
                     </select>`;
        } else if (f.type === 'textarea') {
            input = `<textarea id="${id}" class="form-control form-control-glass" rows="2"></textarea>`;
        } else {
            const type = f.type === 'datetime' ? 'datetime-local' : (f.type || 'text');
            input = `<input type="${type}" id="${id}" class="form-control form-control-glass"${f.type === 'number' ? ' step="any"' : ''}>`;
        }
        return `<div class="col-md-${f.col || 6}">${label}${input}<div class="invalid-feedback" id="${uid}-fe-${f.key}"></div></div>`;
    }

    window.RawahelCrud = {
        mount,
        // Called after an Excel upload so every table on the page shows what the file changed.
        reloadAll: () => instances.forEach(i => i.reload()),
        format: { date: fmtDate, dateTime: fmtDateTime, number: fmtNumber, esc }
    };
})();
