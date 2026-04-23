document.addEventListener("DOMContentLoaded", function () {
    const stepMascota = document.getElementById("stepMascota");
    const stepPublicacion = document.getElementById("stepPublicacion");
    const btnSiguiente = document.getElementById("btnSiguientePaso");
    const btnVolver = document.getElementById("btnVolverPaso");

    if (!stepMascota || !stepPublicacion) return;

    btnSiguiente?.addEventListener("click", function () {
        const camposPaso1 = stepMascota.querySelectorAll("input, select, textarea");
        let valido = true;

        camposPaso1.forEach(campo => {
            if (!campo.checkValidity()) {
                valido = false;
                campo.classList.add("is-invalid");
            } else {
                campo.classList.remove("is-invalid");
            }
        });

        if (!valido) {
            const primerInvalido = stepMascota.querySelector("input:invalid, select:invalid, textarea:invalid");
            primerInvalido?.focus();
            return;
        }

        stepMascota.classList.remove("active");
        stepPublicacion.classList.add("active");

        window.scrollTo({
            top: 0,
            behavior: "smooth"
        });
    });

    btnVolver?.addEventListener("click", function () {
        stepPublicacion.classList.remove("active");
        stepMascota.classList.add("active");

        window.scrollTo({
            top: 0,
            behavior: "smooth"
        });
    });
});