// Shared helpers used across the Tabler-based views.

/**
 * Pagination helper: sets the hidden PageId field on a filter form and submits it.
 */
window.TablerPaging = {
    goTo: function (page, formId, fieldId) {
        var form = document.getElementById(formId);
        var field = document.getElementById(fieldId);
        if (!form || !field || page < 1) {
            return false;
        }
        field.value = page;
        form.submit();
        return false;
    }
};

/**
 * Wires up any element with [data-confirm] to show a Tabler confirmation
 * modal before navigating to its href (used for single delete actions).
 */
(function () {
    document.addEventListener("DOMContentLoaded", function () {
        var modalEl = document.getElementById("confirm-modal");
        if (!modalEl) {
            return;
        }

        var modal = new bootstrap.Modal(modalEl);
        var messageEl = modalEl.querySelector("[data-confirm-message]");
        var confirmBtn = modalEl.querySelector("[data-confirm-accept]");
        var pendingHref = null;

        document.querySelectorAll("[data-confirm]").forEach(function (el) {
            el.addEventListener("click", function (event) {
                event.preventDefault();
                pendingHref = el.getAttribute("href");
                messageEl.textContent = el.getAttribute("data-confirm");
                modal.show();
            });
        });

        confirmBtn.addEventListener("click", function () {
            if (pendingHref) {
                window.location.href = pendingHref;
            }
        });
    });
})();

/**
 * Posts a list of ids as JSON to the given url and calls back with
 * { isSuccess, message, items }.
 */
function tablerPostJson(url, payload, onDone) {
    fetch(url, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload)
    })
        .then(function (response) { return response.json(); })
        .then(function (data) { onDone(data); })
        .catch(function () {
            onDone({ isSuccess: false, message: "An error occurred while contacting the server." });
        });
}
