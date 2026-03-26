document.addEventListener("DOMContentLoaded", function () {
    const imagenes = window.detalleImagenes || [];
    if (!imagenes.length) return;

    const triggers = document.querySelectorAll(".detalle-lightbox-trigger");
    const lightbox = document.getElementById("detalleLightbox");
    const lightboxImg = document.getElementById("detalleLightboxImg");
    const btnCerrar = document.getElementById("detalleLightboxCerrar");
    const btnPrev = document.getElementById("detalleLightboxPrev");
    const btnNext = document.getElementById("detalleLightboxNext");

    if (!lightbox || !lightboxImg || !btnCerrar || !btnPrev || !btnNext) return;

    let indiceActual = 0;

    function mostrarImagen(index) {
        if (!imagenes.length) return;

        if (index < 0) {
            indiceActual = imagenes.length - 1;
        } else if (index >= imagenes.length) {
            indiceActual = 0;
        } else {
            indiceActual = index;
        }

        lightboxImg.src = imagenes[indiceActual];
    }

    function abrirLightbox(index) {
        mostrarImagen(index);
        lightbox.classList.remove("d-none");
        document.body.classList.add("overflow-hidden");
    }

    function cerrarLightbox() {
        lightbox.classList.add("d-none");
        document.body.classList.remove("overflow-hidden");
        lightboxImg.src = "";
    }

    triggers.forEach(trigger => {
        const index = parseInt(trigger.dataset.index || "0", 10);

        trigger.addEventListener("dblclick", function (e) {
            e.preventDefault();
            abrirLightbox(index);
        });

        trigger.addEventListener("click", function (e) {
            if (trigger.classList.contains("detalle-thumbnail-img")) {
                e.preventDefault();
                abrirLightbox(index);
            }
        });
    });

    btnCerrar.addEventListener("click", cerrarLightbox);

    btnPrev.addEventListener("click", function () {
        mostrarImagen(indiceActual - 1);
    });

    btnNext.addEventListener("click", function () {
        mostrarImagen(indiceActual + 1);
    });

    lightbox.addEventListener("click", function (e) {
        if (e.target === lightbox) {
            cerrarLightbox();
        }
    });

    document.addEventListener("keydown", function (e) {
        if (lightbox.classList.contains("d-none")) return;

        if (e.key === "Escape") {
            cerrarLightbox();
        } else if (e.key === "ArrowLeft") {
            mostrarImagen(indiceActual - 1);
        } else if (e.key === "ArrowRight") {
            mostrarImagen(indiceActual + 1);
        }
    });
});