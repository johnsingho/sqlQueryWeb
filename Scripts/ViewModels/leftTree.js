"use strict";

function LeftTreeViewModel() {
    var self = this;
    self.PATH_ICON_DB = "/images/iconDB.png";
    self.PATH_ICON_TAB = "/images/iconTab.png";
    self.curDatabase = ko.observable();
    self.curTable = ko.observable();
    //this.recordSet = ko.observableArray([]);
}


//----------------------------------------------------------------------

$(function () {
    var leftTreeViewModel = new LeftTreeViewModel();
    ko.applyBindings(leftTreeViewModel);
    
    //console.log("jstree, treeModel= " + treeModel.toString); 
    $("#dbTree")
        .on('open_node.jstree', onTreeNodeOpen)
        .on('select_node.jstree', onTreeNodeSelected)
        .jstree({
            'core': {
                "multiple": false
                , "check_callback": true
                //,'data': treeModel
            }
        });

    function isFakeTableNode(sText) {
        return sText && ("数据表" == sText.trim());
    }

    function onTreeNodeOpen(e, data) {
        var nodText = data.node.text;
        var nodParent = data.instance.get_node(data.node.parent);
        if (isFakeTableNode(nodText)) {
            if (nodParent != null) {
                onGetAllTables(nodParent, data.node);
            }
        }
    }

    function onGetAllTables(nodDB, nodTemp) {
        var sDB = nodDB.text;
        $.post('/query/GetAllTables', { sDBname: sDB }, function (data) {
            //console.log("*onGetAllTables ret:" + data.sData);
            if (data.retCode) {
                var dbTree = $.jstree.reference('#dbTree');
                var ltab = JSON.parse(data.sData);
                leftTreeViewModel.curDatabase(sDB);
                dbTree.delete_node(nodTemp);
                console.log("tables cnt=" + ltab.length);
                for (var i in ltab) {
                    var newNode = { "text": ltab[i], icon: leftTreeViewModel.PATH_ICON_TAB };
                    dbTree.create_node(nodDB, newNode);
                }
                dbTree.redraw_node(nodDB);
                //dbTree.redraw(true);
            } else {
                console.log("**GetAllTables fail!");
            }
        },
        "json");

    }

    function myGetParentNode(node, sTreeID) {
        var dbTree = $.jstree.reference(sTreeID);
        var nodParent = dbTree.get_node(node.parent);
        return nodParent;
    }

    function onTreeNodeSelected(node, sel, event) {
        var nodTab = sel.node;
        var nodDB = myGetParentNode(nodTab, "#dbTree");
        if (isFakeTableNode(nodTab.text)
            || "#" == nodDB.id) 
        {
            return;
        }

        var sDB = nodDB.text.trim();
        var sTab = nodTab.text.trim();
        leftTreeViewModel.curTable(sTab);
        leftTreeViewModel.curDatabase(sDB);

        $.get('/query/GetTabData', { sDBname: sDB, sTable: sTab }, function (data) {
            document.getElementById('table_detail_panel').innerHTML = data;

        });
    }


//------------------------------------------------------------------------------------------
    //init tree height
    var h = $(window).innerHeight() - 50;
    $('.lt-scroll').css({ "height": h + "px" });

//    $(document).ready(function () {
//        leftTreeViewModel.initTable();
//    });
});



