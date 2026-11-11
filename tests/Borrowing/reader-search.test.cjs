const { test } = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const vm = require('node:vm');
const path = require('node:path');

class Element {
    constructor() {
        this.listeners = {};
        this.attrs = {};
        this.dataset = {};
        this.children = [];
        this.value = '';
        this.textContent = '';
        this.hidden = false;
        this.disabled = false;
        const classes = new Set();
        this.classList = {
            add: name => classes.add(name),
            remove: name => classes.delete(name),
            contains: name => classes.has(name),
            toggle: (name, enabled) => enabled ? classes.add(name) : classes.delete(name)
        };
    }
    addEventListener(name, callback) { (this.listeners[name] ??= []).push(callback); }
    dispatch(name, extra = {}) {
        const event = { target: this, prevented: false, preventDefault() { this.prevented = true; }, stopPropagation() {}, ...extra };
        (this.listeners[name] || []).forEach(callback => callback(event));
        return event;
    }
    setAttribute(name, value) { this.attrs[name] = value; }
    removeAttribute(name) { delete this.attrs[name]; }
    setCustomValidity(value) { this.validityMessage = value; }
    reportValidity() { return !this.validityMessage; }
    querySelector() { return this.strong ??= new Element(); }
    append(...children) { children.forEach(child => { child.parent = this; this.children.push(child); }); }
    replaceChildren() { this.children = []; }
    contains(element) { return element === this || this.children.some(child => child.contains(element)); }
    scrollIntoView() {}
    focus() { this.owner.activeElement = this; this.dispatch('focus'); }
}

const reader = (overrides = {}) => ({
    maBD: 'SV001', hoTen: 'Đỗ Văn Đạt', lopKhoa: 'CNTT 1', soSachDangMuon: 1,
    gioiHanSach: 5, borrowedTitles: ['Borrowed book'], ...overrides
});

function setup() {
    const document = new Element();
    const elements = {};
    document.getElementById = id => elements[id] ??= Object.assign(new Element(), { owner: document });
    document.createElement = () => Object.assign(new Element(), { owner: document });
    const get = document.getElementById;
    get('formCreateBorrow').dataset = { canBorrow: 'true', readerSearchUrl: '/Admin/Circulation/SearchReaders' };
    get('readerSearchContainer').append(get('readerSearch'), get('readerSearchResults'));
    get('selectBook').options = [new Element(), ...['Borrowed book', 'Available book', 'Empty book'].map((title, index) => {
        const option = new Element();
        option.value = String(index + 1);
        option.textContent = title;
        option.dataset = { title, author: 'Author', copies: index === 2 ? '0' : '3' };
        return option;
    })];
    get('selectBook').selectedIndex = 0;
    let nextTimer = 0;
    const timers = new Map();
    const requests = [];
    const context = {
        document, URL, AbortController, window: { location: { origin: 'http://localhost' } },
        setTimeout(callback) { timers.set(++nextTimer, callback); return nextTimer; },
        clearTimeout(id) { timers.delete(id); },
        fetch(url, options) {
            return new Promise((resolve, reject) => requests.push({ url, options, resolve, reject }));
        }
    };
    vm.runInNewContext(fs.readFileSync(path.join(__dirname, '../../wwwroot/js/circulation-borrow.js'), 'utf8'), context);
    document.dispatch('DOMContentLoaded');
    get('readerSearch').focus();
    return {
        get, requests,
        input(value) { get('readerSearch').value = value; get('readerSearch').dispatch('input'); },
        flush() { const pending = Array.from(timers.values()); timers.clear(); pending.forEach(callback => callback()); },
        async respond(index, readers) {
            requests[index].resolve({ ok: true, redirected: false, json: async () => ({ readers }) });
            await new Promise(resolve => setImmediate(resolve));
        },
        chooseBook(index) {
            const books = get('selectBook'); books.selectedIndex = index; books.value = books.options[index].value;
            books.dispatch('change');
        }
    };
}

test('debounces, requires two characters and ignores out-of-order replies', async () => {
    const ui = setup();
    ui.input('d'); ui.flush(); assert.equal(ui.requests.length, 0);
    ui.input('do'); ui.input('do v'); ui.flush(); assert.equal(ui.requests.length, 1);
    assert.equal(ui.requests[0].url.searchParams.get('query'), 'do v');
    ui.input('dat'); ui.flush(); assert.equal(ui.requests[0].options.signal.aborted, true);
    await ui.respond(1, [reader({ maBD: 'LATEST' })]);
    await ui.respond(0, [reader({ maBD: 'STALE' })]);
    ui.get('readerSearchResults').children[0].dispatch('click');
    assert.equal(ui.get('selectedReaderId').value, 'LATEST');
});

test('keyboard selection, duplicate books, no stock and editing an existing selection', async () => {
    const ui = setup(); ui.input('dat'); ui.flush(); await ui.respond(0, [reader()]);
    ui.get('readerSearch').dispatch('keydown', { key: 'ArrowDown' });
    assert.equal(ui.get('readerSearch').attrs['aria-activedescendant'], 'reader-result-0');
    ui.get('readerSearch').dispatch('keydown', { key: 'Enter' });
    assert.equal(ui.get('selectedReaderId').value, 'SV001');
    assert.equal(ui.get('selectBook').disabled, false);
    assert.equal(ui.get('selectBook').options[1].disabled, true);
    assert.equal(ui.get('selectBook').options[3].disabled, true);
    ui.chooseBook(2); assert.equal(ui.get('btnConfirmBorrow').disabled, false);
    assert.equal(ui.get('formCreateBorrow').dispatch('submit').prevented, false);
    ui.input('new reader');
    assert.equal(ui.get('selectedReaderId').value, '');
    assert.equal(ui.get('selectBook').disabled, true);
    assert.equal(ui.get('btnConfirmBorrow').disabled, true);
    assert.equal(ui.get('formCreateBorrow').dispatch('submit').prevented, true);
});

test('quota applies to both student and staff; ambiguous Enter never selects silently', async () => {
    const ui = setup(); ui.input('test'); ui.flush();
    await ui.respond(0, [reader({ soSachDangMuon: 5 }), reader({ maBD: 'GV001', soSachDangMuon: 9, gioiHanSach: 10 })]);
    ui.get('readerSearch').dispatch('keydown', { key: 'Enter' });
    assert.equal(ui.get('selectedReaderId').value, '');
    ui.get('readerSearchResults').children[0].dispatch('click');
    assert.equal(ui.get('selectBook').disabled, true);
    assert.equal(ui.get('readerLimitAlert').classList.contains('d-none'), false);
    ui.input('GV'); ui.flush(); await ui.respond(1, [reader({ maBD: 'GV001', soSachDangMuon: 9, gioiHanSach: 10 })]);
    ui.get('readerSearch').dispatch('keydown', { key: 'Enter' });
    ui.chooseBook(2); assert.equal(ui.get('btnConfirmBorrow').disabled, false);
});

test('empty results, network failure and closing during a request remain safe', async () => {
    const ui = setup(); ui.input('unknown'); ui.flush(); await ui.respond(0, []);
    assert.match(ui.get('readerSearchStatus').textContent, /Không tìm thấy/);
    ui.input('retry'); ui.flush(); ui.requests[1].reject(new Error('Offline'));
    await new Promise(resolve => setImmediate(resolve));
    assert.match(ui.get('readerSearchStatus').textContent, /thử lại/);
    ui.input('pending'); ui.flush(); ui.get('createBorrowModal').dispatch('hidden.bs.modal');
    await ui.respond(2, [reader()]);
    assert.equal(ui.get('readerSearchResults').hidden, true);
    assert.equal(ui.get('selectedReaderId').value, '');
    assert.equal(ui.get('readerSearch').value, '');
});

test('names render as text and Escape dismisses suggestions without selecting', async () => {
    const ui = setup(); ui.input('test'); ui.flush();
    await ui.respond(0, [reader({ hoTen: '<img src=x onerror=alert(1)>' })]);
    assert.equal(ui.get('readerSearchResults').children[0].children[0].textContent, '<img src=x onerror=alert(1)>');
    ui.get('readerSearch').dispatch('keydown', { key: 'Escape' });
    assert.equal(ui.get('readerSearchResults').hidden, true);
    assert.equal(ui.get('selectedReaderId').value, '');
});
