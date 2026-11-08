document.addEventListener("DOMContentLoaded", function() {
    let selectedRole = null;

    const roleCards = document.querySelectorAll(".role-card");
    const registerBtn = document.getElementById("registerBtn");
    const loginBtn = document.getElementById("loginBtn");

    if (!registerBtn || !loginBtn) {
        return;
    }

    roleCards.forEach(card => {
        card.addEventListener("click", function () {
            roleCards.forEach(c => c.classList.remove("selected"));

            card.classList.add("selected");
            selectedRole = card.dataset.role;

            console.log("Selected role:", selectedRole);
        });
    });

    registerBtn.addEventListener("click", function() {
        if (!selectedRole) {
            alert("Please select a role first.");
            return;
        }
        window.location.href = `/Account/Register?role=${encodeURIComponent(selectedRole)}`;
    });

    loginBtn.addEventListener("click", function() {
        if (!selectedRole) {
            alert("Please select a role first.");
            return;
        }
        window.location.href = `/Account/Login?role=${encodeURIComponent(selectedRole)}`;
    });
});
