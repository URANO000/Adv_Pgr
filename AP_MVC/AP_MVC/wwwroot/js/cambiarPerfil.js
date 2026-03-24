$.validator.addMethod("tamanoMaximo", function (value, element) {
    if (element.files.length === 0) return true;
    return element.files[0].size <= 524288;
}, "La imagen no puede superar los 0.5 MB");
