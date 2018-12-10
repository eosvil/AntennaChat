(function ($) {
    $.fn.extend(
        {
            contextMenu: function (trueid, id, arr) {
                // Generate menu
                var $menu = $('<menu id="' + trueid + '" class="cm"></menu>');
                $.each(arr, function (i, ele) {
                    if (ele.text === "delimiter") {
                        $menu.append('<li class="delimiter"></li>');
                    } else {
                        var $li = $('<li value=' + arr[i].method + ' id=' + i + '>' + ele.text + '</li>');
                        $menu.append($li);
                    }
                });
                $menu.unbind("click contextmenu mousedown").bind("click contextmenu mousedown", function (e) {
                    var menuTarget = e.currentTarget;
                    var menuId = menuTarget.id;
                    var ss = e.target;
                    var msgId = document.getElementById(menuId).getAttribute("recallid");
                    var method = document.getElementById(menuId).getAttribute("selfmethods");
                    var imgMsgId = document.getElementById(menuId).getAttribute("imgMessageId");
                    switch (method) {
                        case "group":
                            switch (ss.value) {
                                case 1:
                                    callbackUserId.executeMethod("1", menuId);
                                    menuId = "";
                                    break;
                                case 2:
                                    callbackUserId.executeMethod("2", menuId);
                                    menuId = "";
                                    break;
                            }
                        case "one":
                            var iamgeurl = document.getElementById(menuId).src;
                          
                            switch (ss.value) {
                                case 1:
                                    callbackObj.imageMenuMethod("1", iamgeurl);
                                    break;
                                case 2:
                                    callbackObj.imageMenuMethod("2", iamgeurl);
                                    break;
                                case 3:
                                    callbackObj.imageMenuMethod("3", imgMsgId);
                                    break;
                                    //语音撤销
                                case 4:
                                    callbackObj.imageMenuMethod("3", menuId.substring(1, menuId.length));
                                    break;
                            }
                            break;
                        case "copy":
                            try {
                                //判断页面有没有选中内容
                                var divcontent = "";
                                if (window.getSelection()) {
                                    divcontent = window.getSelection().toString();
                                    if (divcontent == "") {
                                        divcontent = document.getElementById(menuId).innerText;
                                    }
                                }
                                switch (ss.value) {
                                    case 1:
                                        callbackObj.copydivContent("1", divcontent);
                                        break;
                                    case 2:
                                        callbackObj.copydivContent("2", msgId);
                                        break;
                                }
                            } catch (e) {
                                alert(e);
                            }
                            break;
                    }
                    $menu.remove();
                    return false;
                });

                $('body').unbind("contextmenu").bind("contextmenu").append($menu);
                this.unbind('contextmenu').bind('contextmenu', function (e) {
                    $menu.css({
                        left: ((e.pageX - $(window).scrollLeft()) + $menu.outerWidth(true) < $(window).width()) ? e.pageX : e.pageX - $menu.outerWidth(true),
                        top: ((e.pageY - $(window).scrollTop()) + $menu.outerHeight(true) < $(window).height()) ? e.pageY : e.pageY - $menu.outerHeight(true)
                    });
                    $menu.show();
                    hideMenu($menu);
                    return true;  // disable default context menu
                });
                function hideMenu($ele) {
                    $(document).unbind('mousedown').bind('mousedown', function () {
                        $menu.remove();
                        $(document).unbind('mousedown').bind('mousedown');
                    });
                }
                return this;
            }
        });
})(jQuery);