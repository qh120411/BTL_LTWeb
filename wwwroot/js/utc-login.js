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
    document.querySelectorAll("[data-login-username]").forEach(button => {
        button.addEventListener("click", () => {
            username.value = button.dataset.loginUsername;
            password.value = button.dataset.loginPassword;
            username.dispatchEvent(new Event("input", { bubbles: true }));
            password.dispatchEvent(new Event("input", { bubbles: true }));
            username.focus();
        });
    });
});
