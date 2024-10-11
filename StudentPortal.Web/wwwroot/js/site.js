// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
window.onload = () => {
    let currentYearElement = document.getElementById("CurrentYearSpan");

    if (currentYearElement !== undefined) {
        currentYearElement.innerHTML = new Date().getFullYear();
    }
};