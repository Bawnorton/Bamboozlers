window.settingsInterop = {
    ForceLogout: function () {
        fetch("/Account/DeAuth", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
        })
            .then(async (response) => response)
            .then((result) => window.location.href = result.url)
            .catch((err) => console.error(err))
    },

    Reauthenticate: function () {
        fetch("/Account/ReAuth", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
        })
            .then(async (response) => response)
            .then((result) => {
                console.log(result);
                window.location.href = result.url;
            })
            .catch((err) => console.error(err))
    },

    SendNewEmailConfirmation: function (userId, email) {
        let data = {
            "Id": userId,
            "Email": email
        };
        fetch("/Account/SendConfirmationEmail", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(data)
        })
            .then((response) => response.json())
            .then(() => console.log("success"))
            .catch((err) => console.error(err))
    },
}