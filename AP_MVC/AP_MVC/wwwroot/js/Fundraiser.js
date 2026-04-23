$(function () {

    // Habilitar botón confirmar al marcar el checkbox
    var chk = document.getElementById('confirmacionInactivar');
    var btn = document.getElementById('btnConfirmarInactivar');
    if (chk && btn) {
        chk.addEventListener('change', function () {
            btn.disabled = !this.checked;
        });
    }

    // Mostrar/ocultar campo "otro motivo"
    var selectMotivo = document.getElementById('motivoInactivacion');
    var contenedorOtro = document.getElementById('contenedorOtroMotivo');
    if (selectMotivo && contenedorOtro) {
        selectMotivo.addEventListener('change', function () {
            if (this.value === 'otro') {
                contenedorOtro.classList.remove('d-none');
            } else {
                contenedorOtro.classList.add('d-none');
            }
        });
    }

    // ================================================
    // RF-021: Filtros del catálogo
    // ================================================
    var inputBusqueda = document.getElementById('inputBusqueda');
    var inputFiltroAnimal = document.getElementById('inputFiltroAnimal');
    var btnLimpiar = document.getElementById('btnLimpiarFiltros');
    var gridFundraisers = document.getElementById('gridFundraisers');
    var sinResultados = document.getElementById('sinResultados');
    var contadorResultados = document.getElementById('contadorResultados');

    function aplicarFiltros() {
        if (!gridFundraisers) return;

        var textoBusqueda = inputBusqueda ? inputBusqueda.value.trim().toLowerCase() : '';
        var textoAnimal = inputFiltroAnimal ? inputFiltroAnimal.value.trim().toLowerCase() : '';

        var items = gridFundraisers.querySelectorAll('.fundraiser-card-item');
        var visibles = 0;

        items.forEach(function (item) {
            var titulo = (item.getAttribute('data-titulo') || '');
            var descripcion = (item.getAttribute('data-descripcion') || '');
            var animal = (item.getAttribute('data-animal') || '');

            var coincideBusqueda = !textoBusqueda ||
                titulo.includes(textoBusqueda) ||
                descripcion.includes(textoBusqueda);

            var coincideAnimal = !textoAnimal || animal.includes(textoAnimal);

            if (coincideBusqueda && coincideAnimal) {
                item.classList.remove('d-none');
                visibles++;
            } else {
                item.classList.add('d-none');
            }
        });

        if (contadorResultados) {
            contadorResultados.textContent = visibles + ' campaña(s) encontrada(s)';
        }

        if (sinResultados) {
            sinResultados.classList.toggle('d-none', visibles > 0);
        }
    }

    if (inputBusqueda) {
        inputBusqueda.addEventListener('input', aplicarFiltros);
    }
    if (inputFiltroAnimal) {
        inputFiltroAnimal.addEventListener('input', aplicarFiltros);
    }
    if (btnLimpiar) {
        btnLimpiar.addEventListener('click', function () {
            if (inputBusqueda) inputBusqueda.value = '';
            if (inputFiltroAnimal) inputFiltroAnimal.value = '';
            aplicarFiltros();
        });
    }

    // ================================================
    // RF-022: Filtro activas/inactivas en MisDonaciones
    // ================================================
    var botonesEstado = document.querySelectorAll('[data-filtro]');
    if (botonesEstado.length > 0) {
        botonesEstado.forEach(function (btn) {
            btn.addEventListener('click', function () {
                // Quitar clase active de todos
                botonesEstado.forEach(function (b) { b.classList.remove('active-filter', 'btn-primary', 'btn-success', 'btn-secondary'); });
                btn.classList.add('active-filter');

                var filtro = btn.getAttribute('data-filtro');
                var filas = document.querySelectorAll('.fila-historial');
                var visibles = 0;

                filas.forEach(function (fila) {
                    var estado = fila.getAttribute('data-estado');
                    var mostrar = filtro === 'todas' || estado === filtro;
                    fila.style.display = mostrar ? '' : 'none';
                    if (mostrar) visibles++;
                });

                var contador = document.getElementById('contadorHistorial');
                if (contador) contador.textContent = visibles + ' publicación(es)';
            });
        });
    }

    // ================================================
    // MisDonaciones: pasar fundraiserId al modal de inactivar
    // ================================================
    var modalInactivar = document.getElementById('modalInactivar');
    if (modalInactivar) {
        modalInactivar.addEventListener('show.bs.modal', function (event) {
            var boton = event.relatedTarget; // el botón que disparó el modal
            var fundraiserId = boton.getAttribute('data-fundraiser-id');

            // Poner el id en el hidden input
            document.getElementById('hiddenFundraiserId').value = fundraiserId;

            // Resetear el modal para cada apertura
            var chk = document.getElementById('confirmacionInactivar');
            var btn = document.getElementById('btnConfirmarInactivar');
            var select = document.getElementById('motivoInactivacion');
            var contenedor = document.getElementById('contenedorOtroMotivo');

            if (chk) chk.checked = false;
            if (btn) btn.disabled = true;
            if (select) select.value = '';
            if (contenedor) contenedor.classList.add('d-none');
        });
    }

    // ================================================
    // RF-020: Montos rápidos en modal de donación
    // ================================================
    var botonesMontoRapido = document.querySelectorAll('.btn-monto-rapido');
    var inputMonto = document.getElementById('monto');

    if (botonesMontoRapido.length > 0 && inputMonto) {
        botonesMontoRapido.forEach(function (btn) {
            btn.addEventListener('click', function () {
                inputMonto.value = btn.getAttribute('data-monto');
                // Resaltar botón seleccionado
                botonesMontoRapido.forEach(function (b) { b.classList.remove('btn-success'); b.classList.add('btn-outline-success'); });
                btn.classList.remove('btn-outline-success');
                btn.classList.add('btn-success');
            });
        });
    }

});