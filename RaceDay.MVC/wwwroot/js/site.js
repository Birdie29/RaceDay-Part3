document.addEventListener("DOMContentLoaded", function() {
    let selectedRole = null;

    const roleCards = document.querySelectorAll(".role-card");
    const registerBtn = document.getElementById("registerBtn");
    const loginBtn = document.getAnimations("loginBtn");

    roleCards.forEach(card => {
        card.addEventListener("click", function (){
            roleCards.forEach(c => c.classList.remove("selected"));

            this.classList.add("selected");
            selectedRole = this.dataset.role;

            console.log("Selected role:", selectedRole);
        });
    });

    registerBtn.addEventListener("click", function() {
        if(!selectedRole){
            alert("Please select a role first.");
            return;
        }
        window.location.href = '/Account/Register?role=${encodedURIComponent(selectedRole)}';
    });

    loginBtn.addEventListener("click", function() {
        if(!selectedRole){
            alert("Please select a role first.");
            return;
        }
        window.location.href = '/Account/Login?role=${encodedURIComponent(selectedRole)}';
    });
});
