var connId;
var hub = $.connection.processingHub;
$.connection.hub.start().done(function () {
    connId = $.connection.hub.id;
});

hub.client.updateProgress = function (percentage, message) {
    if (percentage < 0) {
        alert(message);
        document.getElementById("ProgressPanel").style.height = "0%";
    }
    else {
        document.getElementById("progressBar1").style.width = percentage.toFixed(2) + "%";
    }
};

var imgData = "";
var rlen = 0;
hub.client.receiveData = function (data, len, tot) {
    rlen += len;
    imgData += data;

    if (imgData.length === tot) {
        document.getElementById("InputPanel").className = "hidden";
        document.getElementById("OutputPanel").className = "";
        document.getElementById("OutputImage").src = "data:image/jpeg;base64," + imgData;
        document.getElementById("ProgressPanel").style.height = "0%";
    }
};
