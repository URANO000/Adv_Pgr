document.addEventListener("DOMContentLoaded", function () {

    // ================================================
    // RF-022: Filtro por estado
    // ================================================
    var filas = document.querySelectorAll(".fila-solicitud");
    var contador = document.getElementById("contadorSolicitudes");
    var sinResultados = document.getElementById("sinResultados");

    function filtrar(filtro) {
        var visibles = 0;

        filas.forEach(function (fila) {
            var estado = fila.getAttribute("data-estado") || "";
            var mostrar = filtro === "todas" || estado === filtro;
            fila.style.display = mostrar ? "" : "none";
            if (mostrar) visibles++;
        });

        if (contador) contador.textContent = visibles + " solicitud(es)";
        if (sinResultados) sinResultados.classList.toggle("d-none", visibles > 0);
    }

    document.querySelectorAll("[data-filtro]").forEach(function (btn) {
        btn.addEventListener("click", function () {
            document.querySelectorAll("[data-filtro]")
                .forEach(function (b) { b.classList.remove("active-filter"); });
            this.classList.add("active-filter");
            filtrar(this.getAttribute("data-filtro"));
        });
    });

    // ================================================
    // RF-022: Modal de gestión — poblar según botón
    // ================================================
    var modalEl = document.getElementById("modalGestionar");

    if (modalEl) {
        modalEl.addEventListener("show.bs.modal", function (event) {
            var btn = event.relatedTarget;
            var solicitudId = btn.getAttribute("data-solicitud-id");
            var publicacionId = btn.getAttribute("data-publicacion-id");
            var nombre = btn.getAttribute("data-nombre");
            var nuevoEstado = btn.getAttribute("data-nuevo-estado");

            // Textos y colores por estado
            var config = {
                Aprobada: {
                    accion: "aprobar",
                    clase: "btn btn-gestionar-aprobar",
                    icono: "mdi:check",
                    label: "Aprobar"
                },
                Rechazada: {
                    accion: "rechazar",
                    clase: "btn btn-gestionar-rechazar",
                    icono: "mdi:close",
                    label: "Rechazar"
                },
                Revisar: {
                    accion: "marcar como En revisión",
                    clase: "btn btn-gestionar-revisar",
                    icono: "mdi:eye-outline",
                    label: "Marcar en revisión"
                }
            };

            var cfg = config[nuevoEstado] || config["Rechazada"];

            // Texto de confirmación
            var textoEl = document.getElementById("textoConfirmacion");
            if (textoEl) {
                textoEl.textContent =
                    "¿Estás seguro de que querés " + cfg.accion +
                    " la solicitud de " + nombre + "?";
            }

            // Nota de correo (solo Aprobada y Rechazada notifican)
            var notaCorreo = document.getElementById("notaCorreo");
            if (notaCorreo) {
                notaCorreo.classList.toggle("d-none", nuevoEstado === "Revisar");
            }

            // Botón confirmar
            var btnConfirmar = document.getElementById("btnConfirmarGestion");
            if (btnConfirmar) {
                btnConfirmar.className = cfg.clase;
                btnConfirmar.innerHTML =
                    '<iconify-icon icon="' + cfg.icono + '" class="me-1"></iconify-icon>' +
                    cfg.label;
                btnConfirmar.disabled = true;   // requiere checkbox
            }

            // Resetear checkbox de confirmación
            var chk = document.getElementById("checkConfirmar");
            if (chk) chk.checked = false;

            // Hidden inputs del form
            var inputSolicitudId = document.getElementById("inputSolicitudId");
            var inputPublicacionId = document.getElementById("inputPublicacionId");
            var inputNuevoEstado = document.getElementById("inputNuevoEstado");

            if (inputSolicitudId) inputSolicitudId.value = solicitudId;
            if (inputPublicacionId) inputPublicacionId.value = publicacionId;
            if (inputNuevoEstado) inputNuevoEstado.value = nuevoEstado;
        });

        // Limpiar al cerrar
        modalEl.addEventListener("hidden.bs.modal", function () {
            var btnConfirmar = document.getElementById("btnConfirmarGestion");
            if (btnConfirmar) btnConfirmar.disabled = true;

            var chk = document.getElementById("checkConfirmar");
            if (chk) chk.checked = false;
        });
    }

    // ================================================
    // Validación: checkbox habilita el botón confirmar
    // ================================================
    var checkConfirmar = document.getElementById("checkConfirmar");
    var btnConfirmarGestion = document.getElementById("btnConfirmarGestion");

    if (checkConfirmar && btnConfirmarGestion) {
        checkConfirmar.addEventListener("change", function () {
            btnConfirmarGestion.disabled = !this.checked;
        });
    }

});