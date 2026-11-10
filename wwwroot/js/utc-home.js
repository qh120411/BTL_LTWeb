document.addEventListener("DOMContentLoaded", () => {
    const categoryInput = document.getElementById("searchCategory");
    const tabs = document.querySelectorAll("[data-catalog-category]");
    if (!categoryInput) return;
    tabs.forEach(tab => tab.addEventListener("click", () => {
        categoryInput.value = tab.dataset.catalogCategory;
        tabs.forEach(item => {
            const selected = item === tab;
            item.classList.toggle("active", selected);
            item.setAttribute("aria-pressed", String(selected));
        });
    }));
});
