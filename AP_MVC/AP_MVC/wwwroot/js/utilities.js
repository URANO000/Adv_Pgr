function ConsultarNombre() {

    //Limpiar campos antes de empezar
    $("#PrimerNombre").val("");
    $("#SegundoNombre").val("");
    $("#PrimerApellido").val("");
    $("#SegundoApellido").val("");

    var cedula = $("#Cedula").val().trim();

    if (cedula.length >= 9) {
        $.ajax({
            url: "https://apis.gometa.org/cedulas/" + cedula,
            type: "GET",
            success: function (data) {

                if (data.results && data.results.length > 0) {

                    var persona = data.results[0];

                    //Se utilizan los otros valores de la API
                    $("#PrimerNombre").val(persona.firstname1 || "");
                    $("#SegundoNombre").val(persona.firstname2 || "");
                    $("#PrimerApellido").val(persona.lastname1 || "");
                    $("#SegundoApellido").val(persona.lastname2 || "");
                }
            },
            error: function () {
                //Si ocurre un error
                console.error("Error al consultar la cédula");
            }
        });
    }
}