"use strict";

document.addEventListener("DOMContentLoaded", () => {

    let connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .build();

    let currentUser = "";

    connection.on("UserConnected", (user, userList) => {
        updateUserList(userList);
        appendMessage(`🟢 ${user} se unió al chat`);
    });

    connection.on("UserDisconnected", (user, userList) => {
        updateUserList(userList);
        appendMessage(`🔴 ${user} salió del chat`);
    });

    connection.on("ReceivePublicMessage", (user, message) => {
        appendMessage(`💬 ${user}: ${message}`);
    });

    connection.on("ReceivePrivateMessage", (user, message) => {
        appendMessage(`🔒 [Privado] ${user}: ${message}`);
    });

    connection.start().catch(err => console.error(err));

    document.getElementById("joinChat").addEventListener("click", async () => {
        const name = document.getElementById("username").value.trim();
        if (!name) return alert("Debes ingresar un nombre");

        const ok = await connection.invoke("RegisterUser", name);
        if (!ok) return alert("❌ Ese nombre ya está en uso, intenta otro.");

        currentUser = name;
        document.getElementById("login").style.display = "none";
        document.getElementById("chat").style.display = "block";
        document.getElementById("usernameTitle").innerText = name;

        const users = await connection.invoke("GetActiveUsers");
        updateUserList(users);
    });

    document.getElementById("sendMessage").addEventListener("click", async () => {
        const msg = document.getElementById("message").value.trim();
        const to = document.getElementById("toUser").value.trim();
        if (!msg) return;

        if (to) {
            await connection.invoke("SendPrivateMessage", currentUser, to, msg);
            appendMessage(`📤 [Privado a ${to}] Tú: ${msg}`);
        } else {
            await connection.invoke("SendMessageToAll", currentUser, msg);
        }

        document.getElementById("message").value = "";
    });

    function updateUserList(users) {
        const list = document.getElementById("users");
        list.innerHTML = "";
        users.forEach(u => {
            const li = document.createElement("li");
            li.classList.add("list-group-item");
            li.textContent = u;
            list.appendChild(li);
        });

        const combo = document.getElementById("toUser");
        let currentSelection = combo.value;

        // Limpiar el combo
        combo.innerHTML = "<option value=''>Todos</option>";
        const currentUser = document.getElementById("username").value.trim();

        users.forEach(user => {
            if (user !== currentUser) {
                const option = document.createElement("option");
                option.value = user;
                option.textContent = user;
                combo.appendChild(option);
            }
        });

        if (users.includes(currentSelection)) {
            combo.value = currentSelection;
        } else {
            combo.selectedIndex = 0;
        }
    }

    function appendMessage(text) {
        const msg = document.createElement("div");
        msg.textContent = text;
        document.getElementById("messages").appendChild(msg);
    }
});