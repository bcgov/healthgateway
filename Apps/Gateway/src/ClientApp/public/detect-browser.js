function detectBrowser() {
    var body = document.getElementsByTagName("BODY")[0];
    var unsupportedBrowser = document.getElementById("unsupported-browser");
    if (Modernizr.es6number) {
        body.removeChild(unsupportedBrowser);
    } else {
        body.removeChild(document.getElementById("app-root"));
        var unsupportedBrowserMsg =
            "Health Gateway is not compatible with this browser. Please use a modern browser.";
        unsupportedBrowser.innerHTML = unsupportedBrowserMsg;
    }
}

window.onload = function () {
    detectBrowser();
};
