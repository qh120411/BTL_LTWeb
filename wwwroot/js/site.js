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

document.addEventListener("DOMContentLoaded", async () => {
    await Promise.all([
        loadPartial("#site-header", "partials/header.html"),
        loadPartial("#site-footer", "partials/footer.html"),
    ]);
    setActiveNav();
    initNavToggle();
});
