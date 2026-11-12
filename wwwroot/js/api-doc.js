document.addEventListener("DOMContentLoaded", async () => {
    const status = document.getElementById("api-doc-status");
    const token = document.querySelector('input[name="__RequestVerificationToken"]');
    try {
        const response = await fetch("/swagger.json", { credentials: "same-origin", cache: "no-store" });
        if (!response.ok) throw new Error("Cannot load OpenAPI");
        const spec = await response.json();

        // Populate required headers before Swagger validates its input fields.
        for (const path of Object.values(spec.paths || {})) {
            for (const operation of Object.values(path)) {
                if (!operation || typeof operation !== "object") continue;
                for (const parameter of operation.parameters || []) {
                    if (token && parameter.in === "header" && parameter.name === "RequestVerificationToken") {
                        parameter.schema = { ...parameter.schema, default: token.value };
                    }
                }
            }
        }
        window.ui = SwaggerUIBundle({
            spec,
            dom_id: "#swagger-ui",
            deepLinking: true,
            docExpansion: "list",
            withCredentials: true,
            requestInterceptor(request) {
                const target = new URL(request.url, window.location.href);
                if (token && target.origin === window.location.origin && String(request.method || "GET").toUpperCase() === "POST") {
                    request.headers = request.headers || {};
                    request.headers.RequestVerificationToken = token.value;
                }
                return request;
            },
            presets: [SwaggerUIBundle.presets.apis, SwaggerUIStandalonePreset],
            plugins: [SwaggerUIBundle.plugins.DownloadUrl],
            layout: "StandaloneLayout"
        });
        if (status) status.hidden = true;
    } catch {
        if (status) status.textContent = "Không tải được Swagger UI. Vui lòng tải lại trang hoặc mở liên kết đặc tả OpenAPI ở trên.";
    }
});
