async function loadPartial(selector, url) {
    const target = document.querySelector(selector);
    if (!target) return;
    const res = await fetch(url);
    target.innerHTML = await res.text();
}

function setActiveNav() {
    const page = document.body.dataset.page;
    if (!page) return;
    document.querySelectorAll(".nav-list a").forEach((link) => {
        link.classList.toggle("active", link.dataset.page === page);
    });
}

function initNavToggle() {
    const btn = document.querySelector(".nav-toggle");
    const list = document.querySelector(".nav-list");
    if (!btn || !list) return;

    btn.addEventListener("click", () => {
        const open = btn.getAttribute("aria-expanded") === "true";
        btn.setAttribute("aria-expanded", String(!open));
        btn.setAttribute("aria-label", open ? "Mở menu" : "Đóng menu");
        list.classList.toggle("open", !open);
    });
}

function initReaderNav() {
    const toggle = document.querySelector("[data-utc-nav-toggle]");
    const menu = document.getElementById("utcReaderNav");
    if (!toggle || !menu) return;
    function setOpen(open) {
        menu.classList.toggle("show", open);
        toggle.setAttribute("aria-expanded", String(open));
        toggle.setAttribute("aria-label", open ? "Đóng menu" : "Mở menu");
    }
    toggle.addEventListener("click", () => setOpen(toggle.getAttribute("aria-expanded") !== "true"));
    menu.addEventListener("click", event => { if (event.target.closest("a")) setOpen(false); });
    document.addEventListener("keydown", event => {
        if (event.key === "Escape" && menu.classList.contains("show")) {
            setOpen(false);
            toggle.focus();
        }
    });
}

document.addEventListener("DOMContentLoaded", async () => {
    await Promise.all([
        loadPartial("#site-header", "partials/header.html"),
        loadPartial("#site-footer", "partials/footer.html"),
    ]);
    setActiveNav();
    initNavToggle();
    initReaderNav();
});
