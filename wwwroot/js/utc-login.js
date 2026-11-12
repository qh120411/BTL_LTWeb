document.addEventListener("DOMContentLoaded", () => {
    const password = document.getElementById("loginPassword");
    const username = document.getElementById("loginUsername");
    const toggle = document.getElementById("togglePasswordBtn");
    const icon = document.getElementById("toggleEyeIcon");
    if (!password || !username || !toggle) return;
    toggle.addEventListener("click", () => {
        const reveal = password.type === "password";
        password.type = reveal ? "text" : "password";
        toggle.setAttribute("aria-pressed", String(reveal));
        toggle.setAttribute("aria-label", reveal ? "Ẩn mật khẩu" : "Hiện mật khẩu");
        toggle.title = reveal ? "Ẩn mật khẩu" : "Hiện mật khẩu";
        if (icon) icon.className = reveal ? "bi bi-eye-slash" : "bi bi-eye";
    });

    const form = document.getElementById("loginForm");
    const quickButtons = document.querySelectorAll("[data-quick-login]");
    if (!form) return;
    quickButtons.forEach(button => button.addEventListener("click", () => {
        username.value = button.dataset.username;
        password.value = button.dataset.password;
        password.type = "password";
        toggle.setAttribute("aria-pressed", "false");
        toggle.setAttribute("aria-label", "Hiện mật khẩu");
        toggle.title = "Hiện mật khẩu";
        if (icon) icon.className = "bi bi-eye";
        form.requestSubmit();
    }));
    form.addEventListener("submit", () => {
        quickButtons.forEach(button => { button.disabled = true; });
    });
});
