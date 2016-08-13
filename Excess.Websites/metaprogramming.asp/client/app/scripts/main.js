(function(){
    "use strict";
    var App;

    App = {
        init: function() {
            App.setCopyRight();
            // App.setFixedNavbar();
            App.smoothScroll();
        },
        setCopyRight: function() {
            var date, year;
            date = new Date();
            year = date.getFullYear();
            $("#copyright").text(year);
        },
        smoothScroll: function() {
            $(".navbar-nav a").smoothScroll();
            $(".footer-menu a").smoothScroll();
            $(".app-buttons a").smoothScroll();
            $(".app-link a").smoothScroll();
        },
        setFixedNavbar: function() {
            var $win, $header;
            var h = 280;
            $win = $(window);
            $header = $('#header')
            $win.on('scroll', function() {
                if ($win.scrollTop() > h) {
                    $header.addClass('navbar-fixed-top');
                } else {
                    $header.removeClass('navbar-fixed-top');
                }
            })
        }
    }

    App.init();

}).call(this);