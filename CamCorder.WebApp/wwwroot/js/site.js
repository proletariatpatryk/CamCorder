// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

(() => {
  const storageKey = "theme-preference";
  const systemThemeQuery = window.matchMedia("(prefers-color-scheme: dark)");
  const themeIcons = {
    system: "bi-circle-half",
    light: "bi-sun-fill",
    dark: "bi-moon-stars-fill"
  };

  const isValidTheme = (theme) => ["system", "light", "dark"].includes(theme);

  const getStoredTheme = () => {
    const storedTheme = localStorage.getItem(storageKey);
    return isValidTheme(storedTheme) ? storedTheme : "system";
  };

  const resolveTheme = (theme) => theme === "system"
    ? (systemThemeQuery.matches ? "dark" : "light")
    : theme;

  const applyTheme = (theme) => {
    document.documentElement.setAttribute("data-bs-theme", resolveTheme(theme));
    document.documentElement.setAttribute("data-theme-preference", theme);
  };

  const updateThemeToggle = (theme) => {
    const themeToggle = document.querySelector("[data-theme-toggle]");
    const themeToggleIcon = document.querySelector("[data-theme-toggle-icon]");

    if (themeToggle) {
      const label = `Theme: ${theme.charAt(0).toUpperCase()}${theme.slice(1)}`;
      themeToggle.setAttribute("aria-label", label);
      themeToggle.setAttribute("title", label);
    }

    if (themeToggleIcon) {
      themeToggleIcon.className = `bi ${themeIcons[theme] ?? themeIcons.system}`;
    }

    document.querySelectorAll("[data-theme-option]").forEach((option) => {
      const isActive = option.dataset.themeOption === theme;
      option.classList.toggle("active", isActive);
      option.setAttribute("aria-pressed", isActive.toString());
    });
  };

  const setTheme = (theme) => {
    if (!isValidTheme(theme)) {
      return;
    }

    localStorage.setItem(storageKey, theme);
    applyTheme(theme);
    updateThemeToggle(theme);
  };

  document.addEventListener("DOMContentLoaded", () => {
    const preferredTheme = getStoredTheme();

    applyTheme(preferredTheme);
    updateThemeToggle(preferredTheme);

    document.querySelectorAll("[data-theme-option]").forEach((option) => {
      option.addEventListener("click", () => {
        setTheme(option.dataset.themeOption ?? "system");
      });
    });
  });

  const handleSystemThemeChange = () => {
    if (getStoredTheme() === "system") {
      applyTheme("system");
      updateThemeToggle("system");
    }
  };

  if (typeof systemThemeQuery.addEventListener === "function") {
    systemThemeQuery.addEventListener("change", handleSystemThemeChange);
  } else {
    systemThemeQuery.addListener(handleSystemThemeChange);
  }
})();
