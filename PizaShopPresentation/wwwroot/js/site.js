document.addEventListener("DOMContentLoaded", function () {
  const passwordFields = document.querySelectorAll("#inputPassword");
  const toggleIcons = document.querySelectorAll("#togglePassword");

  if (passwordFields.length > 0 && toggleIcons.length > 0) {
    toggleIcons.forEach((icon, index) => {
      icon.addEventListener("click", function () {
        let passwordField = passwordFields[index];
        if (passwordField.type === "password") {
          passwordField.type = "text";
          icon.src = "/images/icons/passShow.png";
        } else {
          passwordField.type = "password";
          icon.src = "/images/icons/passwordhide.png";
        }
      });
    });
  } else {
    console.error("Password fields or toggle icons not found!");
  }
});
