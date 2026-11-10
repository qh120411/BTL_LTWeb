document.addEventListener("DOMContentLoaded", () => {
    const toast = document.getElementById("previewMessage");
    let hideTimer;
    function notify(message) {
        toast.textContent = message;
        toast.hidden = false;
        clearTimeout(hideTimer);
        hideTimer = setTimeout(() => { toast.hidden = true; }, 6000);
    }
    document.addEventListener("submit", event => {
        event.preventDefault();
        event.stopImmediatePropagation();
        if (event.target.method.toLowerCase() === "get") {
            const query = (new FormData(event.target).get("keyword") || "").toString().toLocaleLowerCase("vi");
            document.querySelectorAll(".btn-view-copies").forEach(button => {
                const row = button.closest("tr");
                row.hidden = !row.textContent.toLocaleLowerCase("vi").includes(query);
            });
            notify("Đã lọc học liệu minh họa trong bản xem trước.");
        } else {
            notify("Đây là bản xem trước. Thông tin vừa nhập không được lưu.");
        }
    }, true);
    document.querySelectorAll("[data-preview-unavailable]").forEach(link => link.addEventListener("click", event => {
        event.preventDefault();
        notify("Bạn đang xem thử giao diện. Đăng nhập hệ thống để sử dụng chức năng này.");
    }));
    function showModal(id) { bootstrap.Modal.getOrCreateInstance(document.getElementById(id)).show(); }
    document.querySelectorAll(".btn-edit-book").forEach(button => button.addEventListener("click", () => {
        const cells = button.closest("tr").cells;
        document.getElementById("editBookTitle").value = cells[1].querySelector("[title]").title;
        document.getElementById("editBookAuthors").value = cells[2].firstElementChild.textContent.trim();
        document.getElementById("editBookShelf").value = cells[4].textContent.trim();
        document.getElementById("editBookYear").value = "2023";
        document.getElementById("editBookPages").value = "300";
        showModal("editBookModal");
    }));
    document.querySelectorAll(".btn-delete-book").forEach(button => button.addEventListener("click", () => {
        document.getElementById("deleteBookTitle").textContent = button.dataset.title;
        document.getElementById("deleteBookWarning").textContent = "Dữ liệu minh họa. Thao tác xác nhận chỉ hiển thị thông báo.";
        showModal("deleteBookModal");
    }));
    document.querySelectorAll(".btn-view-copies").forEach(button => button.addEventListener("click", () => {
        const cells = button.closest("tr").cells;
        document.getElementById("copiesModalBookTitle").textContent = cells[1].querySelector("[title]").title;
        document.getElementById("copiesModalShelf").textContent = cells[4].textContent.trim();
        document.getElementById("copiesModalStatsBadge").textContent = "2 bản sao minh họa";
        const body = document.getElementById("copiesTableBody");
        body.replaceChildren();
        [1,2].forEach(number => {
            const row = body.insertRow();
            ["BS-MAU-" + number, "UTC-MAU-" + button.dataset.id + "-" + number, cells[4].textContent.trim(), "Tốt", number === 1 ? "Có sẵn" : "Đang mượn"].forEach(value => { row.insertCell().textContent = value; });
        });
        showModal("bookCopiesModal");
    }));
});
