function isNumber(evt) {
    var theEvent = evt || window.event;
    var key = theEvent.keyCode || theEvent.which;
    key = String.fromCharCode(key);
    if (key.length == 0) return;
    var regex = /^[0-9.]+$/;
    if (!regex.test(key)) {
        theEvent.returnValue = false;
        if (theEvent.preventDefault) {
            theEvent.preventDefault();
            return false;
        }
    }
    return true;
}
function setCookie(cname, cvalue, exdays) {
    var d = new Date();
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    var expires = "expires=" + d.toUTCString();
    document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
}
function getCookie(cname) {
    var name = cname + "=";
    var decodedCookie = decodeURIComponent(document.cookie);
    var ca = decodedCookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}
function validationPhonNumber(element, evt, enablemunis) {
    var theEvent = evt || window.event;
    var key = theEvent.keyCode || theEvent.which;
    key = String.fromCharCode(key);
    if (key == '+' && element.value.length == 0) {
        return true;
    }
    isNumber(evt);
}

function FormaittingNumber(element,bool) {
    
    element.value = deleteCommaFromNumber(element.value);

    element.value = numberWithCommas(element.value, bool);
    //element.value = text;
}
function deleteCommaFromNumber(text) {
    if (text === null || text === '')
        return 0;
    text = text.replace(/,/g, '');
    return text;
}
function validation(element, evt, bool) {
    var text = element.value;
    var e = evt || window.event;
    var charCode = e.which || e.keyCode;
    if (bool != undefined && bool == true) {
        if (charCode != 46) {
            if (text[1] == '0' && text[0] == '-' && !text.includes('.')) {
                if (charCode == 48)
                    evt.preventDefault();
                else {
                    if (charCode == 45) {
                        element.value = element.value.replace('-', '');
                        evt.preventDefault();
                        return;
                    }
                    if (!isNaN(String.fromCharCode(charCode))) {
                        element.value = element.value.replace('0', '');
                    }
                    else {
                        console.log('dd');
                    }
                }
            }
        }
        if (charCode == 45) {
            evt.preventDefault();
            if (text.includes('-')) {
                element.value = element.value.replace('-', '');
            } else {
                element.value = '-' + element.value;
            }
            return false;
        }
    }
    if (isNumber(evt)) {
        if (charCode == 46) {
            if (text.includes('.')) {
                e.preventDefault();
                return;
            }
            if (text.length == 0) {
                // e.preventDefault();
                element.value = '0';
                return;
            }
        }
        if (text[0] == 0 && charCode == 48 && text.length == 1) {
            e.preventDefault();
            return;
        }
        if (text[0] == 0 && text.length == 1 && charCode != 46) {
            element.value = "";
        }
    }
}


function FlipYesOrNoToBoolean(answer) {
    if (answer === "نعم")
        return true;
    return false;

}
function FlipBoolToYesOrNo(status) {
    if (status)
        return "نعم";
    return "لا";
}
function PriventEmptytext(element, evt, bool) {
    var text = element.value;
    var e = evt || window.event;
    var charCode = e.which || e.keyCode;
    if (charCode == 8) {
        if (bool != undefined && bool) {
            if (text[0] == '-') {
                if (text[1] == '0') {
                    console.log(text.length);
                    if (text.length <= 2) {
                        e.preventDefault();
                        element.value = '0';
                    }
                    return;
                } else {
                    if (text.length == 2) {
                        element.value = '0-';
                    }
                }
            }
        }
        if (text.length <= 1) {
            evt.preventDefault();
            element.value = 0;
        }
    }
}
function getRowFormAnyElement(elemet) {
    if (elemet == undefined || elemet == null)
        return undefined;
    if (elemet.tagName == "TR") {
        return elemet;
    }
    return getRowFormAnyElement(elemet.parentElement);
}

function GetDiveFromAbyElement(element) {
    if (element == undefined || element == null) {
        return undefined;
    }
    if (element.tagName == "DIV") {
        return element;
    }
    return GetDiveFromAbyElement(element.parentElement);
}
function GitDivWithRowClassRow(element) {
    if (element.classList.contains('row') && element.tagName == 'DIV') {
        return element;
    }
    return GitDivWithRowClassRow(element.parentElement);
}
Date.prototype.toDateInputValue = (function() {
    var local = new Date(this);
    local.setMinutes(this.getMinutes() - this.getTimezoneOffset());
    return local.toJSON().slice(0,10);
});