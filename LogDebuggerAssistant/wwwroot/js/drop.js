window.dragDropInterop = {
    registerDropZone: function (dropZoneId, dotNetHelper) {
        const dropZone = document.getElementById(dropZoneId);
        if (!dropZone) return;

        // Prevent default drag behavior globally
        document.addEventListener('dragover', function (e) {
            e.preventDefault();
            e.stopPropagation();
        });

        document.addEventListener('drop', function (e) {
            e.preventDefault();
            e.stopPropagation();
        });

        dropZone.addEventListener('dragover', function (e) {
            e.preventDefault();
            e.stopPropagation();
            e.dataTransfer.dropEffect = 'copy';
            dropZone.classList.add('drag-over');
        });

        dropZone.addEventListener('dragleave', function (e) {
            e.preventDefault();
            e.stopPropagation();
            dropZone.classList.remove('drag-over');
        });

        dropZone.addEventListener('drop', async function (e) {
            e.preventDefault();
            e.stopPropagation();
            dropZone.classList.remove('drag-over');

            const files = e.dataTransfer.files;
            if (files.length > 0) {
                const file = files[0];
                try {
                    const arrayBuffer = await file.arrayBuffer();
                    const byteArray = Array.from(new Uint8Array(arrayBuffer));

                    await dotNetHelper.invokeMethodAsync('HandleDroppedFile', {
                        name: file.name,
                        type: file.type,
                        size: file.size,
                        content: byteArray
                    });
                } catch (error) {
                    console.error('File processing error:', error);
                }
            }
        });
    }
};