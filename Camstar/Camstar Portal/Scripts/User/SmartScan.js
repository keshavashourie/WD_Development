/*
* For extending Smart Scan behavior
*/
(function () {
    let ssns = CR.GetNamespace('CR.SmartScan');

    // To extend functionality:
    //  - Add functions to the Smart Scan namespace object (ssns)
    //  - Configure "Action" type Patterns to call the functions
    // Arguments:
    //  - One argument for each value parsed from the barcode
    //  - Each argument is formatted like:
    //  {
    //      Key: "Batch",
    //      Value: "batch1"
    //  }
    //  - Can also be accessed using the "arguments" object
    ssns.exampleFunction = function (batchName, actionName) {
        alert(`This is the example function.  First arg value: ${batchName?.Value}, Second arg key: ${arguments[1]?.Key}`);
    };
})();