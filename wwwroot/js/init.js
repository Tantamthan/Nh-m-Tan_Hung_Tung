(() => {
    const submitHiddenForm = (formId) => {
        const form = document.getElementById(formId);
        if (!form) {
            return;
        }

        form.submit();
    };

    window.submitHiddenForm = submitHiddenForm;

    document.addEventListener("click", (event) => {
        const trigger = event.target.closest("[data-submit-form]");
        if (!trigger) {
            return;
        }

        event.preventDefault();
        submitHiddenForm(trigger.getAttribute("data-submit-form"));
    });

    document.addEventListener("contextmenu", (event) => {
        if (event.target.closest("[data-allow-context-menu='true']")) {
            return;
        }

        event.preventDefault();
    });

    window.addEventListener("pageshow", (event) => {
        if (event.persisted) {
            window.location.reload();
        }
    });

    window.addEventListener("popstate", () => {
        if (document.body.dataset.secureContext === "true") {
            window.location.reload();
        }
    });
})();
