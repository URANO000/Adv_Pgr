$(function () {

    // Toggle label Activa / Inactiva
    var toggle = document.getElementById('IsActive');
    var label = document.getElementById('isActiveLabel');
    if (toggle && label) {
        var updateLabel = function () {
            label.textContent = toggle.checked ? 'Activa' : 'Inactiva';
        };
        toggle.addEventListener('change', updateLabel);
        updateLabel();
    }

    // Modal inactivar: pasar el FundraiserId al hidden input del form
    var modalInactivar = document.getElementById('modalInactivar');
    if (modalInactivar) {
        modalInactivar.addEventListener('show.bs.modal', function (event) {
            var trigger = event.relatedTarget;
            var fundraiserId = trigger ? trigger.getAttribute('data-fundraiser-id') : '';
            document.getElementById('modalFundraiserId').value = fundraiserId;
        });
    }

    // Modal publicación inactiva
    if (typeof mostrarModalInactivo !== 'undefined' && mostrarModalInactivo) {
        var modalInactivo = new bootstrap.Modal(document.getElementById('modalInactivo'));
        modalInactivo.show();
    }

});