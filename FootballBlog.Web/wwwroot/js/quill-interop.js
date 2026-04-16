/**
 * Quill.js JS Interop cho Blazor
 * Dùng cho QuillEditor.razor component trong admin pages
 */
window.QuillInterop = (function () {
    const _instances = {};

    return {
        /**
         * Khởi tạo Quill editor trên element có id = elementId
         * @param {string} elementId - ID của div placeholder
         * @param {string} initialContent - HTML content ban đầu
         * @param {DotNetObjectReference} dotnetRef - .NET reference để callback onChange
         */
        create: function (elementId, initialContent, dotnetRef) {
            const el = document.getElementById(elementId);
            if (!el) return;

            const quill = new Quill(el, {
                theme: 'snow',
                placeholder: 'Viết nội dung tại đây...',
                modules: {
                    toolbar: [
                        [{ header: [2, 3, false] }],
                        ['bold', 'italic', 'underline'],
                        [{ list: 'ordered' }, { list: 'bullet' }],
                        ['blockquote', 'code-block'],
                        ['link', 'image'],
                        ['clean']
                    ]
                }
            });

            if (initialContent) {
                quill.clipboard.dangerouslyPasteHTML(initialContent);
            }

            quill.on('text-change', function () {
                const html = quill.root.innerHTML;
                dotnetRef.invokeMethodAsync('OnContentChanged', html);
            });

            _instances[elementId] = quill;
        },

        /**
         * Lấy HTML content hiện tại
         */
        getContent: function (elementId) {
            const quill = _instances[elementId];
            return quill ? quill.root.innerHTML : '';
        },

        /**
         * Set HTML content (dùng khi load bài viết để edit)
         */
        setContent: function (elementId, html) {
            const quill = _instances[elementId];
            if (quill) {
                quill.clipboard.dangerouslyPasteHTML(html || '');
            }
        },

        /**
         * Dọn dẹp khi component bị dispose
         */
        destroy: function (elementId) {
            delete _instances[elementId];
        }
    };
})();
