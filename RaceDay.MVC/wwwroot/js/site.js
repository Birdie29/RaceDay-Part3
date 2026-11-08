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
