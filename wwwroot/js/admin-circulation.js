// admin-circulation.js - Xử lý quét barcode & nghiệp vụ mượn trả
document.addEventListener("DOMContentLoaded", function () {
    const barcodeInput = document.getElementById("barcodeInput");
    if (barcodeInput) {
        barcodeInput.focus();
        barcodeInput.addEventListener("keypress", function (e) {
            if (e.key === "Enter") {
                e.preventDefault();
                const barcode = this.value.trim();
                if (barcode) {
                    console.log("Đang xử lý quét barcode:", barcode);
                }
            }
        });
    }
});
