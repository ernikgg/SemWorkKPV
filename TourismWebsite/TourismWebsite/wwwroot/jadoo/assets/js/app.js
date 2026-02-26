(function initToursAjaxSearch() {
    const input = document.getElementById("tourSearch");
    const grid = document.getElementById("toursGrid");
    const hint = document.getElementById("tourSearchHint");
    if (!input || !grid) return;

    let timer = null;

    function escapeHtml(s) {
        return (s ?? "").toString()
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#039;");
    }

    function cardHtml(t) {
        return `
      <div class="col-md-4 mb-4">
        <div class="card overflow-hidden shadow position-relative">
          ${t.isTop ? <span class="badge bg-danger position-absolute m-3">Top</span> : ""}
          <img class="card-img-top" src="${escapeHtml(t.imageUrl)}" alt="${escapeHtml(t.title)}" />
          <div class="card-body py-4 px-3">
            <div class="d-flex flex-column flex-lg-row justify-content-between mb-3">
              <h4 class="text-secondary fw-medium">
                <a class="link-900 text-decoration-none stretched-link" href="/tours/${t.id}">
                  ${escapeHtml(t.title)}
                </a>
              </h4>
              <span class="fs-1 fw-medium">${escapeHtml(t.priceText)}</span>
            </div>

            <div class="d-flex align-items-center">
              <img src="/jadoo/assets/img/dest/navigation.svg" style="margin-right: 14px" width="20" alt="navigation" />
              <span class="fs-0 fw-medium">${escapeHtml(t.durationText)}</span>
            </div>
          </div>
        </div>
      </div>
    `;
    }

    function render(items) {
        grid.innerHTML = items.map(cardHtml).join("");
        if (hint) hint.textContent = Found: ${ items.length };
    }

    async function load(q) {
        try {
            if (hint) hint.textContent = "Loading...";
            const res = await fetch("/api/tours?q=" + encodeURIComponent(q), {
                headers: { "Accept": "application/json" }
            });
            const data = await res.json();
            if (!data.ok) {
                if (hint) hint.textContent = "Error";
                return;
            }
            render(data.items || []);
        } catch {
            if (hint) hint.textContent = "Network error";
        }
    }

    input.addEventListener("input", () => {
        clearTimeout(timer);
        timer = setTimeout(() => load(input.value.trim()), 250);
    });
})();
(function initTourFormValidation() {
    const form = document.getElementById("tourForm");
    if (!form) return;

    const title = document.getElementById("tourTitle");
    const price = document.getElementById("tourPrice");
    const duration = document.getElementById("tourDuration");
    const image = document.getElementById("tourImage");
    const submit = document.getElementById("tourSubmit");

    const titleErr = document.getElementById("tourTitleErr");
    const priceErr = document.getElementById("tourPriceErr");
    const durationErr = document.getElementById("tourDurationErr");
    const imageErr = document.getElementById("tourImageErr");

    const rePrice = /^\$\s?\d+(?:[.,]\d+)?k?$/i;     // $5,42k / $4.2k / $15k
    const reDuration = /^\d+\s+Days\s+Trip$/i;       // 10 Days Trip
    const reImg = /^(\/|https?:\/\/).+\.(png|jpg|jpeg|webp|svg)$/i;

    function setInvalid(input, errEl, msg) {
        if (!input) return;
        if (msg) {
            input.classList.add("is-invalid");
            input.classList.remove("is-valid");
            if (errEl) errEl.textContent = msg;
        } else {
            input.classList.remove("is-invalid");
            input.classList.add("is-valid");
            if (errEl) errEl.textContent = "";
        }
    }

    function validate() {
        let ok = true;

        const t = (title?.value || "").trim();
        if (t.length < 3) { setInvalid(title, titleErr, "Title: минимум 3 символа"); ok = false; }
        else setInvalid(title, titleErr, "");

        const p = (price?.value || "").trim();
        if (!rePrice.test(p)) { setInvalid(price, priceErr, "PriceText пример: $5,42k"); ok = false; }
        else setInvalid(price, priceErr, "");

        const d = (duration?.value || "").trim();
        if (!reDuration.test(d)) { setInvalid(duration, durationErr, "DurationText пример: 10 Days Trip"); ok = false; }
        else setInvalid(duration, durationErr, "");

        const i = (image?.value || "").trim();
        if (!reImg.test(i)) { setInvalid(image, imageErr, "ImageUrl пример: /jadoo/assets/img/...jpg"); ok = false; }
        else setInvalid(image, imageErr, "");

        if (submit) submit.disabled = !ok;
        return ok;
    }

    [title, price, duration, image].forEach(x => x?.addEventListener("input", validate));
    form.addEventListener("submit", (ev) => { if (!validate()) ev.preventDefault(); });
    validate();
})();