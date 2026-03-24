document.addEventListener('DOMContentLoaded', function () {

    // Khởi tạo Parallax
    var parallaxElems = document.querySelectorAll('.parallax');
    M.Parallax.init(parallaxElems);

    // Khởi tạo Sidenav mobile menu
    var sidenavElems = document.querySelectorAll('.sidenav');
    M.Sidenav.init(sidenavElems);

    // Khởi tạo Collapsible accordion menu
    var collapsibleElems = document.querySelectorAll('.collapsible');
    M.Collapsible.init(collapsibleElems);

});