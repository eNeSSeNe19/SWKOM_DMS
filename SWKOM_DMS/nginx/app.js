document.addEventListener("DOMContentLoaded", function () {
    // Fetch existing documents
    fetch('http://localhost:8081/api/documents/list')
        .then(response => response.json())
        .then(data => {
            const messageDiv = document.getElementById("message");
            messageDiv.innerHTML = ""; // Clear previous content
            data.forEach(doc => {
                const docElement = document.createElement("div");
                docElement.classList.add("doc-item");
                docElement.innerHTML = `
                    <p>Name: ${doc.fileName}, Type: ${doc.fileType}</p>
                    <button class="delete-btn" data-id="${doc.id}">Delete</button>
                `;
                messageDiv.appendChild(docElement);
            });

            // Attach delete functionality to each button
            document.querySelectorAll(".delete-btn").forEach(button => {
                button.addEventListener("click", function () {
                    const documentId = this.getAttribute("data-id");
                    if (confirm("Are you sure you want to delete this document?")) {
                        fetch(`http://localhost:8081/api/documents/${documentId}`, {
                            method: "DELETE",
                        })
                            .then(response => {
                                if (response.ok) {
                                    this.parentElement.remove(); // Remove item from the UI
                                    alert("Document deleted successfully!");
                                } else {
                                    alert("Failed to delete document.");
                                }
                            })
                            .catch(error => {
                                console.error("Error deleting document:", error);
                                alert("An error occurred while deleting the document.");
                            });
                    }
                });
            });
        })
        .catch(err => {
            document.getElementById("message").innerText = "Error fetching documents.";
        });

    // Handle the file upload
    const uploadForm = document.getElementById("uploadForm");
    uploadForm.addEventListener("submit", function (event) {
        event.preventDefault();

        const fileInput = document.getElementById("file");
        const file = fileInput.files[0];
        if (!file) {
            document.getElementById("uploadMessage").innerText = "Please select a file.";
            return;
        }

        const formData = new FormData();
        formData.append("file", file);
        formData.append("fileType", file.type); // Add file type explicitly

        fetch('http://localhost:8081/api/documents/upload', {
            method: "POST",
            body: formData,
        })
            .then(response => {
                if (response.ok) {
                    document.getElementById("uploadMessage").innerText = "File uploaded successfully!";
                    fileInput.value = ""; // Clear the input
                } else {
                    document.getElementById("uploadMessage").innerText = "Failed to upload file.";
                }
            })
            .catch(error => {
                console.error(error);
                document.getElementById("uploadMessage").innerText = "An error occurred during the upload.";
            });
    });

    // Handle the search
    const searchForm = document.getElementById("searchForm");
    searchForm.addEventListener("submit", function (event) {
        event.preventDefault();

        const queryInput = document.getElementById("query");
        const query = queryInput.value.trim();

        if (!query) {
            document.getElementById("searchResults").innerText = "Please enter a search term.";
            return;
        }

        fetch(`http://localhost:8081/api/documents/search?query=${encodeURIComponent(query)}`)
            .then(response => response.json())
            .then(data => {
                const resultsDiv = document.getElementById("searchResults");

                // Clear previous results
                resultsDiv.innerHTML = "";

                if (data.length === 0) {
                    resultsDiv.innerText = "No results found.";
                    return;
                }

                // Render search results without Delete button
                data.forEach(result => {
                    const highlightedContent = result.ocrContent.replace(
                        new RegExp(`(${query})`, "gi"), // Match the query, case-insensitive
                        `<span class="highlight">$1</span>` // Wrap matched text with a span
                    );

                    const resultElement = document.createElement("div");
                    resultElement.classList.add("result-item");
                    resultElement.innerHTML = `
                        <p><strong>File Path:</strong> ${result.filePath}</p>
                        <p><strong>OCR Content:</strong> ${highlightedContent}</p>
                    `;

                    resultsDiv.appendChild(resultElement);
                });
            })
            .catch(err => {
                console.error(err);
                document.getElementById("searchResults").innerText = "An error occurred while searching.";
            });
    });
});