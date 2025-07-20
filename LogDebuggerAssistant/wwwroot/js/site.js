window.registerSidebarOutsideClick = (dotNetHelper) => {
    document.addEventListener('click', function (event) {
        const sidebar = document.getElementById('appSidebar');
        if (!sidebar || !sidebar.offsetParent) return;

        if (!sidebar.contains(event.target)) {
            dotNetHelper.invokeMethodAsync('CollapseSidebar');
        }
    });
};
