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
 * Reads the page-level antiforgery token rendered by _Layout.
 */
function tablerAntiforgeryToken() {
    var field = document.querySelector('input[name="__RequestVerificationToken"]');
    return field ? field.value : "";
}

/**
 * Upgrades the confirmation step on delete buttons.
 *
 * Each delete button lives inside a real POST form carrying its own antiforgery
 * token, so deleting works with no JavaScript at all. This only intercepts the
 * submit to ask for confirmation first - via the Tabler modal when Tabler's JS
 * is available, and via the browser's own confirm() when it is not. A failure
 * here must never leave the button dead.
 */
(function () {
    document.addEventListener("DOMContentLoaded", function () {
        var modalEl = document.getElementById("confirm-modal");
        var messageEl = modalEl ? modalEl.querySelector("[data-confirm-message]") : null;
        var confirmBtn = modalEl ? modalEl.querySelector("[data-confirm-accept]") : null;
        var pendingForm = null;
        var modal = null;

        // Tabler is loaded from a CDN; if that request failed, fall back to
        // confirm() rather than throwing and unbinding every delete button.
        if (modalEl && typeof bootstrap !== "undefined" && bootstrap.Modal) {
            modal = new bootstrap.Modal(modalEl);
        }

        document.querySelectorAll("[data-confirm]").forEach(function (el) {
            var form = el.closest("form");
            if (!form) {
                return;
            }

            form.addEventListener("submit", function (event) {
                if (form === pendingForm) {
                    return; // confirmed already - let it through
                }

                var message = el.getAttribute("data-confirm");

                if (!modal) {
                    if (!window.confirm(message)) {
                        event.preventDefault();
                    }
                    return;
                }

                event.preventDefault();
                pendingForm = form;
                messageEl.textContent = message;
                modal.show();
            });
        });

        if (confirmBtn) {
            confirmBtn.addEventListener("click", function () {
                if (pendingForm) {
                    pendingForm.submit();
                }
            });
        }
    });
})();

/**
 * Posts a list of ids as JSON to the given url and calls back with
 * { isSuccess, message, items }.
 */
function tablerPostJson(url, payload, onDone) {
    fetch(url, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "RequestVerificationToken": tablerAntiforgeryToken()
        },
        body: JSON.stringify(payload)
    })
        .then(function (response) { return response.json(); })
        .then(function (data) { onDone(data); })
        .catch(function () {
            onDone({ isSuccess: false, message: "An error occurred while contacting the server." });
        });
}
