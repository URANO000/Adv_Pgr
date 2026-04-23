document.addEventListener("DOMContentLoaded", function () {

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

});