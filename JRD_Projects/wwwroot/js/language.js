export let translations = {};

// Load JSON file and store translations
export async function loadLanguage(lang) {
    try {
        const res = await fetch(`/Lang/${lang}.json`);
        translations = await res.json();
        applyTranslations();
    } catch (err) {
        console.error("Translation load failed:", err);
    }
}

// Apply translations to DOM
export function applyTranslations() {

    // TEXT CONTENT
    document.querySelectorAll("[data-i18n]").forEach(el => {
        const key = el.getAttribute("data-i18n");
        const translated = translations[key];
        if (!translated) return;

        if (el.tagName === "INPUT") {
            const type = el.type.toLowerCase();
            if (type === "submit" || type === "button") {
                el.value = translated;
                return;
            }
        }

        el.textContent = translated;
    });

    // PLACEHOLDER
    document.querySelectorAll("[data-i18n-placeholder]").forEach(el => {
        const key = el.getAttribute("data-i18n-placeholder");
        const translated = translations[key];
        if (translated) el.placeholder = translated;
    });

    // TITLE
    document.querySelectorAll("[data-i18n-title]").forEach(el => {
        const key = el.getAttribute("data-i18n-title");
        const translated = translations[key];
        if (translated) el.title = translated;
    });

    // ARIA LABEL
    document.querySelectorAll("[data-i18n-aria]").forEach(el => {
        const key = el.getAttribute("data-i18n-aria");
        const translated = translations[key];
        if (translated) el.setAttribute("aria-label", translated);
    });
}

// Update radio button labels
export function updateLanguageLabels(lang) {
    const map = {
        en: { en: "English", fr: "French", es: "Spanish" },
        fr: { en: "Anglais", fr: "Français", es: "Espagnol" },
        es: { en: "Inglés", fr: "Francés", es: "Español" }
    };

    document.getElementById("label-en").textContent = map[lang].en;
    document.getElementById("label-fr").textContent = map[lang].fr;
    document.getElementById("label-es").textContent = map[lang].es;
}

// Save language
export function setLang(lang) {
    localStorage.setItem("lang", lang);
}

// Read language
export function getLang() {
    return localStorage.getItem("lang") || "en";
}

// Sync radio buttons
export function syncRadioButtons() {
    const lang = getLang();
    const rb = document.getElementById("lang-" + lang);
    if (rb) rb.checked = true;
}

// Attach radio listeners
export function attachLanguageEvents() {
    document.querySelectorAll("input[name='lang']").forEach(rb => {
        rb.addEventListener("change", () => {
            const selected = rb.value;
            setLang(selected);
            updateLanguageLabels(selected);
            loadLanguage(selected);
        });
    });
}

// Initialize language system
export function initLanguage() {
    const lang = getLang();      // read saved language
    syncRadioButtons();          // update radio buttons
    updateLanguageLabels(lang);  // update English / Français / Español
    loadLanguage(lang);          // load JSON
    attachLanguageEvents();      // attach radio listeners
}

