document.addEventListener("DOMContentLoaded", function () {
    "use strict";

    const form = document.getElementById("formCreateBorrow");
    if (!form) return;

    const search = document.getElementById("readerSearch");
    const readerId = document.getElementById("selectedReaderId");
    const container = document.getElementById("readerSearchContainer");
    const results = document.getElementById("readerSearchResults");
    const status = document.getElementById("readerSearchStatus");
    const readerCard = document.getElementById("readerInfoCard");
    const limitAlert = document.getElementById("readerLimitAlert");
    const badge = document.getElementById("infoReaderBadge");
    const books = document.getElementById("selectBook");
    const bookCard = document.getElementById("bookPreviewCard");
    const confirm = document.getElementById("btnConfirmBorrow");
    const canBorrow = form.dataset.canBorrow === "true";
    let selectedReader = null;
    let matches = [];
    let activeIndex = -1;
    let timer;
    let request;
    let revision = 0;

    function hideResults() {
        results.hidden = true;
        search.setAttribute("aria-expanded", "false");
        search.removeAttribute("aria-activedescendant");
        activeIndex = -1;
    }

    function cancelSearch() {
        clearTimeout(timer);
        revision++;
        if (request) request.abort();
    }

    function clearReader() {
        selectedReader = null;
        readerId.value = "";
        search.setCustomValidity("Vui lòng chọn bạn đọc trong kết quả tìm kiếm.");
        readerCard.classList.add("d-none");
        limitAlert.classList.add("d-none");
        books.value = "";
        books.disabled = true;
        bookCard.classList.add("d-none");
        confirm.disabled = true;
    }

    function selectReader(reader) {
        cancelSearch();
        selectedReader = reader;
        readerId.value = reader.maBD;
        search.value = reader.hoTen + " · " + reader.maBD;
        search.setCustomValidity("");
        hideResults();
        status.textContent = "";
        document.getElementById("infoReaderName").textContent = reader.hoTen;
        document.getElementById("infoReaderId").textContent = reader.maBD;
        document.getElementById("infoReaderClass").textContent = reader.lopKhoa || "—";
        badge.textContent = "Đang mượn " + reader.soSachDangMuon + "/" + reader.gioiHanSach + " cuốn";
        const atLimit = reader.soSachDangMuon >= reader.gioiHanSach;
        badge.className = "badge px-2 py-1 " + (atLimit ? "bg-danger" : "bg-primary");
        limitAlert.querySelector("strong").textContent = reader.gioiHanSach + "/" + reader.gioiHanSach + " cuốn";
        limitAlert.classList.toggle("d-none", !atLimit);
        readerCard.classList.remove("d-none");
        books.disabled = !canBorrow || atLimit;
        books.value = "";
        bookCard.classList.add("d-none");
        confirm.disabled = true;

        const titles = new Set(reader.borrowedTitles.map(title => title.trim().toLowerCase()));
        Array.from(books.options).slice(1).forEach(option => {
            const borrowed = titles.has((option.dataset.title || "").trim().toLowerCase());
            const empty = Number(option.dataset.copies || 0) <= 0;
            option.dataset.baseLabel = option.dataset.baseLabel || option.textContent.trim();
            option.disabled = borrowed || empty;
            option.textContent = option.dataset.baseLabel + (borrowed ? " [Đã mượn]" : empty ? " [Hết sách]" : "");
        });
        search.focus();
    }

    function activate(index) {
        activeIndex = index;
        Array.from(results.children).forEach((option, i) => {
            option.setAttribute("aria-selected", String(i === index));
        });
        const active = results.children[index];
        if (active) {
            search.setAttribute("aria-activedescendant", active.id);
            active.scrollIntoView({ block: "nearest" });
        }
    }

    function renderMatches() {
        results.replaceChildren();
        matches.forEach((reader, index) => {
            const option = document.createElement("button");
            option.type = "button";
            option.className = "reader-search-option";
            option.id = "reader-result-" + index;
            option.tabIndex = -1;
            option.setAttribute("role", "option");
            option.setAttribute("aria-selected", "false");
            const name = document.createElement("span");
            name.className = "reader-search-name";
            name.textContent = reader.hoTen;
            const details = document.createElement("span");
            details.className = "reader-search-meta";
            details.textContent = reader.maBD + (reader.lopKhoa ? " · " + reader.lopKhoa : "");
            option.append(name, details);
            option.addEventListener("click", () => selectReader(reader));
            results.append(option);
        });
        results.hidden = matches.length === 0 || document.activeElement !== search;
        search.setAttribute("aria-expanded", String(!results.hidden));
        status.textContent = matches.length === 0 ? "Không tìm thấy bạn đọc." :
            matches.length === 10 ? "10 kết quả. Nhập rõ tên hoặc mã để tìm chính xác hơn." : "";
    }

    async function findReaders(keyword, version) {
        request = new AbortController();
        status.textContent = "Đang tìm…";
        search.setAttribute("aria-busy", "true");
        try {
            const url = new URL(form.dataset.readerSearchUrl, window.location.origin);
            url.searchParams.set("query", keyword);
            const response = await fetch(url, { signal: request.signal, headers: { Accept: "application/json" }, cache: "no-store" });
            if (!response.ok || response.redirected) throw new Error("Search unavailable");
            const data = await response.json();
            if (version !== revision) return;
            if (!Array.isArray(data.readers)) throw new Error("Invalid search response");
            matches = data.readers;
            renderMatches();
        } catch (error) {
            if (version !== revision || error.name === "AbortError") return;
            hideResults();
            status.textContent = "Chưa tìm được bạn đọc. Vui lòng thử lại.";
        } finally {
            if (version === revision) search.removeAttribute("aria-busy");
        }
    }

    search.addEventListener("input", () => {
        cancelSearch();
        clearReader();
        matches = [];
        results.replaceChildren();
        hideResults();
        search.removeAttribute("aria-busy");
        const keyword = search.value.trim();
        status.textContent = keyword.length < 2 ? "Nhập ít nhất 2 ký tự." : "";
        if (keyword.length >= 2) {
            const version = revision;
            timer = setTimeout(() => findReaders(keyword, version), 300);
        }
    });

    search.addEventListener("keydown", event => {
        if ((event.key === "ArrowDown" || event.key === "ArrowUp") && matches.length) {
            event.preventDefault();
            results.hidden = false;
            search.setAttribute("aria-expanded", "true");
            activate(event.key === "ArrowDown" ? (activeIndex + 1) % matches.length :
                (activeIndex <= 0 ? matches.length : activeIndex) - 1);
        } else if (event.key === "Enter") {
            event.preventDefault();
            if (!results.hidden && activeIndex >= 0) selectReader(matches[activeIndex]);
            else if (!results.hidden && matches.length === 1) selectReader(matches[0]);
        } else if (event.key === "Escape" && !results.hidden) {
            event.preventDefault();
            event.stopPropagation();
            hideResults();
        }
    });

    search.addEventListener("focus", () => {
        if (!selectedReader && matches.length) {
            results.hidden = false;
            search.setAttribute("aria-expanded", "true");
        }
    });
    results.addEventListener("pointerdown", event => event.preventDefault());
    container.addEventListener("focusout", event => {
        if (!container.contains(event.relatedTarget)) hideResults();
    });
    document.addEventListener("click", event => {
        if (!container.contains(event.target)) hideResults();
    });

    books.addEventListener("change", () => {
        const option = books.options[books.selectedIndex];
        const valid = selectedReader && canBorrow && !books.disabled && books.value && option && !option.disabled;
        confirm.disabled = !valid;
        bookCard.classList.toggle("d-none", !valid);
        if (!valid) return;
        document.getElementById("previewBookTitle").textContent = option.dataset.title;
        document.getElementById("previewBookAuthor").textContent = option.dataset.author;
        document.getElementById("previewBookCover").src = option.dataset.cover || "/images/covers/sach_01.jpg";
        document.getElementById("previewBookStock").textContent = option.dataset.copies + " bản sẵn có";
    });
    form.addEventListener("submit", event => {
        if (!selectedReader || readerId.value !== selectedReader.maBD || confirm.disabled) {
            event.preventDefault();
            search.reportValidity();
        }
    });
    const modal = document.getElementById("createBorrowModal");
    modal.addEventListener("shown.bs.modal", () => search.focus());
    modal.addEventListener("hidden.bs.modal", () => {
        cancelSearch();
        search.value = "";
        search.removeAttribute("aria-busy");
        matches = [];
        status.textContent = "";
        results.replaceChildren();
        hideResults();
        clearReader();
    });
    clearReader();
});
