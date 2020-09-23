function detectBrowser() {
    var body = document.getElementsByTagName("BODY")[0];
    var unsupportedBrowser = document.getElementById("unsupported-browser");
    console.log("Modernizr.es6number: " + Modernizr.es6number);
    if (Modernizr.es6number) {
        body.removeChild(unsupportedBrowser);
    } else {
        body.removeChild(document.getElementById("app-root"));
        var unsupportedBrowserMsg =
            "Health Gateway is not compatible with this browser. Please use another browser.";
        unsupportedBrowser.innerHTML = unsupportedBrowserMsg;
    }
}
window.onload = function () {
    detectBrowser();
};
