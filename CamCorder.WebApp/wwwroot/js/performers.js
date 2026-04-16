(function () {
    const tableBody = document.querySelector('#performers-table tbody');

    function createRow(p) {
        const id = p.id ?? p.Id;
        const name = p.name ?? p.Name ?? '';
        const url = p.url ?? p.Url ?? '';

        const tr = document.createElement('tr');
        tr.dataset.id = String(id);
        tr.innerHTML = `
            <td data-prop="name">${name}</td>
            <td data-prop="url"><a href="${url}" target="_blank">${url}</a></td>
            <td data-prop="actions">
                <a class="btn btn-sm btn-primary" href="/Performer/Details/${id}">Details</a>
                <a class="btn btn-sm btn-secondary" href="/Performer/Edit/${id}">Edit</a>
                <a class="btn btn-sm btn-danger" href="/Performer/Delete/${id}">Delete</a>
            </td>
        `;
        return tr;
    }

    function renderPerformers(performers) {
        tableBody.innerHTML = '';
        performers.forEach(p => tableBody.appendChild(createRow(p)));
    }

    function updateOrInsertPerformer(p) {
        const id = String(p.id ?? p.Id);
        const existing = tableBody.querySelector(`tr[data-id="${id}"]`);
        if (existing) {
            // update only changed cells using data-prop attributes
            const nameCell = existing.querySelector('td[data-prop="name"]');
            const urlCell = existing.querySelector('td[data-prop="url"]');
            const actionsCell = existing.querySelector('td[data-prop="actions"]');

            const name = p.name ?? p.Name ?? '';
            const url = p.url ?? p.Url ?? '';

            // update name only when it changed
            if (nameCell) {
                const currentName = nameCell.textContent?.trim() ?? '';
                if (currentName !== name) {
                    nameCell.textContent = name;
                }
            }

            // update url only when it changed
            if (urlCell) {
                const anchor = urlCell.querySelector('a');
                const currentUrl = anchor ? (anchor.getAttribute('href') ?? anchor.textContent ?? '') : urlCell.textContent?.trim() ?? '';
                if (currentUrl !== url) {
                    // prefer keeping an anchor element
                    if (anchor) {
                        anchor.setAttribute('href', url);
                        anchor.textContent = url;
                    } else {
                        urlCell.innerHTML = `<a href="${url}" target="_blank">${url}</a>`;
                    }
                }
            }

            // update action links if their hrefs differ (id could have changed)
            if (actionsCell) {
                const details = actionsCell.querySelector('a.btn-primary');
                const edit = actionsCell.querySelector('a.btn-secondary');
                const del = actionsCell.querySelector('a.btn-danger');
                if (details) {
                    const want = `/Performer/Details/${id}`;
                    if (details.getAttribute('href') !== want) details.setAttribute('href', want);
                }
                if (edit) {
                    const want = `/Performer/Edit/${id}`;
                    if (edit.getAttribute('href') !== want) edit.setAttribute('href', want);
                }
                if (del) {
                    const want = `/Performer/Delete/${id}`;
                    if (del.getAttribute('href') !== want) del.setAttribute('href', want);
                }
            }
        } else {
            // new performer, append to table
            tableBody.appendChild(createRow(p));
        }
    }

    async function loadPerformers() {
        const resp = await fetch('/Performer/GetAll');
        if (!resp.ok) return;
        const performers = await resp.json();
        renderPerformers(performers);
    }

    // Setup SignalR connection
    const connection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/performer')
        .withAutomaticReconnect()
        .build();

    connection.on('PerformerUpdated', function (performer) {
        // Update only the changed performer row
        try {
            updateOrInsertPerformer(performer);
        } catch (e) {
            console.error('Failed to apply performer update', e);
            // fallback
            loadPerformers();
        }
    });

    connection.start().then(loadPerformers).catch(err => console.error(err));
})();
