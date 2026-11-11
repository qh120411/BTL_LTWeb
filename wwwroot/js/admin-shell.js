document.addEventListener("DOMContentLoaded", () => {
    const header = document.querySelector(".admin-main-header");
    const toggle = document.getElementById("adminSidebarToggle");
    const close = document.getElementById("adminSidebarClose");
    const sidebar = document.getElementById("adminSidebar");
    const backdrop = document.getElementById("adminSidebarBackdrop");
    if (!header || !toggle || !sidebar || !backdrop) return;

    const compact = window.matchMedia("(max-width: 1199.98px)");
    const updateHeaderHeight = () => {
        document.body.style.setProperty("--admin-header-height", `${Math.ceil(header.getBoundingClientRect().height)}px`);
    };

    function setOpen(open, restoreFocus = false) {
        open = open && compact.matches;
        toggle.setAttribute("aria-expanded", String(open));
        toggle.setAttribute("aria-label", open ? "Đóng menu quản trị" : "Mở menu quản trị");
        sidebar.classList.toggle("show", open);
        backdrop.classList.toggle("show", open);
        document.body.classList.toggle("sidebar-open", open);
        sidebar.inert = compact.matches && !open;
        if (open) {
            sidebar.setAttribute("role", "dialog");
            sidebar.setAttribute("aria-modal", "true");
            (close || sidebar.querySelector("a"))?.focus();
        } else {
            sidebar.removeAttribute("role");
            sidebar.removeAttribute("aria-modal");
            if (restoreFocus && compact.matches) toggle.focus();
        }
    }

    toggle.addEventListener("click", () => setOpen(!sidebar.classList.contains("show")));
    close?.addEventListener("click", () => setOpen(false, true));
    backdrop.addEventListener("click", () => setOpen(false, true));
    sidebar.querySelectorAll(".sidebar-link, .btn-back-web").forEach(link => {
        link.addEventListener("click", () => setOpen(false));
    });
    document.addEventListener("keydown", event => {
        if (!compact.matches || !sidebar.classList.contains("show")) return;
        if (event.key === "Escape") {
            event.preventDefault();
            setOpen(false, true);
        } else if (event.key === "Tab") {
            const focusable = [...sidebar.querySelectorAll("a[href], button:not([disabled]), [tabindex='0']")];
            const first = focusable[0];
            const last = focusable[focusable.length - 1];
            if (event.shiftKey && document.activeElement === first) {
                event.preventDefault();
                last?.focus();
            } else if (!event.shiftKey && document.activeElement === last) {
                event.preventDefault();
                first?.focus();
            }
        }
    });

    compact.addEventListener("change", () => setOpen(false));
    window.addEventListener("resize", updateHeaderHeight);
    if (typeof ResizeObserver !== "undefined") new ResizeObserver(updateHeaderHeight).observe(header);
    updateHeaderHeight();
    setOpen(false);
});
