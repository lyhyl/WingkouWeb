var uploadUrl = "";
var upcID;
var curProgess = 0;
var tmpProgess = 0;
var tarProgess = 0;

function ShowImagePreview(input) {
    var imgp = $("#ImagePreviewer");
    if (input.value === "") {
        imgp.prop("src", "Content/ui/selectImage.png");
        return;
    }
    if (input.files && input.files[0]) {
        var fail = false;
        if (!ValidateSingleInput(input)) {
            MakeAlert("你上传的不是自拍吧?");
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
            imgp.prop("src", "../Content/ui/selectImage.png");
            input.value = "";
            return;
        }
        imgp.prop("src", "Content/ui/uploading.gif");
        var reader = new FileReader();
        reader.onload = function (e) {
            var img = new Image();
            img.src = e.target.result;
            imgp.prop("src", e.target.result).fadeIn("fast");
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

function upload() {
    var maxSize = 100;
    var file = document.getElementById("ImageSelector").files[0];
    if (!file) {
        MakeAlert("咦自拍呢?");
        return;
    }
    console.log(file.size);
    if (file.size > maxSize * 1024 * 1024) {
        console.log(">" + maxSize + "M");
        return;
    }

    // show loading
    document.getElementById("UploadMask").style.height = "100%";
    $("UploadMask").fadeIn("fast");

    var reader = new FileReader();
    upcID = setInterval("updateProgessCircle()", 30);
    curProgess = 0;
    tmpProgess = 0;
    tarProgess = 0;
    reader.onload = function (e) {
        $.ajax({
            async: true,
            type: "POST",
            url: uploadUrl + "?file=" + file.name,
            contentType: "application/octect-stream",
            processData: false,
            data: e.target.result,
            xhr: function () {
                var xhr = $.ajaxSettings.xhr();
                xhr.upload.onprogress = function (evt) {
                    tarProgess = evt.loaded / file.size;
                };
                return xhr;
            }
        });
    };
    reader.readAsArrayBuffer(file);
}

function updateProgessCircle() {
    if (curProgess === 1) {
        clearInterval(upcID);
    }

    if (tmpProgess === curProgess) {
        tmpProgess = tarProgess;
    }

    var toProgess = (tmpProgess - curProgess) * 0.05 + curProgess;
    curProgess = tmpProgess - toProgess < 0.01 ? tmpProgess : toProgess;

    var ps = curProgess * 100;
    document.getElementById("progess").innerHTML = ps.toFixed(2) + "%";
    document.getElementById("UploadMask").style.height = (100 - ps).toFixed(2) + "%";

    //var canvas = document.getElementById("progessCircle"),
    //    context = canvas.getContext("2d"),
    //    cenx = canvas.width / 2,
    //    ceny = canvas.height / 2,
    //    begr = -Math.PI / 2;

    //var ang = Math.PI * 2 * curProgess;

    //context.clearRect(0, 0, canvas.width, canvas.height);
    //context.strokeStyle = "#888";
    //context.lineWidth = 5;
    //context.beginPath();
    //context.arc(cenx, ceny, Math.min(cenx, ceny), begr, begr + ang, false);
    //context.stroke();
    //context.closePath();
}