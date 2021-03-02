mergeInto(LibraryManager.library, {

  GetCurrentDevice: function () {
	return UnityLoader.SystemInfo.mobile ? 1 : 3;
  },
});