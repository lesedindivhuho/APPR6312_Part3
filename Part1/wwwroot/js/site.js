document.addEventListener("DOMContentLoaded", () => {
    const toggle = document.querySelector(".nav-toggle");
    const navigation = document.querySelector("#main-navigation");

    if (toggle && navigation) {
        toggle.addEventListener("click", () => {
            const isOpen = navigation.classList.toggle("is-open");
            toggle.setAttribute("aria-expanded", String(isOpen));
        });
    }

    const search = document.querySelector("[data-volunteer-search]");
    const statusFilter = document.querySelector("[data-volunteer-status]");
    const availabilityFilter = document.querySelector("[data-volunteer-availability]");
    const rows = Array.from(document.querySelectorAll("[data-volunteer-row]"));
    const emptyState = document.querySelector("[data-volunteer-empty]");

    if (search && statusFilter && availabilityFilter && rows.length > 0) {
        const filterVolunteers = () => {
            const searchTerm = search.value.trim().toLowerCase();
            const status = statusFilter.value;
            const availability = availabilityFilter.value;
            let visibleCount = 0;

            rows.forEach((row) => {
                const matchesSearch = row.dataset.search.toLowerCase().includes(searchTerm);
                const matchesStatus = status === "all" || row.dataset.status === status;
                const matchesAvailability = availability === "all" || row.dataset.availability === availability;
                const isVisible = matchesSearch && matchesStatus && matchesAvailability;

                row.hidden = !isVisible;
                if (isVisible) {
                    visibleCount += 1;
                }
            });

            if (emptyState) {
                emptyState.hidden = visibleCount !== 0;
            }
        };

        search.addEventListener("input", filterVolunteers);
        statusFilter.addEventListener("change", filterVolunteers);
        availabilityFilter.addEventListener("change", filterVolunteers);
    }

    document.querySelectorAll("form[method='post']").forEach((form) => {
        form.addEventListener("submit", (event) => {
            if (!form.checkValidity()) {
                event.preventDefault();
                form.querySelector(":invalid")?.focus();
                return;
            }

            const submitButton = form.querySelector("button[type='submit'], input[type='submit']");
            form.setAttribute("aria-busy", "true");

            if (submitButton) {
                submitButton.disabled = true;
                submitButton.dataset.originalText = submitButton.textContent;
                submitButton.textContent = "Submitting...";
            }
        });
    });
});
