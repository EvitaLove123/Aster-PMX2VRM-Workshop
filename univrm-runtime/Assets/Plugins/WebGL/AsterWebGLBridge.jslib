mergeInto(LibraryManager.library, {
  AsterPostMessage: function (jsonPtr) {
    var json = UTF8ToString(jsonPtr);
    try {
      var payload = JSON.parse(json);
      window.parent.postMessage(payload, "*");
    } catch (error) {
      window.parent.postMessage({
        source: "aster-univrm",
        state: "error",
        message: "Could not parse Unity runtime response."
      }, "*");
    }
  },
  AsterDownloadFile: function (fileNamePtr, bytesPtr, length) {
    try {
      var fileName = UTF8ToString(fileNamePtr);
      var bytes = HEAPU8.slice(bytesPtr, bytesPtr + length);
      var blob = new Blob([bytes], { type: "application/octet-stream" });
      var url = URL.createObjectURL(blob);
      var anchor = document.createElement("a");
      anchor.href = url;
      anchor.download = fileName || "AsterAvatar.vrm";
      anchor.style.display = "none";
      document.body.appendChild(anchor);
      anchor.click();
      anchor.remove();
      window.setTimeout(function () { URL.revokeObjectURL(url); }, 30000);
    } catch (error) {
      window.parent.postMessage({
        source: "aster-univrm",
        state: "error",
        message: "Browser download failed: " + String(error)
      }, "*");
    }
  }
});
