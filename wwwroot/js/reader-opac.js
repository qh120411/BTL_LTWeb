// reader-opac.js - Tìm kiếm OPAC debounce
document.addEventListener("DOMContentLoaded", function () {
    const searchInput = document.getElementById("opacSearchInput");
    let debounceTimer;

    if (searchInput) {
        searchInput.addEventListener("input", function () {
            clearTimeout(debounceTimer);
            const query = this.value.trim();
            debounceTimer = setTimeout(() => {
                if (query.length >= 2) {
                    console.log("OPAC searching for:", query);
                }
            }, 300);
        });
    }
});
