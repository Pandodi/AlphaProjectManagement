const validateField = (field) => {
    const form = document.getElementById("internal-signup")
    const errorSpan = form.querySelector(`span[data-valmsg-for='${field.name}']`)
    if (!errorSpan) return;

    let errorMessage = "";
    const value = field.type === "checkbox" ? field.checked : field.value.trim();

    if (field.hasAttribute("data-val-required") && (value === "" || value === false)) {
        errorMessage = field.getAttribute("data-val-required");
    }

    if (field.hasAttribute("data-val-regex") && typeof value === "string" && value !== "") {
        const pattern = new RegExp(field.getAttribute("data-val-regex-pattern"));
        if (!pattern.test(value)) {
            errorMessage = field.getAttribute("data-val-regex");
        }
    }

    if (errorMessage) {
        field.classList.add("input-validation-error");
        errorSpan.classList.remove("field-validation-valid");
        errorSpan.classList.add("field-validation-error");
        errorSpan.textContent = errorMessage;
    } else {
        field.classList.remove("input-validation-error");
        errorSpan.classList.remove("field-validation-error");
        errorSpan.classList.add("field-validation-valid");
        errorSpan.textContent = "";
    }
};

document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("internal-signup");
    if (!form) return;

    const fields = form.querySelectorAll("input[data-val='true']");

    fields.forEach(field => {
        const eventType = field.type === "checkbox" ? "change" : "input";

        field.addEventListener(eventType, () => validateField(field));
        field.addEventListener("blur", () => validateField(field)); // good for catching last-minute errors
    });
});
