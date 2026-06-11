var scsJobMain = (function () {
    'use strict';

    var scsjobMainInterface = {
        initialize: initialize,
        addSlideoutToogler: addSlideoutToogler
    };

    function initialize() {
       
    }
    function addSlideoutToogler(gridID) {
        //  Call Core function to bind slideout action to grid and search panel
        SearchLayout_AddSlideoutToogler(gridID);
    }


    return scsjobMainInterface;
})();