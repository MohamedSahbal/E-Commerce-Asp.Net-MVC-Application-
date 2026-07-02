(() => {

    const html = document.documentElement;
    const btn = document.getElementById("themeToggle");

    const savedTheme =
        localStorage.getItem("theme") ??
        (window.matchMedia("(prefers-color-scheme: dark)").matches
            ? "dark"
            : "light");

    setTheme(savedTheme);

    if (btn) {

        btn.addEventListener("click", () => {

            const newTheme =
                html.getAttribute("data-bs-theme") === "dark"
                    ? "light"
                    : "dark";

            setTheme(newTheme);

        });

    }

    function setTheme(theme) {

        html.setAttribute("data-bs-theme", theme);

        localStorage.setItem("theme", theme);

        updateIcon(theme);

    }

    function updateIcon(theme) {

        if (!btn) return;

        btn.innerHTML =
            theme === "dark"
                ? '<i class="bi bi-sun-fill"></i>'
                : '<i class="bi bi-moon-stars-fill"></i>';

    }

})();