document.addEventListener('DOMContentLoaded', function () {
    var parallaxElems = document.querySelectorAll('.parallax');
    M.Parallax.init(parallaxElems);

    var sidenavElems = document.querySelectorAll('.sidenav');
    M.Sidenav.init(sidenavElems);

    var collapsibleElems = document.querySelectorAll('.collapsible');
    M.Collapsible.init(collapsibleElems);

    var secureMenuTrigger = document.getElementById('secureMenuTrigger');
    if (secureMenuTrigger) {
        var desktopQuery = window.matchMedia('(min-width: 993px)');
        var collapsedKey = 'ascwed.secureNavCollapsed';
        var secureNav = document.getElementById(secureMenuTrigger.getAttribute('data-target'));

        var applyCollapsedState = function (collapsed) {
            document.body.classList.toggle('secure-nav-collapsed', collapsed);

            if (!secureNav) {
                return;
            }

            if (!desktopQuery.matches) {
                secureNav.style.transform = '';
                secureNav.style.boxShadow = '';
                secureNav.style.pointerEvents = '';
                secureNav.removeAttribute('aria-hidden');
                return;
            }

            secureNav.style.transform = collapsed ? 'translateX(-105%)' : 'translateX(0)';
            secureNav.style.boxShadow = collapsed ? 'none' : '';
            secureNav.style.pointerEvents = collapsed ? 'none' : 'auto';
            secureNav.setAttribute('aria-hidden', collapsed ? 'true' : 'false');
        };

        applyCollapsedState(localStorage.getItem(collapsedKey) === 'true');

        secureMenuTrigger.addEventListener('click', function (event) {
            if (!desktopQuery.matches) {
                return;
            }

            event.preventDefault();
            var collapsed = !document.body.classList.contains('secure-nav-collapsed');
            applyCollapsedState(collapsed);
            localStorage.setItem(collapsedKey, String(collapsed));
        });

        var syncNavState = function (event) {
            if (!event.matches) {
                localStorage.removeItem(collapsedKey);
                document.body.classList.remove('secure-nav-collapsed');
                applyCollapsedState(false);
                return;
            }

            applyCollapsedState(localStorage.getItem(collapsedKey) === 'true');
        };

        if (typeof desktopQuery.addEventListener === 'function') {
            desktopQuery.addEventListener('change', syncNavState);
        } else if (typeof desktopQuery.addListener === 'function') {
            desktopQuery.addListener(syncNavState);
        }
    }

    if (window.history && window.history.pushState) {
        window.history.pushState(null, '', window.location.href);
        window.addEventListener('popstate', function () {
            window.history.pushState(null, '', window.location.href);
        });
    }

    document.addEventListener('contextmenu', function (event) {
        event.preventDefault();
    });
});
