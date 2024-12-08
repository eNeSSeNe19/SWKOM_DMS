document.addEventListener("DOMContentLoaded", function () {
    // Fetch existing documents
    fetch('http://localhost:8081/api/documents/list')
        .then(response => response.json())
        .then(data => {
            const messageDiv = document.getElementById("message");
            messageDiv.innerHTML = ""; // Clear previous content
            data.forEach(doc => {
                const docElement = document.createElement("p");
                docElement.innerText = `Name: ${doc.fileName}, Type: ${doc.fileType}`;
                messageDiv.appendChild(docElement);
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
                resultsDiv.innerHTML = ""; // Add this line to clear old results

                if (data.length === 0) {
                    resultsDiv.innerText = "No results found.";
                    return;
                }

                // Display new results
                data.forEach(result => {
                    const resultElement = document.createElement("p");
                    resultElement.innerHTML = `<strong>File Path:</strong> ${result.filePath}<br><strong>OCR Content:</strong> ${result.ocrContent}`;
                    resultsDiv.appendChild(resultElement);
                });
            })
            .catch(err => {
                console.error(err);
                document.getElementById("searchResults").innerText = "An error occurred while searching.";
            });
    });

});
