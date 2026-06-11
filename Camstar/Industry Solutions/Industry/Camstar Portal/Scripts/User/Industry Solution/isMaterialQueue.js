
function MaterialQueue_renderCompleted() {
    var grid = $(this.GridID);
    var rowData = grid.getRowData();
    var validForIssue;
    var issueStatusCell;
    for (var x = 0; x < rowData.length; x++) {
        if (!rowData[x]._id_column.includes("#empty#") && rowData[x].isValidForIssue != "") {

            issueStatusCell = $(".issue-status", grid)[x];
            if (rowData[x].isValidForIssue.toLowerCase() === "false")
                $(issueStatusCell).addClass("error");
            else
                $(issueStatusCell).addClass("satisfied");
        }
    }
}

