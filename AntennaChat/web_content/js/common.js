/**
 * 移除div
 * @param {any} mId 消息id
 */
function removeDiv(mId) {
    try {
        $("#" + mId).remove();
        $("#" + mId).remove();
    }
    catch (ex) {

    }
}

function scrollToend(e) {
    window.scrollTo(0, document.body.scrollHeight);
}
/*
function menuLr(event) {
    var leftOrRightButton = event.button;

    switch (leftOrRightButton) {
        case 0:
            alert("您点击了鼠标左键");
            break;
        case 1:
            alert("您点击了鼠标中键");
            break;
        case 2:
            var ss = window.getSelection();
            if (ss.toString().length == 0) {
                document.getElementById("M1510537840163700942").getSelection();
            }
            break;
    }
}*/
function getScroolPosition() {
    var bool = false;
    var scrollHeight = document.documentElement.scrollHeight;
    //alert("scrollHeight:" + scrollHeight);
    var clientHeight = document.documentElement.clientHeight;
    //alert("clientHeight:" + clientHeight);
    var scrollTop = document.body.scrollTop;
    //alert("scrollTop:" + scrollTop);
    var add = clientHeight + scrollTop;
    //alert("clientHeight + scrollTop:" + scrollTop);
    if (scrollHeight < clientHeight) {
        return true;
    }
    else {
        if (scrollHeight - add < 5) {
            return true;
        }
        else {
            return false;
        }
    }
}
function isExistId(Mid) {
    try {
        var boo = false;
        if (document.getElementById(Mid)) {
            return true;
        }
        else {
            return false;
        }
    }
    catch (ex) {
        return false;
    }
}