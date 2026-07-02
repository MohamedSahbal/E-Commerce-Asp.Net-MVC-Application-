document.addEventListener("DOMContentLoaded", () => {

    const mainImage = document.getElementById("mainImage");

    // Thumbnail Switcher
    document.querySelectorAll(".thumb-img").forEach(img => {

        img.addEventListener("click", function () {

            const imageUrl = this.src;

            if (mainImage) {

                mainImage.src = imageUrl;
                mainImage.setAttribute("data-zoom", imageUrl);

            }

            document.querySelectorAll(".thumb-img").forEach(t => {
                t.classList.remove("border-primary", "border-2");
            });

            this.classList.add("border-primary", "border-2");

        });

    });

    // Rating
    const stars = document.querySelectorAll(".star-btn");
    const ratingInput = document.getElementById("ratingInput");

    if (stars.length && ratingInput) {

        stars.forEach(star => {

            star.addEventListener("click", () => {

                const value = parseInt(star.dataset.value);

                ratingInput.value = value;

                stars.forEach((s, index) => {

                    s.classList.toggle("bi-star-fill", index < value);
                    s.classList.toggle("bi-star", index >= value);

                });

            });

        });

    }

    // Drift Zoom
    if (mainImage && typeof Drift !== "undefined") {

        new Drift(mainImage, {
            hoverBoundingBox: true,
            zoomFactor: 3
        });

    }

});