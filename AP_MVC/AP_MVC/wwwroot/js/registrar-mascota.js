document.addEventListener("DOMContentLoaded", function () {
    const inputImagenes = document.getElementById("ImagenesArchivos");
    const previewVacio = document.getElementById("previewVacio");
    const previewGrid = document.getElementById("previewGrid");
    const previewContador = document.getElementById("previewContador");

    if (!inputImagenes || !previewVacio || !previewGrid || !previewContador) {
        return;
    }

    const extensionesPermitidas = [".jpg", ".jpeg", ".png", ".webp"];
    let archivosSeleccionados = [];

    function obtenerExtension(nombreArchivo) {
        const ultimoPunto = nombreArchivo.lastIndexOf(".");
        if (ultimoPunto === -1) return "";
        return nombreArchivo.substring(ultimoPunto).toLowerCase();
    }

    function reconstruirInputFiles() {
        const dataTransfer = new DataTransfer();

        archivosSeleccionados.forEach(item => {
            dataTransfer.items.add(item.file);
        });

        inputImagenes.files = dataTransfer.files;
    }

    function actualizarContador() {
        if (archivosSeleccionados.length === 0) {
            previewContador.classList.add("d-none");
            previewContador.textContent = "";
            return;
        }

        previewContador.classList.remove("d-none");
        previewContador.textContent = `${archivosSeleccionados.length} imagen(es) seleccionada(s)`;
    }

    function renderizarPreview() {
        previewGrid.innerHTML = "";

        if (archivosSeleccionados.length === 0) {
            previewGrid.classList.add("d-none");
            previewVacio.classList.remove("d-none");
            actualizarContador();
            return;
        }

        previewVacio.classList.add("d-none");
        previewGrid.classList.remove("d-none");

        archivosSeleccionados.forEach((item, index) => {
            const previewItem = document.createElement("div");
            previewItem.className = "mascota-preview-item";

            previewItem.innerHTML = `
                <div class="mascota-preview-image-box">
                    <img src="${item.url}" alt="Imagen ${index + 1}" class="mascota-preview-image" />
                    <button type="button"
                            class="mascota-preview-remove"
                            data-index="${index}"
                            aria-label="Quitar imagen">
                        ×
                    </button>
                </div>
                <div class="mascota-preview-name" title="${item.file.name}">
                    ${item.file.name}
                </div>
            `;

            previewGrid.appendChild(previewItem);
        });

        actualizarContador();

        const botonesQuitar = previewGrid.querySelectorAll(".mascota-preview-remove");
        botonesQuitar.forEach(boton => {
            boton.addEventListener("click", function () {
                const index = parseInt(this.getAttribute("data-index"), 10);

                if (!isNaN(index)) {
                    URL.revokeObjectURL(archivosSeleccionados[index].url);
                    archivosSeleccionados.splice(index, 1);
                    reconstruirInputFiles();
                    renderizarPreview();
                }
            });
        });
    }

    inputImagenes.addEventListener("change", function () {
        const nuevosArchivos = Array.from(inputImagenes.files || []);

        if (nuevosArchivos.length === 0) {
            return;
        }

        nuevosArchivos.forEach(file => {
            const extension = obtenerExtension(file.name);

            if (!extensionesPermitidas.includes(extension)) {
                return;
            }

            const yaExiste = archivosSeleccionados.some(item =>
                item.file.name === file.name &&
                item.file.size === file.size &&
                item.file.lastModified === file.lastModified
            );

            if (!yaExiste) {
                archivosSeleccionados.push({
                    file: file,
                    url: URL.createObjectURL(file)
                });
            }
        });

        reconstruirInputFiles();
        renderizarPreview();
    });
});