window.setFocus = (element) => {
    if (element) {
        element.focus();
    }
};
window.setFocusById = function (id) {
    const el = document.getElementById(id);
    if (el) el.focus();
};
window.clearInputById = function (id) {
    const el = document.getElementById(id);
    if (el) el.value = "";
}
window.preventBackNavigation = function () {
    history.pushState(null, "", location.href);
    window.onpopstate = function () {
        history.go(1);
    };
};
var isLoading = false
function toggleLoading(updateValue) {
    isLoading = updateValue;
    if (isLoading) run_waitMe('bounce');
    else run_waitMe('hide');
}

function run_waitMe(effect) {
    if (effect == 'hide') {
        $('#app').waitMe('hide');
        return;
    }
    $('#app').waitMe({
        effect: effect,
        text: 'Loading',
        bg: 'rgba(255,255,255,0.7)',
        color: '#000',
        maxSize: '',
        waitTime: -1,
        source: '',
        textPos: 'vertical',
        fontSize: '',
        onClose: function () { }
    });
}
function selectAllText(element) {
    element.select();
}
function showToast(status, title, text, speed = 200, autotimeout = 1500) {
    new Notify({
        status: status,
        title: title,
        text: text,
        effect: 'fade',
        speed: speed,
        customClass: null,
        customIcon: null,
        showIcon: true,
        showCloseButton: true,
        autoclose: true,
        autotimeout: autotimeout,
        gap: 20,
        distance: 20,
        type: 'outline',
        position: 'right top'
    })
}