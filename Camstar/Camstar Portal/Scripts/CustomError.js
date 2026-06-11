// Copyright Siemens 2023  
var camstar = camstar || {};

camstar.customError = function() {
    var init = function () {
        $('.details').hide();
        $('.details-header').click(function () {
            $('.details').toggle();
            $('.arrow', this).toggleClass('expanded');
        });
    }; //public members

    return {
        init: init
    };

}();

$(function () {
    camstar.customError.init();
});
