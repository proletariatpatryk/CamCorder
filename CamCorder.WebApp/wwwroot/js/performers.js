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
            // new performer, but guard against duplicates by URL or name
            const url = p.url ?? p.Url ?? '';
            // try to find an existing row by url
            let foundByUrl = null;
            if (url) {
                const anchor = tableBody.querySelector(`td[data-prop="url"] a[href="${url}"]`);
                if (anchor) foundByUrl = anchor.closest('tr');
            }

            if (foundByUrl) {
                // update the existing row's dataset id and cells
                foundByUrl.dataset.id = id;
                const nameCell = foundByUrl.querySelector('td[data-prop="name"]');
                if (nameCell) nameCell.textContent = p.name ?? p.Name ?? '';
                const urlCell = foundByUrl.querySelector('td[data-prop="url"]');
                if (urlCell) {
                    const anchor = urlCell.querySelector('a');
                    if (anchor) {
                        anchor.setAttribute('href', url);
                        anchor.textContent = url;
                    } else {
                        urlCell.innerHTML = `<a href="${url}" target="_blank">${url}</a>`;
                    }
                }
            } else {
                tableBody.appendChild(createRow(p));
            }
        }
    }

    async function loadPerformers() {
        const resp = await fetch('/Performer/GetAll');
        if (!resp.ok) return;
        const performers = await resp.json();
        renderPerformers(performers);
    }

    // Add performer by URL input handling
    const addInput = document.getElementById('performer-url-input');
    const addButton = document.getElementById('performer-add-btn');

    function extractUsernameFromUrl(raw) {
        if (!raw) return '';
        let s = raw.trim();
        // remove any query or fragment
        s = s.split('#')[0].split('?')[0];
        // remove trailing slash
        while (s.endsWith('/')) s = s.slice(0, -1);
        try {
            // if it's a full URL, use URL to get pathname
            const u = new URL(s.startsWith('http') ? s : 'https://' + s);
            const parts = u.pathname.split('/').filter(Boolean);
            return parts.length ? decodeURIComponent(parts[parts.length - 1]) : u.hostname;
        } catch (e) {
            // fallback: take last segment after slash
            const parts = s.split('/').filter(Boolean);
            return parts.length ? decodeURIComponent(parts[parts.length - 1]) : s;
        }
    }

    async function addPerformerByUrl(url) {
        if (!url) return;
        const name = extractUsernameFromUrl(url);
        const meta = document.querySelector('meta[name="csrf-token"]');
        const token = meta ? meta.getAttribute('content') : null;

        const form = new FormData();
        form.append('Name', name);
        form.append('Url', url);
        if (token) form.append('__RequestVerificationToken', token);

        try {
            addButton.disabled = true;
            const resp = await fetch('/Performer/Create', {
                method: 'POST',
                body: form,
                credentials: 'same-origin',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest',
                    'Accept': 'application/json'
                }
            });
            // server will notify via SignalR; show toast based on response
            if (resp.ok) {
                try {
                    const json = await resp.json();
                    if (json && json.success) {
                        // If the server returned the created performer in JSON, use it
                        // to update/insert into the table immediately rather than
                        // relying on SignalR notifications.
                        if (json.performer) {
                            try {
                                updateOrInsertPerformer(json.performer);
                                const id = String(json.performer.id ?? json.performer.Id ?? '');
                                if (id) {
                                    recentlyCreatedIds.add(id);
                                    // remove the marker after a short window
                                    setTimeout(() => recentlyCreatedIds.delete(id), 5000);
                                }
                            } catch (e) {
                                console.error('Failed to insert performer from create response', e);
                            }
                        }
                        if (window.Swal) {
                            Swal.fire({
                                toast: true,
                                position: 'top-end',
                                icon: 'success',
                                title: 'Performer added',
                                showConfirmButton: false,
                                timer: 2500
                            });
                        }
                    } else {
                        if (window.Swal) {
                            Swal.fire({
                                toast: true,
                                position: 'top-end',
                                icon: 'error',
                                title: 'Failed to add performer',
                                showConfirmButton: false,
                                timer: 3000
                            });
                        }
                    }
                } catch (e) {
                    console.warn('Create returned non-json response', e);
                }
            } else {
                if (window.Swal) {
                    Swal.fire({
                        toast: true,
                        position: 'top-end',
                        icon: 'error',
                        title: 'Failed to add performer',
                        showConfirmButton: false,
                        timer: 3000
                    });
                }
                console.error('Failed to create performer', resp.status);
            }
        } catch (err) {
            console.error('Error creating performer', err);
        } finally {
            addButton.disabled = false;
            if (addInput) addInput.value = '';
        }
    }

    if (addButton && addInput) {
        addButton.addEventListener('click', () => addPerformerByUrl(addInput.value));
        addInput.addEventListener('keydown', (e) => {
            if (e.key === 'Enter') {
                e.preventDefault();
                addPerformerByUrl(addInput.value);
            }
        });
    }

    // Setup SignalR connection
    const connection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/performer')
        .withAutomaticReconnect()
        .build();

    // track performers we just created from this client so SignalR notifications
    // originating from the server for the same creation don't cause duplicates
    const recentlyCreatedIds = new Set();

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

    // When a new performer is created elsewhere, SignalR will notify and we insert it
    connection.on('PerformerCreated', function (performer) {
        try {
            const id = String(performer?.id ?? performer?.Id ?? '');
            if (id && recentlyCreatedIds.has(id)) {
                // this client already inserted the performer from the Create JSON
                // response; ignore the server notification to avoid duplicates
                recentlyCreatedIds.delete(id);
                return;
            }
            updateOrInsertPerformer(performer);
        } catch (e) {
            console.error('Failed to apply performer created', e);
            loadPerformers();
        }
    });

    // When a performer is deleted, remove the row
    connection.on('PerformerDeleted', function (performerId) {
        try {
            const id = String(performerId);
            const existing = tableBody.querySelector(`tr[data-id="${id}"]`);
            if (existing) existing.remove();
        } catch (e) {
            console.error('Failed to apply performer deleted', e);
            loadPerformers();
        }
    });

    connection.start().then(loadPerformers).catch(err => console.error(err));
})();
