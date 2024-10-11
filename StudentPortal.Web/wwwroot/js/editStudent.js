let changePhotosCheckbox = document.getElementById("ChangePhotos");
let photosFileInput = document.getElementById("Photos");

photosFileInput.disabled = !photosFileInput.checked;

changePhotosCheckbox.addEventListener("change", () => {
    photosFileInput.disabled = !changePhotosCheckbox.checked;
});