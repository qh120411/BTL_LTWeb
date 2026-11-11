const { test } = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const script = fs.readFileSync(path.join(__dirname, '../../wwwroot/js/admin-report.js'), 'utf8');
const tick = () => new Promise(resolve => setImmediate(resolve));

function element() {
    const classes = new Set();
    return {
        style: {}, attrs: {}, hidden: false, value: '',
        classList: { toggle: (name, value) => value ? classes.add(name) : classes.delete(name), contains: name => classes.has(name) },
        setAttribute(name, value) { this.attrs[name] = value; },
        getBoundingClientRect: () => ({ height: 800 })
    };
}

function setup({ reducedMotion = false } = {}) {
    let address = new URL('http://localhost/Admin/Report?startDate=2026-11-01&endDate=2026-11-30');
    const listeners = {}, requests = [], parsed = new Map(), assigned = [], scrolls = [];
    const exportLink = element(), searchFilter = element();
    let root;
    function region(page = 1, search = '') {
        const item = element();
        const query = new URLSearchParams({ startDate: '2026-11-01', endDate: '2026-11-30', search, page });
        item.dataset = { currentUrl: '/Admin/Report?' + query, exportUrl: '/Admin/Report/ExportExcel?search=' + encodeURIComponent(search) };
        item.table = element();
        item.wrap = Object.assign(element(), { scrollLeft: 33 });
        item.results = element();
        item.search = Object.assign(element(), { value: search });
        item.heading = Object.assign(element(), { focus: options => item.focused = options });
        item.feedbackText = element();
        item.retry = Object.assign(element(), { owner: item, kind: 'retry' });
        item.feedback = Object.assign(element(), {
            querySelector: selector => selector === '[data-report-feedback]' ? item.feedbackText : item.retry
        });
        item.querySelector = selector => ({
            '#reportInventory': item.table, '.report-table-wrap': item.wrap, '[data-report-results]': item.results,
            '[name="search"]': item.search, '#reportBooksTitle': item.heading, '.report-feedback': item.feedback
        })[selector];
        item.contains = node => node.owner === item;
        item.replaceWith = next => root = next;
        item.scrollIntoView = options => scrolls.push(options);
        return item;
    }
    root = region();
    const document = {
        querySelector: selector => selector === '[data-report-inventory]' ? root : selector === '.report-export' ? exportLink : searchFilter,
        addEventListener: (name, callback) => listeners['document:' + name] = callback
    };
    const entries = [{ url: address.href, state: null }];
    let index = 0;
    const history = {
        get state() { return entries[index].state; },
        replaceState(state, _, url) { entries[index] = { state, url }; address = new URL(url); },
        pushState(state, _, url) { entries.splice(index + 1); entries.push({ state, url }); index++; address = new URL(url); }
    };
    const location = {
        get href() { return address.href; }, get origin() { return address.origin; }, get pathname() { return address.pathname; },
        assign: url => assigned.push(url)
    };
    const window = {
        location, history, AbortController, scrollY: 700,
        fetch: (url, options) => new Promise((resolve, reject) => requests.push({ url, options, resolve, reject })),
        matchMedia: () => ({ matches: reducedMotion }), scrollTo: options => scrolls.push(options),
        addEventListener: (name, callback) => listeners['window:' + name] = callback
    };
    const context = {
        document, window, URL, FormData: class { constructor(form) { return Object.entries(form.values); } },
        DOMParser: class { parseFromString(text) { return { querySelector: () => parsed.get(text) }; } }
    };
    vm.runInNewContext(script, context);
    const event = (target, extras = {}) => ({ target, button: 0, prevented: false, preventDefault() { this.prevented = true; }, ...extras });
    function click(page, extras = {}) {
        const url = new URL(root.dataset.currentUrl, address);
        url.searchParams.set('page', page); url.hash = 'reportBooks';
        const link = {
            owner: root, href: url.href, hasAttribute: () => false,
            closest: selector => selector === '[data-report-retry]' ? null : link
        };
        const e = event(link, extras); listeners['document:click'](e); return e;
    }
    return {
        requests, entries, assigned, scrolls, exportLink, searchFilter, click, region,
        get root() { return root; }, get url() { return address; },
        submit(search) {
            const form = { owner: root, action: '/Admin/Report', matches: () => true, values: {
                startDate: '2026-11-01', endDate: '2026-11-30', search, page: '8'
            } };
            const e = event(form); listeners['document:submit'](e); return e;
        },
        async respond(i, next = region(), overrides = {}) {
            const token = String(i); parsed.set(token, next);
            requests[i].resolve({ ok: true, redirected: false, text: async () => token, ...overrides });
            await tick();
        },
        async fail(i) { requests[i].reject(new Error('offline')); await tick(); },
        retry() {
            const button = root.retry; button.closest = selector => selector === '[data-report-retry]' ? button : null;
            listeners['document:click'](event(button));
        },
        back() {
            index--; address = new URL(entries[index].url);
            listeners['window:popstate']({ state: entries[index].state });
        },
        hide() { listeners['window:pagehide'](); }
    };
}

test('updates only the latest requested page and retains filter and panel position', async () => {
    const ui = setup();
    const original = ui.root;
    assert.equal(ui.click(2).prevented, true);
    assert.equal(ui.requests[0].options.headers['X-Report-Partial'], 'inventory');
    assert.equal(ui.root.attrs['aria-busy'], 'true');
    ui.click(3);
    assert.equal(ui.requests[0].options.signal.aborted, true);
    const next = ui.region(3);
    await ui.respond(1, next);
    await ui.respond(0, ui.region(2));
    assert.equal(ui.root, next);
    assert.notEqual(ui.root, original);
    assert.equal(ui.root.results.style.minHeight, '800px');
    assert.equal(ui.root.wrap.scrollLeft, 33);
    assert.equal(ui.root.attrs['aria-busy'], 'false');
    assert.equal(ui.url.searchParams.get('page'), '3');
    assert.equal(ui.url.searchParams.get('startDate'), '2026-11-01');
    assert.equal(ui.entries.length, 2);
    assert.equal(ui.scrolls.at(-1).behavior, 'smooth');
    assert.equal(ui.root.focused.preventScroll, true);
});

test('search resets the page, keeps dates and updates Excel and report filters', async () => {
    const ui = setup();
    ui.submit('  Cấu trúc  ');
    const request = new URL(ui.requests[0].url);
    assert.equal(request.searchParams.has('page'), false);
    assert.equal(request.searchParams.get('search'), 'Cấu trúc');
    assert.equal(request.searchParams.get('endDate'), '2026-11-30');
    const next = ui.region(1, 'Cấu trúc');
    await ui.respond(0, next);
    assert.equal(ui.root.results.style.minHeight, undefined);
    assert.equal(ui.searchFilter.value, 'Cấu trúc');
    assert.equal(ui.exportLink.href, next.dataset.exportUrl);
});

test('network failure keeps the previous data and URL and allows retry', async () => {
    const ui = setup();
    const original = ui.root, oldUrl = ui.url.href;
    ui.click(2); await ui.fail(0);
    assert.equal(ui.root, original);
    assert.equal(ui.url.href, oldUrl);
    assert.equal(ui.root.feedback.hidden, false);
    assert.equal(ui.root.retry.hidden, false);
    assert.equal(ui.root.attrs['aria-busy'], 'false');
    ui.retry(); await ui.respond(1, ui.region(2));
    assert.equal(ui.url.searchParams.get('page'), '2');
    assert.equal(ui.root.feedback.hidden, true);
});

test('Back reloads the saved page without adding another history entry', async () => {
    const ui = setup();
    ui.click(2); await ui.respond(0, ui.region(2));
    ui.click(3); await ui.respond(1, ui.region(3));
    const length = ui.entries.length;
    ui.back(); await ui.respond(2, ui.region(2));
    assert.equal(ui.url.searchParams.get('page'), '2');
    assert.equal(ui.entries.length, length);
    assert.equal(ui.scrolls.at(-1).top, 700);
});

test('preserves modified clicks and redirects to login when the session expires', async () => {
    const ui = setup();
    assert.equal(ui.click(2, { ctrlKey: true }).prevented, false);
    assert.equal(ui.requests.length, 0);
    ui.click(2);
    await ui.respond(0, null, { redirected: true, url: 'http://localhost/Account/Login' });
    assert.deepEqual(ui.assigned, ['http://localhost/Account/Login']);
});

test('rejects incomplete fragments, respects reduced motion and cancels on leaving', async () => {
    const ui = setup({ reducedMotion: true });
    const original = ui.root;
    ui.click(2); await ui.respond(0, null);
    assert.equal(ui.root, original);
    assert.equal(ui.root.retry.hidden, false);
    ui.click(2); await ui.respond(1, ui.region(2));
    assert.equal(ui.scrolls.at(-1).behavior, 'instant');
    ui.click(3); ui.hide();
    assert.equal(ui.requests[2].options.signal.aborted, true);
    assert.equal(ui.root.attrs['aria-busy'], 'false');
    await ui.respond(2, ui.region(3));
    assert.equal(ui.url.searchParams.get('page'), '2');
});
