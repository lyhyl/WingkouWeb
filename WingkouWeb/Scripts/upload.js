var uploadUrl = "/cglab/pimg";
var method = "ImageProcessingRaw";
var getParams;

function getRootUrl() {
    return window.location.origin ? window.location.origin + '/' : window.location.protocol + '/' + window.location.host + '/';
}

function ShowImagePreview(input) {
    var img = $("#ImagePreviewer");
    if (input.value === "") {
        img.prop("src", getRootUrl() + "Content/ui/selectImage.png");
        return;
    }
    if (input.files && input.files[0]) {
        var fail = false;
        if (!ValidateSingleInput(input)) {
            MakeAlert("你上传的不是图片吧?");
            fail = true;
        }
        if (!fail && input.files[0].size > 8 * 1024 * 1024) {
            MakeAlert("图片太大辣,换一张吧(<8M)");
            fail = true;
        }
        if (!fail && input.files[0].size < 1 * 1024) {
            MakeAlert("图片这么小,你确定这不是表情包?(>1K)");
            fail = true;
        }
        if (fail) {
            img.prop("src", getRootUrl() + "Content/ui/selectImage.png");
            input.value = "";
            return;
        }
        img.prop("src", getRootUrl() + "Content/ui/uploading.gif");
        var reader = new FileReader();
        reader.onload = function (e) {
            var im = new Image();
            im.src = e.target.result;
            img.prop("src", e.target.result).fadeIn("fast");
        };
        reader.readAsDataURL(input.files[0]);
    }
}
function MakeAlert(msg) {
    $('#AlertMessagePlaceholder').html('<p>' + msg + '</p>');
    document.getElementById("AlertPanel").style.height = "100%";
}

function DismissAlert() {
    document.getElementById("AlertPanel").style.height = "0%";
}

function ValidateSingleInput(input) {
    var ext = [".jpg", ".jpeg", ".png"];
    var fn = input.value;
    if (fn.length > 0) {
        for (var i = 0; i < ext.length; i++) {
            var fnext = fn.substr(fn.length - ext[i].length, ext[i].length);
            if (fnext.toLowerCase() === ext[i]) {
                return true;
            }
        }
    }
    return false;
}

function Upload() {
    var file = document.getElementById("ImageSelector").files[0];
    if (!file) {
        MakeAlert("咦图片呢?");
        return;
    }

    document.getElementById("ProgressPanel").style.height = "100%";

    var reader = new FileReader();
    reader.onload = function (e) {
        $.ajax({
            async: true,
            type: "POST",
            url: uploadUrl + "?connId=" + connId + "&method=" + method + "&parameters" + getParams(),
            contentType: "application/octect-stream",
            processData: false,
            data: e.target.result,
            xhr: function () {
                var xhr = $.ajaxSettings.xhr();
                xhr.upload.onprogress = function (evt) {
                    UpdateProgress(evt.loaded / file.size);
                };
                return xhr;
            }
        }).done(function (data, textStatus, jqXHR) {
        }).fail(function (jqXHR, textStatus, errorThrown) {
            alert(errorThrown);
        }).always(function (v, textStatus, v2) {
        });
    };
    reader.readAsArrayBuffer(file);
}

function UpdateProgress(tarProgress) {
    var ps = tarProgress * 100;
    document.getElementById("progressBar0").style.width = ps.toFixed(2) + "%";
}
