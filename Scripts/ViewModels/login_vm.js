
function LoginViewModel() {
    var self = this;
    self.sHost = ko.observable("");
    self.sUser = ko.observable("");
    self.sPassword = ko.observable("");

    self.sending = ko.observable(false);
    
    self.doQuery = function() {
        self.sending(true);
        var hostInfo = { host: self.sHost, user: self.sUser, pass: self.sPassword };
        var vData = ko.toJSON(hostInfo);
        //console.log(vData);
        $.ajax({
            url: '/query/Login',
            type: 'POST',
            contentType: 'application/json',
            dataType: 'json',
            data: vData
        }).success(self.succConn).error(self.errConn).complete(function () { self.sending(false); });
    };

    self.succConn = function (data) {
        console.log("connect to.\n ret:" + data.toString());
        if (data.retCode) {
            window.location.href = data.sData;
        }else{
            alert("连接到数据库失败！");
        }
    };
    
    self.errConn = function (XMLHttpRequest, textStatus, errorThrown) {
        console.log(XMLHttpRequest.status);
        console.log(XMLHttpRequest.readyState);
        console.log(XMLHttpRequest.statusText);
        console.log("textStatus=" + textStatus);
    };
    
}





