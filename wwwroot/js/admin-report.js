(() => {
    'use strict';

    let inventory = document.querySelector('[data-report-inventory]');
    if (!inventory || !window.fetch || !window.AbortController || !window.history.pushState) return;

    const paths = new Set([window.location.pathname, new URL(inventory.dataset.currentUrl, window.location.href).pathname]);
    let requestId = 0;
    let controller;
    let retry;

    function localUrl(value) {
        const url = new URL(value, window.location.href);
        return url.origin === window.location.origin && paths.has(url.pathname) ? url : null;
    }

    function feedback(message, isError) {
        const box = inventory.querySelector('.report-feedback');
        box.hidden = !message;
        box.classList.toggle('is-loading', !!message && !isError);
        box.classList.toggle('is-error', !!isError);
        box.querySelector('[data-report-feedback]').textContent = message;
        box.querySelector('[data-report-retry]').hidden = !isError;
    }

    function busy(value) {
        inventory.classList.toggle('is-loading', value);
        inventory.setAttribute('aria-busy', String(value));
    }

    function saveScroll() {
        window.history.replaceState({ ...window.history.state, reportInventory: true, scrollY: window.scrollY }, '', window.location.href);
    }

    async function load(value, options = {}) {
        const url = localUrl(value);
        if (!url) return;
        const id = ++requestId;
        controller?.abort();
        controller = new window.AbortController();
        retry = { url: url.href, options };
        busy(true);
        feedback('Đang tải sách…', false);

        try {
            const response = await window.fetch(url.href, {
                headers: { 'X-Report-Partial': 'inventory' },
                credentials: 'same-origin',
                cache: 'no-store',
                signal: controller.signal
            });
            if (id !== requestId) return;
            if (response.redirected) {
                const destination = new URL(response.url, window.location.href);
                if (destination.origin === window.location.origin) window.location.assign(destination.href);
                return;
            }
            if (!response.ok) throw new Error('Report request failed');
            const html = await response.text();
            if (id !== requestId) return;
            const page = new DOMParser().parseFromString(html, 'text/html');
            const next = page.querySelector('[data-report-inventory]');
            if (!next || !next.querySelector('#reportInventory') || !localUrl(next.dataset.currentUrl))
                throw new Error('Report table missing');

            const horizontalScroll = inventory.querySelector('.report-table-wrap').scrollLeft;
            if (!options.search) {
                // Preserve the panel height when the final page has fewer rows.
                next.querySelector('[data-report-results]').style.minHeight =
                    inventory.querySelector('[data-report-results]').getBoundingClientRect().height + 'px';
            }
            if (!options.history) saveScroll();
            inventory.replaceWith(next);
            inventory = next;
            inventory.querySelector('.report-table-wrap').scrollLeft = horizontalScroll;
            const exportLink = document.querySelector('.report-export');
            if (exportLink) exportLink.href = inventory.dataset.exportUrl;
            const searchFilter = document.querySelector('.report-filters input[name="search"]');
            if (searchFilter) searchFilter.value = inventory.querySelector('[name="search"]').value;

            if (!options.history) {
                const canonical = localUrl(inventory.dataset.currentUrl);
                canonical.hash = 'reportBooks';
                if (canonical.href !== window.location.href)
                    window.history.pushState({ reportInventory: true }, '', canonical.href);
            }
            busy(false);
            feedback('', false);
            inventory.querySelector('#reportBooksTitle').focus({ preventScroll: true });
            if (options.history && Number.isFinite(options.scrollY)) {
                window.scrollTo({ top: options.scrollY, behavior: 'instant' });
            } else {
                const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
                inventory.scrollIntoView({ behavior: reducedMotion ? 'instant' : 'smooth', block: 'start' });
            }
            retry = null;
        } catch (error) {
            if (id !== requestId || error.name === 'AbortError') return;
            busy(false);
            feedback('Chưa tải được danh sách sách. Bạn thử lại nhé.', true);
        } finally {
            if (id === requestId) busy(false);
        }
    }

    document.addEventListener('click', event => {
        const retryButton = event.target.closest('[data-report-retry]');
        if (retryButton && inventory.contains(retryButton) && retry) {
            event.preventDefault();
            load(retry.url, retry.options);
            return;
        }
        if (event.defaultPrevented || event.button !== 0 || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;
        const link = event.target.closest('.report-pagination a, [data-report-link]');
        if (!link || !inventory.contains(link) || link.target === '_blank' || link.hasAttribute('download') || !localUrl(link.href)) return;
        event.preventDefault();
        load(link.href, { search: link.hasAttribute('data-report-link') });
    });

    document.addEventListener('submit', event => {
        const form = event.target;
        if (!form.matches('[data-report-search]') || !inventory.contains(form) || event.defaultPrevented) return;
        const url = localUrl(form.action);
        if (!url) return;
        event.preventDefault();
        url.search = '';
        for (const [key, value] of new FormData(form)) {
            if (String(value).trim()) url.searchParams.set(key, String(value).trim());
        }
        url.searchParams.delete('page');
        load(url.href, { search: true });
    });

    window.addEventListener('popstate', event => {
        if (localUrl(window.location.href))
            load(window.location.href, { history: true, search: true, scrollY: event.state?.scrollY });
    });
    window.addEventListener('pagehide', () => {
        ++requestId;
        controller?.abort();
        busy(false);
    });
    saveScroll();
})();
