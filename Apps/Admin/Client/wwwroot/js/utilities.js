function initializeInactivityTimer(dotnetHelper) {
    var timer;
    document.onmousemove = resetTimer;
    document.onkeypress = resetTimer;

    function resetTimer() {
        clearTimeout(timer);
        timer = setTimeout(openInactivityDialog, 300000);
    }

    function openInactivityDialog() {
        dotnetHelper.invokeMethodAsync("OpenInactivityDialog");
    }
}
