document.addEventListener("DOMContentLoaded", function() {
    fetch('http://localhost:8081/api/documents/list')
    .then(response => response.json())
    .then(data => {
        document.getElementById("message").innerText = `Documents: ${data.join(', ')}`;
    })
    .catch(err => {
        document.getElementById("message").innerText = "Error fetching documents.";
    });
});
