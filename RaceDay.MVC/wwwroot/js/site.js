document.addEventListener("DOMContentLoaded", function ( ) {
    const roleCards = document.querySelectorAll(".role-card");

    roleCards.forEach(card => {
        card.addEventListener("click", function () {
            roleCards.forEach(c => {
                c.classList.add("Selected");
            });

            this.classList.add("selected");
            console.log("selected role:", this.dataset.role);
        });
    });
});

let selectedRole = null;
document.addEventListener("DOMContentLoaded", function () {
    const roleCards = document.querySelectorAll(".role-card");

    roleCards.forEach( card => {
        card.addEventListener("click", function () {
            roleCards.forEach(c => c.classList.remove("selected"));

            this.classList.add("selected");

            selectedRole = this.dataset.role;

            console.log("Selected role:", selectedRole);
        });
    });
});

document.getElementById("registerBtn").addEventListener("click", function () {
    if(!selectedRole) {
        alert("Please select your role first.");
        return;
    }
    window.location.href = '/Account/Regsiter?role=${selectedRole}';
});

document.getElementById("loginBtn").addEventListener("click", function () {
    if(!selectedRole){
        alert("Please select a role first.");
        return;
    }
    window.location.href = '/Account/Login?role=${selectedRole}';
});
