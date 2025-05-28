function initTagSelector(config) {
    console.log('initTagSelector called with preselected:', config.preselected);
    let activeIndex = -1;
    let selectedIds = []; // Array to hold selected IDs as strings

    const tagContainer = document.getElementById(config.containerId);
    const input = document.getElementById(config.inputId);
    const results = document.getElementById(config.resultsId);

    // Handle preselected items
    if (Array.isArray(config.preselected)) {
        config.preselected.forEach(item => addTag(item));
    }

    // Focus and blur events for styling
    input.addEventListener('focus', () => {
        tagContainer.classList.add('focused');
        results.classList.add('focused');
    });

    input.addEventListener('blur', () => {
        setTimeout(() => {
            tagContainer.classList.remove('focused');
            results.classList.remove('focused');
        }, 100);
    });

    // Search input event
    input.addEventListener('input', () => {
        const query = input.value.trim();
        activeIndex = -1;

        if (query.length === 0) {
            results.style.display = 'none';
            results.innerHTML = '';
            return;
        }

        fetch(config.searchUrl(query))
            .then(r => r.json())
            .then(data => renderSearchResults(data));
    });

    function renderSearchResults(data) {
        results.innerHTML = '';
        console.log('Rendering with data:', data);

        if (data.length === 0) {
            const noResult = document.createElement('div');
            noResult.classList.add('search-item');
            noResult.textContent = config.emptyMessage || 'No results.';
            results.appendChild(noResult);
        } else {
            data.forEach(item => {
                const idStr = item.id.toString(); // Convert ID to string for consistency
                if (!selectedIds.includes(idStr)) {
                    const resultItem = document.createElement('div');
                    resultItem.classList.add('search-item');
                    resultItem.dataset.id = idStr;

                    if (config.tagType === 'tag') {
                        resultItem.innerHTML = `<span>${item[config.displayProperty]}</span>`;
                    } else if (config.tagType === 'user') {
                        resultItem.innerHTML = `
                            <img class="user-avatar" src="${(config.avatarFolder || '') + (item[config.imageProperty] || '')}">
                            <span>${item[config.displayProperty]}</span>
                        `;
                    }
                    resultItem.addEventListener('click', (event) => {
                        event.stopPropagation();
                        addTag(item);
                    });

                    results.appendChild(resultItem);
                }
            });
        }

        results.style.display = 'block';
    }

    function addTag(item) {
        const id = item.id.toString(); // Ensure ID is a string
        if (selectedIds.includes(id)) return;
        selectedIds.push(id); // Add to selectedIds array
        updateSelectedIdsInput();
        const tag = document.createElement('div');
        tag.classList.add(config.tagClass || 'tag');

        if (config.tagType === 'tag') {
            tag.innerHTML = `<span>${item[config.displayProperty]}</span>`;
        } else if (config.tagType === 'user') {
            tag.innerHTML = `
                <img class="user-avatar" src="${config.avatarFolder || ''}${item[config.imageProperty]}">
                <span>${item[config.displayProperty]}</span>
            `;
        }

        const removeBtn = document.createElement('span');
        removeBtn.textContent = 'x';
        removeBtn.classList.add('btn-remove');
        removeBtn.dataset.id = id; // dataset.id is always a string
        removeBtn.addEventListener('click', (e) => {
            selectedIds = selectedIds.filter(i => i !== id);
            tag.remove();
            updateSelectedIdsInput();
            e.stopPropagation();
        });

        tag.appendChild(removeBtn);
        tagContainer.insertBefore(tag, input);

        input.value = '';
        results.innerHTML = '';
        results.style.display = 'none';

        updateSelectedIdsInput(); // Update hidden inputs after adding
    }

    function removeLastTag() {
        const tags = tagContainer.querySelectorAll(`.${config.tagClass}`);
        if (tags.length === 0) return;

        const lastTag = tags[tags.length - 1];
        const lastId = lastTag.querySelector('.btn-remove').dataset.id; // Already a string
        selectedIds = selectedIds.filter(id => id !== lastId);
        lastTag.remove();
        updateSelectedIdsInput();
    }

    function updateSelectedIdsInput() {
        const hiddenContainer = document.getElementById(config.selectedInputContainerId || 'hidden-selected-members');
        if (hiddenContainer) {
            hiddenContainer.innerHTML = ''; // Clear existing inputs
            selectedIds.forEach(id => {
                const input = document.createElement('input');
                input.type = 'hidden';
                input.name = 'SelectedMembersIds'; // Matches view model property
                input.value = id;
                hiddenContainer.appendChild(input);
            });
        }
    }

    function updateActiveItem(items) {
        items.forEach(item => item.classList.remove('active'));
        if (items[activeIndex]) {
            items[activeIndex].classList.add('active');
            items[activeIndex].scrollIntoView({ block: 'nearest' });
        }
    }

    input.addEventListener('keydown', (e) => {
        const items = results.querySelectorAll('.search-item');

        switch (e.key) {
            case 'ArrowDown':
                e.preventDefault();
                if (items.length > 0) {
                    activeIndex = (activeIndex + 1) % items.length;
                    updateActiveItem(items);
                }
                break;

            case 'ArrowUp':
                e.preventDefault();
                if (items.length > 0) {
                    activeIndex = (activeIndex - 1 + items.length) % items.length;
                    updateActiveItem(items);
                }
                break;

            case 'Enter':
                e.preventDefault();
                if (activeIndex >= 0 && items[activeIndex]) {
                    items[activeIndex].click();
                }
                break;

            case 'Backspace':
                if (input.value === '') {
                    removeLastTag();
                }
                break;
        }
    });
}