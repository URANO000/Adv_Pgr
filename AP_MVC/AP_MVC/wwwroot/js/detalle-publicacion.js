document.addEventListener("DOMContentLoaded", function () {
    const motivo = document.getElementById("motivoEliminacion");
    const otroContenedor = document.getElementById("contenedorOtroMotivo");
    const otroMotivo = document.getElementById("otroMotivo");
    const confirmacion = document.getElementById("confirmacionEliminar");
    const btnConfirmar = document.getElementById("btnConfirmarEliminar");

    if (!motivo || !otroContenedor || !confirmacion || !btnConfirmar) {
        return;
    }

    function validarModalEliminar() {
        const motivoValido = motivo.value !== "";
        const requiereOtro = motivo.value === "otro";
        const otroValido = !requiereOtro || (otroMotivo && otroMotivo.value.trim().length > 0);
        const confirmado = confirmacion.checked;

        btnConfirmar.disabled = !(motivoValido && otroValido && confirmado);
    }

    motivo.addEventListener("change", function () {
        if (motivo.value === "otro") {
            otroContenedor.classList.remove("d-none");
        } else {
            otroContenedor.classList.add("d-none");
            if (otroMotivo) {
                otroMotivo.value = "";
            }
        }

        validarModalEliminar();
    });

    if (otroMotivo) {
        otroMotivo.addEventListener("input", validarModalEliminar);
    }

    confirmacion.addEventListener("change", validarModalEliminar);
});