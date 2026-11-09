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
  }
});
