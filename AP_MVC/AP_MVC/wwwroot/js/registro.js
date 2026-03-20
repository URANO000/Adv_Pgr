$(function () {
    $("#formRegistro").validate({
        rules: {
            Cedula: {
                required: true,
                minlength: 9,
                maxlength: 9
            },
            PrimerNombre: {
                required: true
            },
            PrimerApellido: {
                required: true
            },
            CorreoElectronico: {
                required: true,
                email: true
            },
            Contrasenna: {
                required:true
            }
        },
        messages: {
            Cedula: {
                required: "Cédula es obligatoria."
            },
            PrimerNombre: {
                required: "Nombre es obligatorio."
            },
            PrimerApellido: {
                required: "El primer apellido es requerido."
            },
            CorreoElectronico: {
                required: "El correo es obligatorio.",
                email: "Formato incorrecto"
            },
            Contrasenna: {
                required: "Contraseña es obligatoria."
            }
        },
        errorClass: "text-white",
        errorElement: "span",
        highlight: function (element) {
            $(element).addClass("is-invalid");
        },
        unhighlight: function (element) {
            $(element).removeClass("is-invalid");
        }
    })
})