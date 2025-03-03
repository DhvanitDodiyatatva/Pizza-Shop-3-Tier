// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener("DOMContentLoaded", function () {
  const passwordFields = document.querySelectorAll("#inputPassword");
  const toggleIcons = document.querySelectorAll("#togglePassword");

  if (passwordFields.length > 0 && toggleIcons.length > 0) {
    toggleIcons.forEach((icon, index) => {
      icon.addEventListener("click", function () {
        let passwordField = passwordFields[index]; // Match the respective input
        if (passwordField.type === "password") {
          passwordField.type = "text";
          icon.src = "/images/icons/passShow.png"; // Ensure correct path
        } else {
          passwordField.type = "password";
          icon.src = "/images/icons/passwordhide.png"; // Ensure correct path
        }
      });
    });
  } else {
    console.error("Password fields or toggle icons not found!");
  }
});
