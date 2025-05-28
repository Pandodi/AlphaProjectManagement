// Dark Mode Toggle
const darkmodeSwitch = document.querySelector('#darkmode-switch');
const hasDarkmode = localStorage.getItem('darkmode');

if (hasDarkmode === null) {
    window.matchMedia('(prefers-color-scheme: dark)').matches ? enableDarkMode() : disableDarkMode();
} else {
    hasDarkmode === 'on' ? enableDarkMode() : disableDarkMode();
}

darkmodeSwitch.addEventListener('change', () => {
    if (darkmodeSwitch.checked) {
        enableDarkMode();
        localStorage.setItem('darkmode', 'on');
    } else {
        disableDarkMode();
        localStorage.setItem('darkmode', 'off');
    }
});

function enableDarkMode() {
    darkmodeSwitch.checked = true;
    document.documentElement.classList.add('dark');
}

function disableDarkMode() {
    darkmodeSwitch.checked = false;
    document.documentElement.classList.remove('dark');
}

// Main Event Listener
document.addEventListener('DOMContentLoaded', () => {
    const previewSize = 125;

    // Unified Dropdown Handler
    document.body.addEventListener('click', e => {
        const toggle = e.target.closest('.toggle-dropdown, .toggle-member-dropdown, [data-type="dropdown"]');
        if (toggle) {
            e.stopPropagation();
            const wrapper = toggle.closest('.card-wrapper, #notification-container, #settings-container');
            if (!wrapper) {
                console.error('Wrapper not found for toggle:', toggle);
                return;
            }

            const isGenericDropdown = toggle.hasAttribute('data-type') && toggle.getAttribute('data-type') === 'dropdown';
            const dropdownSelector = isGenericDropdown
                ? toggle.getAttribute('data-target')
                : toggle.classList.contains('toggle-dropdown')
                    ? '.edit-project-dropdown'
                    : '.edit-member-dropdown';
            const dropdown = isGenericDropdown ? document.querySelector(dropdownSelector) : wrapper.querySelector(dropdownSelector);

            if (!dropdown) {
                console.error('Dropdown not found:', dropdownSelector, 'in', wrapper);
                return;
            }

            console.log('Toggling dropdown:', dropdown);
            const isVisible = isGenericDropdown ? !dropdown.classList.contains('hide') : dropdown.style.display === 'block';
            // Hide all dropdowns
            document.querySelectorAll('.edit-project-dropdown, .edit-member-dropdown').forEach(d => d.style.display = 'none');
            document.querySelectorAll('.dropdown').forEach(d => d.classList.add('hide'));
            // Toggle target dropdown
            if (isGenericDropdown) {
                dropdown.classList.toggle('hide', isVisible);
            } else {
                dropdown.style.display = isVisible ? 'none' : 'block';
            }
        } else if (!e.target.closest('.card-wrapper, .dropdown')) {
            console.log('Closing all dropdowns');
            document.querySelectorAll('.edit-project-dropdown, .edit-member-dropdown').forEach(d => d.style.display = 'none');
            document.querySelectorAll('.dropdown').forEach(d => d.classList.add('hide'));
        }
    });

    // Modal Handlers
    document.body.addEventListener('click', async e => {
        const button = e.target.closest('[data-type="modals"]');
        if (!button) return;

        e.preventDefault();
        const targetSelector = button.getAttribute('data-target');
        console.log('Opening modal:', targetSelector);

        if (targetSelector === '#edit-project') {
            const projectId = button.getAttribute('data-project-id');
            console.log('Fetching Edit Project modal for ID:', projectId);

            try {
                const response = await fetch(`/Projects/EditProject?id=${encodeURIComponent(projectId)}`, {
                    method: 'GET',
                    headers: { 'Accept': 'text/html' }
                });

                if (!response.ok) {
                    console.error('Failed to fetch partial view:', response.status, response.statusText);
                    alert('Failed to load the Edit Project modal.');
                    return;
                }

                const html = await response.text();
                let modal = document.querySelector('#edit-project');
                if (!modal) {
                    modal = document.createElement('section');
                    modal.id = 'edit-project';
                    modal.className = 'modals';
                    document.body.appendChild(modal);
                }
                modal.innerHTML = html;
                modal.classList.add('open');
                const tagContainer = modal.querySelector('#selected-members-edit');
                const raw = tagContainer?.dataset.preselected || '[]';

                let preselected;
                try {
                    preselected = JSON.parse(raw);
                } catch (err) {
                    console.warn('Failed to parse preselected members JSON', err);
                    preselected = [];
                }

                // ✅ Initialize tag selector for the modal, after content is loaded into DOM
                initTagSelector({
                    containerId: 'selected-members-edit',
                    inputId: 'edit-tag-search',
                    resultsId: 'edit-tag-search-results',
                    searchUrl: query => `/Projects/SearchUsers?term=${encodeURIComponent(query)}`,
                    displayProperty: 'fullName',
                    imageProperty: 'userImage',
                    tagClass: 'user-tag',
                    tagType: 'user',
                    emptyMessage: 'No members found.',
                    selectedInputContainerId: 'hidden-selected-members-edit',
                    preselected: preselected
                });

                // Attach close button
                modal.querySelector('.modal-header .btn')?.addEventListener('click', () => {
                    modal.classList.remove('open');
                    resetModal(modal);
                });

                // Re-initialize Quill for Edit Project
                if (modal.querySelector('#edit-project-wysiwyg-editor')) {
                    setupQuillEditor('#edit-project-wysiwyg-editor', '#edit-project-wysiwyg-toolbar', 'edit-project-description');
                }

                // Attach close button
                modal.querySelector('.modal-header .btn')?.addEventListener('click', () => {
                    modal.classList.remove('open');
                    resetModal(modal);
                });

                // Re-initialize Quill for Edit Project
                if (modal.querySelector('#edit-project-wysiwyg-editor')) {
                    setupQuillEditor('#edit-project-wysiwyg-editor', '#edit-project-wysiwyg-toolbar', 'edit-project-description');
                }

            } catch (error) {
                console.error('Error fetching Edit Project modal:', error);
                alert('An error occurred while loading the Edit Project modal.');
            }
        } else {
            const modal = document.querySelector(targetSelector);
            if (modal) {
                modal.classList.add('open');

                // Set IDs for edit modals
                const idAttr = targetSelector === '#edit-member' ? 'data-member-id' :
                    targetSelector === '#edit-client' ? 'data-client-id' :
                        targetSelector === '#edit-project' ? 'data-project-id' : null;
                if (idAttr) {
                    const id = button.getAttribute(idAttr);
                    const hiddenInput = modal.querySelector(`input[name='Id']`);
                    if (hiddenInput) hiddenInput.value = id;
                }
            } else {
                console.error('Modal not found for selector:', targetSelector);
            }
        }
    });

    // Close Modals
    document.querySelectorAll('.modals').forEach(modal => {
        modal.addEventListener('click', e => {
            const wrapper = modal.querySelector('.modal-wrapper');
            if (!wrapper.contains(e.target)) {
                modal.classList.remove('open');
                resetModal(modal);
            }
        });
    });

    document.querySelectorAll('.modals .modal-header .btn').forEach(closeBtn => {
        closeBtn.addEventListener('click', () => {
            const modal = closeBtn.closest('.modals');
            modal?.classList.remove('open');
            resetModal(modal);
        });
    });

    // Image Previewer
    document.querySelectorAll('.image-previewer').forEach(previewer => {
        const fileInput = previewer.querySelector('input[type="file"]');
        const imagePreview = previewer.querySelector('.image-preview');

        previewer.addEventListener('click', () => fileInput.click());
        fileInput.addEventListener('change', ({ target: { files } }) => {
            const file = files[0];
            if (file) processImage(file, imagePreview, previewer, previewSize);
        });
    });

    // Form Submission
    document.body.addEventListener('submit', async e => {
        const form = e.target;
        if (!(form instanceof HTMLFormElement)) return;
        e.preventDefault();
        clearErrorMessages(form);

        const formData = new FormData(form);
        try {
            const res = await fetch(form.action, { method: 'post', body: formData });
            const data = await res.json();
            if (res.ok && data.success) {
                const modal = form.closest('.modals');
                if (modal) { modal.classList.remove('open'); resetModal(modal); }
                window.location.reload();
            } else if (res.status === 400 && data.errors) {
                Object.entries(data.errors).forEach(([key, msgs]) => {
                    const input = form.querySelector(`[name="${key}"]`);
                    if (input) input.classList.add('input-validation-error');
                    const span = form.querySelector(`[data-valmsg-for="${key}"]`);
                    if (span) { span.innerText = Array.isArray(msgs) ? msgs.join('\n') : msgs; }
                });
            } else {
                alert(`Error: ${data.error || 'Unexpected error'}`);
            }
        } catch (err) {
            console.error('Submit error', err);
            alert('An error occurred while submitting.');
        }
    });

    // Quill Editors
    if (document.querySelector('#edit-project-wysiwyg-editor')) {
        setupQuillEditor('#edit-project-wysiwyg-editor', '#edit-project-wysiwyg-toolbar', 'edit-project-description');
    }
    if (document.querySelector('#project-wysiwyg-editor')) {
        setupQuillEditor('#project-wysiwyg-editor', '#project-wysiwyg-toolbar', 'add-project-description');
    }
});

// Utility Functions
function resetModal(modal) {
    modal.querySelectorAll('form').forEach(form => form.reset());
    modal.querySelectorAll('.image-previewer').forEach(previewer => {
        const img = previewer.querySelector('.image-preview');
        const input = previewer.querySelector('input[type="file"]');
        img.src = '';
        previewer.classList.remove('selected', 'has-image');
        input.value = '';
    });
}

async function loadImage(file) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onerror = () => reject(new Error("Failed to load file."));
        reader.onload = e => {
            const img = new Image();
            img.onerror = () => reject(new Error("Failed to load image"));
            img.onload = () => resolve(img);
            img.src = e.target.result;
        };
        reader.readAsDataURL(file);
    });
}

async function processImage(file, imagePreview, previewer, previewSize = 125) {
    try {
        const img = await loadImage(file);
        const canvas = document.createElement('canvas');
        canvas.width = canvas.height = previewSize;
        const ctx = canvas.getContext('2d');
        ctx.drawImage(img, 0, 0, previewSize, previewSize);
        imagePreview.src = canvas.toDataURL('image/jpeg');
        previewer.classList.add('selected');
    } catch (error) {
        console.error('Failed on image-processing:', error);
        alert('Failed to process the image. Please select a valid image file.');
    }
}

function clearErrorMessages(form) {
    form.querySelectorAll('[data-val="true"]').forEach(input => {
        input.classList.remove('input-validation-error');
    });
    form.querySelectorAll('[data-valmsg-for]').forEach(span => {
        span.innerText = '';
        span.classList.remove('field-validation-error');
    });
}

function setupQuillEditor(editorId, toolbarId, textareaId) {
    const textarea = document.getElementById(textareaId);
    const quill = new Quill(editorId, {
        modules: { syntax: true, toolbar: toolbarId },
        theme: 'snow',
        placeholder: 'Type Something'
    });
    quill.on('text-change', () => {
        textarea.value = quill.root.innerHTML;
    });
}