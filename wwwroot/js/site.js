// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener("click", function (event) {
    const toggle = event.target.closest("[data-password-toggle]");
    if (!toggle) {
        return;
    }

    const passwordField = toggle.closest(".password-field");
    const input = passwordField?.querySelector("input");
    if (!input) {
        return;
    }

    const shouldShow = input.type === "password";
    input.type = shouldShow ? "text" : "password";
    toggle.classList.toggle("is-visible", shouldShow);
    toggle.setAttribute("aria-label", shouldShow ? "Hide password" : "Show password");
    toggle.setAttribute("title", shouldShow ? "Hide password" : "Show password");
});
