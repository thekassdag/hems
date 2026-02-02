document.addEventListener("DOMContentLoaded", function () {
    const getCodeBtn = document.getElementById("get-code-btn");
    const emailInput = document.getElementById("university-email");
    const loginForm = document.getElementById("login-form");
    const otpInput = document.getElementById("otp-code"); // Added OTP input
    const errorContainer = document.createElement("div");
    errorContainer.className = "text-xs mt-2 text-center";
    
    // Insert error container before the form's first child
    if (loginForm) {
        loginForm.prepend(errorContainer);
    }

    if (getCodeBtn) {
        getCodeBtn.addEventListener("click", async function () {
            const email = emailInput.value;
            errorContainer.textContent = ""; // Clear previous errors

            if (!email) {
                errorContainer.textContent = "Please enter your email address.";
                return;
            }

            // Basic email validation
            if (!/^\S+@\S+\.\S+$/.test(email)) {
                errorContainer.textContent = "Please enter a valid email address.";
                return;
            }

            getCodeBtn.disabled = true;
            getCodeBtn.textContent = "Sending...";

            try {
                let ipAddress = null;
                try {
                    const ipResponse = await fetch('https://api.ipify.org?format=json');
                    const ipData = await ipResponse.json();
                    ipAddress = ipData.ip;
                } catch (ipError) {
                    console.warn("Could not fetch public IP address:", ipError);
                    // Continue without IP if fetching fails, or handle as an error if critical
                }

                const response = await fetch('/auth/request-otp', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'Accept': 'application/json'
                    },
                    body: JSON.stringify({ email: email, ipAddress: ipAddress })
                });

                const result = await response.json();

                if (response.ok) {
                    errorContainer.className = "text-green-500 text-xs mt-2 text-center";
                    errorContainer.textContent = result.message;
                } else {
                    errorContainer.className = "text-red-500 text-xs mt-2 text-center";
                    errorContainer.textContent = result.message || "An unknown error occurred.";
                }
            } catch (error) {
                console.error("Error requesting OTP:", error);
                errorContainer.textContent = "A network error occurred. Please try again.";
            } finally {
                getCodeBtn.disabled = false;
                getCodeBtn.textContent = "Get Code";
            }
        });
    }

    // Login form submission for OTP verification
    if (loginForm) {
        loginForm.addEventListener("submit", async function (event) {
            event.preventDefault(); // Prevent default form submission

            const email = emailInput.value;
            const otpCode = otpInput.value;
            errorContainer.textContent = ""; // Clear previous errors

            if (!email) {
                errorContainer.textContent = "Please enter your email address.";
                return;
            }
            if (!otpCode) {
                errorContainer.textContent = "Please enter the OTP code.";
                return;
            }
            if (otpCode.length !== 6) {
                errorContainer.textContent = "OTP code must be 6 digits.";
                return;
            }

            const loginButton = loginForm.querySelector('button[type="submit"]');
            loginButton.disabled = true;
            loginButton.textContent = "Logging in...";

            try {
                const response = await fetch('/auth/verify-otp', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'Accept': 'application/json'
                    },
                    body: JSON.stringify({ email: email, otpCode: otpCode })
                });

                const result = await response.json();

                if (response.ok) {
                    errorContainer.className = "text-green-500 text-xs mt-2 text-center";
                    errorContainer.textContent = result.message;
                    window.location.href = "/dashboard";
                } else {
                    errorContainer.className = "text-red-500 text-xs mt-2 text-center";
                    errorContainer.textContent = result.message || "An unknown error occurred during login.";
                }
            } catch (error) {
                console.error("Error verifying OTP:", error);
                errorContainer.textContent = "A network error occurred during login. Please try again.";
            } finally {
                loginButton.disabled = false;
                loginButton.innerHTML = `Login <span class="material-symbols-outlined text-[18px]"><i class="fa-solid fa-arrow-right"></i></span>`;
            }
        });
    }
});
