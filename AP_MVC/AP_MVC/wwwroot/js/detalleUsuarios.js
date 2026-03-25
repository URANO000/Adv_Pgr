let selectedForm = null;

document.querySelectorAll(".btn-confirm").forEach(btn => {
    btn.addEventListener("click", function () {
        selectedForm = this.closest("form");
        let modal = new bootstrap.Modal(document.getElementById("confirmModal"));
        modal.show();
    });
});

document.getElementById("confirmBtn").addEventListener("click", function () {
    if (selectedForm) {
        selectedForm.submit();
    }
});