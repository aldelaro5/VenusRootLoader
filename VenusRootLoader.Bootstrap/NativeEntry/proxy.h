#pragma once
#include <windows.h>

static struct winhttpDll {
	FARPROC originalDllCanUnloadNow;
	FARPROC originalDllGetClassObject;
	FARPROC originalPrivate1;
	FARPROC originalSvchostPushServiceGlobals;
	FARPROC originalWinHttpAddRequestHeaders;
	FARPROC originalWinHttpAddRequestHeadersEx;
	FARPROC originalWinHttpAutoProxySvcMain;
	FARPROC originalWinHttpCheckPlatform;
	FARPROC originalWinHttpCloseHandle;
	FARPROC originalWinHttpConnect;
	FARPROC originalWinHttpConnectionDeletePolicyEntries;
	FARPROC originalWinHttpConnectionDeleteProxyInfo;
	FARPROC originalWinHttpConnectionFreeNameList;
	FARPROC originalWinHttpConnectionFreeProxyInfo;
	FARPROC originalWinHttpConnectionFreeProxyList;
	FARPROC originalWinHttpConnectionGetNameList;
	FARPROC originalWinHttpConnectionGetProxyInfo;
	FARPROC originalWinHttpConnectionGetProxyList;
	FARPROC originalWinHttpConnectionOnlyConvert;
	FARPROC originalWinHttpConnectionOnlyReceive;
	FARPROC originalWinHttpConnectionOnlySend;
	FARPROC originalWinHttpConnectionSetPolicyEntries;
	FARPROC originalWinHttpConnectionSetProxyInfo;
	FARPROC originalWinHttpConnectionUpdateIfIndexTable;
	FARPROC originalWinHttpCrackUrl;
	FARPROC originalWinHttpCreateProxyList;
	FARPROC originalWinHttpCreateProxyManager;
	FARPROC originalWinHttpCreateProxyResolver;
	FARPROC originalWinHttpCreateProxyResult;
	FARPROC originalWinHttpCreateUiCompatibleProxyString;
	FARPROC originalWinHttpCreateUrl;
	FARPROC originalWinHttpDetectAutoProxyConfigUrl;
	FARPROC originalWinHttpFreeProxyResult;
	FARPROC originalWinHttpFreeProxyResultEx;
	FARPROC originalWinHttpFreeProxySettings;
	FARPROC originalWinHttpFreeProxySettingsEx;
	FARPROC originalWinHttpFreeQueryConnectionGroupResult;
	FARPROC originalWinHttpGetDefaultProxyConfiguration;
	FARPROC originalWinHttpGetIEProxyConfigForCurrentUser;
	FARPROC originalWinHttpGetProxyForUrl;
	FARPROC originalWinHttpGetProxyForUrlEx;
	FARPROC originalWinHttpGetProxyForUrlEx2;
	FARPROC originalWinHttpGetProxyForUrlHvsi;
	FARPROC originalWinHttpGetProxyResult;
	FARPROC originalWinHttpGetProxyResultEx;
	FARPROC originalWinHttpGetProxySettingsEx;
	FARPROC originalWinHttpGetProxySettingsResultEx;
	FARPROC originalWinHttpGetProxySettingsVersion;
	FARPROC originalWinHttpGetTunnelSocket;
	FARPROC originalWinHttpOpen;
	FARPROC originalWinHttpOpenRequest;
	FARPROC originalWinHttpPacJsWorkerMain;
	FARPROC originalWinHttpProbeConnectivity;
	FARPROC originalWinHttpProtocolCompleteUpgrade;
	FARPROC originalWinHttpProtocolReceive;
	FARPROC originalWinHttpProtocolSend;
	FARPROC originalWinHttpQueryAuthSchemes;
	FARPROC originalWinHttpQueryConnectionGroup;
	FARPROC originalWinHttpQueryDataAvailable;
	FARPROC originalWinHttpQueryHeaders;
	FARPROC originalWinHttpQueryHeadersEx;
	FARPROC originalWinHttpQueryOption;
	FARPROC originalWinHttpReadData;
	FARPROC originalWinHttpReadDataEx;
	FARPROC originalWinHttpReadProxySettings;
	FARPROC originalWinHttpReadProxySettingsHvsi;
	FARPROC originalWinHttpReceiveResponse;
	FARPROC originalWinHttpRefreshProxySettings;
	FARPROC originalWinHttpRegisterProxyChangeNotification;
	FARPROC originalWinHttpResetAutoProxy;
	FARPROC originalWinHttpResolverGetProxyForUrl;
	FARPROC originalWinHttpSaveProxyCredentials;
	FARPROC originalWinHttpSendRequest;
	FARPROC originalWinHttpSetCredentials;
	FARPROC originalWinHttpSetDefaultProxyConfiguration;
	FARPROC originalWinHttpSetOption;
	FARPROC originalWinHttpSetProxySettingsPerUser;
	FARPROC originalWinHttpSetSecureLegacyServersAppCompat;
	FARPROC originalWinHttpSetStatusCallback;
	FARPROC originalWinHttpSetTimeouts;
	FARPROC originalWinHttpTimeFromSystemTime;
	FARPROC originalWinHttpTimeToSystemTime;
	FARPROC originalWinHttpUnregisterProxyChangeNotification;
	FARPROC originalWinHttpWebSocketClose;
	FARPROC originalWinHttpWebSocketCompleteUpgrade;
	FARPROC originalWinHttpWebSocketQueryCloseStatus;
	FARPROC originalWinHttpWebSocketReceive;
	FARPROC originalWinHttpWebSocketSend;
	FARPROC originalWinHttpWebSocketShutdown;
	FARPROC originalWinHttpWriteData;
	FARPROC originalWinHttpWriteProxySettings;
} winhttp;

void ImplDllCanUnloadNow() { _asm jmp[winhttp.originalDllCanUnloadNow] }
void ImplDllGetClassObject() { _asm jmp[winhttp.originalDllGetClassObject] }
void ImplPrivate1() { _asm jmp[winhttp.originalPrivate1] }
void ImplSvchostPushServiceGlobals() { _asm jmp[winhttp.originalSvchostPushServiceGlobals] }
void ImplWinHttpAddRequestHeaders() { _asm jmp[winhttp.originalWinHttpAddRequestHeaders] }
void ImplWinHttpAddRequestHeadersEx() { _asm jmp[winhttp.originalWinHttpAddRequestHeadersEx] }
void ImplWinHttpAutoProxySvcMain() { _asm jmp[winhttp.originalWinHttpAutoProxySvcMain] }
void ImplWinHttpCheckPlatform() { _asm jmp[winhttp.originalWinHttpCheckPlatform] }
void ImplWinHttpCloseHandle() { _asm jmp[winhttp.originalWinHttpCloseHandle] }
void ImplWinHttpConnect() { _asm jmp[winhttp.originalWinHttpConnect] }
void ImplWinHttpConnectionDeletePolicyEntries() { _asm jmp[winhttp.originalWinHttpConnectionDeletePolicyEntries] }
void ImplWinHttpConnectionDeleteProxyInfo() { _asm jmp[winhttp.originalWinHttpConnectionDeleteProxyInfo] }
void ImplWinHttpConnectionFreeNameList() { _asm jmp[winhttp.originalWinHttpConnectionFreeNameList] }
void ImplWinHttpConnectionFreeProxyInfo() { _asm jmp[winhttp.originalWinHttpConnectionFreeProxyInfo] }
void ImplWinHttpConnectionFreeProxyList() { _asm jmp[winhttp.originalWinHttpConnectionFreeProxyList] }
void ImplWinHttpConnectionGetNameList() { _asm jmp[winhttp.originalWinHttpConnectionGetNameList] }
void ImplWinHttpConnectionGetProxyInfo() { _asm jmp[winhttp.originalWinHttpConnectionGetProxyInfo] }
void ImplWinHttpConnectionGetProxyList() { _asm jmp[winhttp.originalWinHttpConnectionGetProxyList] }
void ImplWinHttpConnectionOnlyConvert() { _asm jmp[winhttp.originalWinHttpConnectionOnlyConvert] }
void ImplWinHttpConnectionOnlyReceive() { _asm jmp[winhttp.originalWinHttpConnectionOnlyReceive] }
void ImplWinHttpConnectionOnlySend() { _asm jmp[winhttp.originalWinHttpConnectionOnlySend] }
void ImplWinHttpConnectionSetPolicyEntries() { _asm jmp[winhttp.originalWinHttpConnectionSetPolicyEntries] }
void ImplWinHttpConnectionSetProxyInfo() { _asm jmp[winhttp.originalWinHttpConnectionSetProxyInfo] }
void ImplWinHttpConnectionUpdateIfIndexTable() { _asm jmp[winhttp.originalWinHttpConnectionUpdateIfIndexTable] }
void ImplWinHttpCrackUrl() { _asm jmp[winhttp.originalWinHttpCrackUrl] }
void ImplWinHttpCreateProxyList() { _asm jmp[winhttp.originalWinHttpCreateProxyList] }
void ImplWinHttpCreateProxyManager() { _asm jmp[winhttp.originalWinHttpCreateProxyManager] }
void ImplWinHttpCreateProxyResolver() { _asm jmp[winhttp.originalWinHttpCreateProxyResolver] }
void ImplWinHttpCreateProxyResult() { _asm jmp[winhttp.originalWinHttpCreateProxyResult] }
void ImplWinHttpCreateUiCompatibleProxyString() { _asm jmp[winhttp.originalWinHttpCreateUiCompatibleProxyString] }
void ImplWinHttpCreateUrl() { _asm jmp[winhttp.originalWinHttpCreateUrl] }
void ImplWinHttpDetectAutoProxyConfigUrl() { _asm jmp[winhttp.originalWinHttpDetectAutoProxyConfigUrl] }
void ImplWinHttpFreeProxyResult() { _asm jmp[winhttp.originalWinHttpFreeProxyResult] }
void ImplWinHttpFreeProxyResultEx() { _asm jmp[winhttp.originalWinHttpFreeProxyResultEx] }
void ImplWinHttpFreeProxySettings() { _asm jmp[winhttp.originalWinHttpFreeProxySettings] }
void ImplWinHttpFreeProxySettingsEx() { _asm jmp[winhttp.originalWinHttpFreeProxySettingsEx] }
void ImplWinHttpFreeQueryConnectionGroupResult() { _asm jmp[winhttp.originalWinHttpFreeQueryConnectionGroupResult] }
void ImplWinHttpGetDefaultProxyConfiguration() { _asm jmp[winhttp.originalWinHttpGetDefaultProxyConfiguration] }
void ImplWinHttpGetIEProxyConfigForCurrentUser() { _asm jmp[winhttp.originalWinHttpGetIEProxyConfigForCurrentUser] }
void ImplWinHttpGetProxyForUrl() { _asm jmp[winhttp.originalWinHttpGetProxyForUrl] }
void ImplWinHttpGetProxyForUrlEx() { _asm jmp[winhttp.originalWinHttpGetProxyForUrlEx] }
void ImplWinHttpGetProxyForUrlEx2() { _asm jmp[winhttp.originalWinHttpGetProxyForUrlEx2] }
void ImplWinHttpGetProxyForUrlHvsi() { _asm jmp[winhttp.originalWinHttpGetProxyForUrlHvsi] }
void ImplWinHttpGetProxyResult() { _asm jmp[winhttp.originalWinHttpGetProxyResult] }
void ImplWinHttpGetProxyResultEx() { _asm jmp[winhttp.originalWinHttpGetProxyResultEx] }
void ImplWinHttpGetProxySettingsEx() { _asm jmp[winhttp.originalWinHttpGetProxySettingsEx] }
void ImplWinHttpGetProxySettingsResultEx() { _asm jmp[winhttp.originalWinHttpGetProxySettingsResultEx] }
void ImplWinHttpGetProxySettingsVersion() { _asm jmp[winhttp.originalWinHttpGetProxySettingsVersion] }
void ImplWinHttpGetTunnelSocket() { _asm jmp[winhttp.originalWinHttpGetTunnelSocket] }
void ImplWinHttpOpen() { _asm jmp[winhttp.originalWinHttpOpen] }
void ImplWinHttpOpenRequest() { _asm jmp[winhttp.originalWinHttpOpenRequest] }
void ImplWinHttpPacJsWorkerMain() { _asm jmp[winhttp.originalWinHttpPacJsWorkerMain] }
void ImplWinHttpProbeConnectivity() { _asm jmp[winhttp.originalWinHttpProbeConnectivity] }
void ImplWinHttpProtocolCompleteUpgrade() { _asm jmp[winhttp.originalWinHttpProtocolCompleteUpgrade] }
void ImplWinHttpProtocolReceive() { _asm jmp[winhttp.originalWinHttpProtocolReceive] }
void ImplWinHttpProtocolSend() { _asm jmp[winhttp.originalWinHttpProtocolSend] }
void ImplWinHttpQueryAuthSchemes() { _asm jmp[winhttp.originalWinHttpQueryAuthSchemes] }
void ImplWinHttpQueryConnectionGroup() { _asm jmp[winhttp.originalWinHttpQueryConnectionGroup] }
void ImplWinHttpQueryDataAvailable() { _asm jmp[winhttp.originalWinHttpQueryDataAvailable] }
void ImplWinHttpQueryHeaders() { _asm jmp[winhttp.originalWinHttpQueryHeaders] }
void ImplWinHttpQueryHeadersEx() { _asm jmp[winhttp.originalWinHttpQueryHeadersEx] }
void ImplWinHttpQueryOption() { _asm jmp[winhttp.originalWinHttpQueryOption] }
void ImplWinHttpReadData() { _asm jmp[winhttp.originalWinHttpReadData] }
void ImplWinHttpReadDataEx() { _asm jmp[winhttp.originalWinHttpReadDataEx] }
void ImplWinHttpReadProxySettings() { _asm jmp[winhttp.originalWinHttpReadProxySettings] }
void ImplWinHttpReadProxySettingsHvsi() { _asm jmp[winhttp.originalWinHttpReadProxySettingsHvsi] }
void ImplWinHttpReceiveResponse() { _asm jmp[winhttp.originalWinHttpReceiveResponse] }
void ImplWinHttpRefreshProxySettings() { _asm jmp[winhttp.originalWinHttpRefreshProxySettings] }
void ImplWinHttpRegisterProxyChangeNotification() { _asm jmp[winhttp.originalWinHttpRegisterProxyChangeNotification] }
void ImplWinHttpResetAutoProxy() { _asm jmp[winhttp.originalWinHttpResetAutoProxy] }
void ImplWinHttpResolverGetProxyForUrl() { _asm jmp[winhttp.originalWinHttpResolverGetProxyForUrl] }
void ImplWinHttpSaveProxyCredentials() { _asm jmp[winhttp.originalWinHttpSaveProxyCredentials] }
void ImplWinHttpSendRequest() { _asm jmp[winhttp.originalWinHttpSendRequest] }
void ImplWinHttpSetCredentials() { _asm jmp[winhttp.originalWinHttpSetCredentials] }
void ImplWinHttpSetDefaultProxyConfiguration() { _asm jmp[winhttp.originalWinHttpSetDefaultProxyConfiguration] }
void ImplWinHttpSetOption() { _asm jmp[winhttp.originalWinHttpSetOption] }
void ImplWinHttpSetProxySettingsPerUser() { _asm jmp[winhttp.originalWinHttpSetProxySettingsPerUser] }
void ImplWinHttpSetSecureLegacyServersAppCompat() { _asm jmp[winhttp.originalWinHttpSetSecureLegacyServersAppCompat] }
void ImplWinHttpSetStatusCallback() { _asm jmp[winhttp.originalWinHttpSetStatusCallback] }
void ImplWinHttpSetTimeouts() { _asm jmp[winhttp.originalWinHttpSetTimeouts] }
void ImplWinHttpTimeFromSystemTime() { _asm jmp[winhttp.originalWinHttpTimeFromSystemTime] }
void ImplWinHttpTimeToSystemTime() { _asm jmp[winhttp.originalWinHttpTimeToSystemTime] }
void ImplWinHttpUnregisterProxyChangeNotification() { _asm jmp[winhttp.originalWinHttpUnregisterProxyChangeNotification] }
void ImplWinHttpWebSocketClose() { _asm jmp[winhttp.originalWinHttpWebSocketClose] }
void ImplWinHttpWebSocketCompleteUpgrade() { _asm jmp[winhttp.originalWinHttpWebSocketCompleteUpgrade] }
void ImplWinHttpWebSocketQueryCloseStatus() { _asm jmp[winhttp.originalWinHttpWebSocketQueryCloseStatus] }
void ImplWinHttpWebSocketReceive() { _asm jmp[winhttp.originalWinHttpWebSocketReceive] }
void ImplWinHttpWebSocketSend() { _asm jmp[winhttp.originalWinHttpWebSocketSend] }
void ImplWinHttpWebSocketShutdown() { _asm jmp[winhttp.originalWinHttpWebSocketShutdown] }
void ImplWinHttpWriteData() { _asm jmp[winhttp.originalWinHttpWriteData] }
void ImplWinHttpWriteProxySettings() { _asm jmp[winhttp.originalWinHttpWriteProxySettings] }

void SetupProxy(HMODULE originalDll)
{
	winhttp.originalDllCanUnloadNow = GetProcAddress(originalDll, "DllCanUnloadNow");
	winhttp.originalDllGetClassObject = GetProcAddress(originalDll, "DllGetClassObject");
	winhttp.originalPrivate1 = GetProcAddress(originalDll, "Private1");
	winhttp.originalSvchostPushServiceGlobals = GetProcAddress(originalDll, "SvchostPushServiceGlobals");
	winhttp.originalWinHttpAddRequestHeaders = GetProcAddress(originalDll, "WinHttpAddRequestHeaders");
	winhttp.originalWinHttpAddRequestHeadersEx = GetProcAddress(originalDll, "WinHttpAddRequestHeadersEx");
	winhttp.originalWinHttpAutoProxySvcMain = GetProcAddress(originalDll, "WinHttpAutoProxySvcMain");
	winhttp.originalWinHttpCheckPlatform = GetProcAddress(originalDll, "WinHttpCheckPlatform");
	winhttp.originalWinHttpCloseHandle = GetProcAddress(originalDll, "WinHttpCloseHandle");
	winhttp.originalWinHttpConnect = GetProcAddress(originalDll, "WinHttpConnect");
	winhttp.originalWinHttpConnectionDeletePolicyEntries = GetProcAddress(originalDll, "WinHttpConnectionDeletePolicyEntries");
	winhttp.originalWinHttpConnectionDeleteProxyInfo = GetProcAddress(originalDll, "WinHttpConnectionDeleteProxyInfo");
	winhttp.originalWinHttpConnectionFreeNameList = GetProcAddress(originalDll, "WinHttpConnectionFreeNameList");
	winhttp.originalWinHttpConnectionFreeProxyInfo = GetProcAddress(originalDll, "WinHttpConnectionFreeProxyInfo");
	winhttp.originalWinHttpConnectionFreeProxyList = GetProcAddress(originalDll, "WinHttpConnectionFreeProxyList");
	winhttp.originalWinHttpConnectionGetNameList = GetProcAddress(originalDll, "WinHttpConnectionGetNameList");
	winhttp.originalWinHttpConnectionGetProxyInfo = GetProcAddress(originalDll, "WinHttpConnectionGetProxyInfo");
	winhttp.originalWinHttpConnectionGetProxyList = GetProcAddress(originalDll, "WinHttpConnectionGetProxyList");
	winhttp.originalWinHttpConnectionOnlyConvert = GetProcAddress(originalDll, "WinHttpConnectionOnlyConvert");
	winhttp.originalWinHttpConnectionOnlyReceive = GetProcAddress(originalDll, "WinHttpConnectionOnlyReceive");
	winhttp.originalWinHttpConnectionOnlySend = GetProcAddress(originalDll, "WinHttpConnectionOnlySend");
	winhttp.originalWinHttpConnectionSetPolicyEntries = GetProcAddress(originalDll, "WinHttpConnectionSetPolicyEntries");
	winhttp.originalWinHttpConnectionSetProxyInfo = GetProcAddress(originalDll, "WinHttpConnectionSetProxyInfo");
	winhttp.originalWinHttpConnectionUpdateIfIndexTable = GetProcAddress(originalDll, "WinHttpConnectionUpdateIfIndexTable");
	winhttp.originalWinHttpCrackUrl = GetProcAddress(originalDll, "WinHttpCrackUrl");
	winhttp.originalWinHttpCreateProxyList = GetProcAddress(originalDll, "WinHttpCreateProxyList");
	winhttp.originalWinHttpCreateProxyManager = GetProcAddress(originalDll, "WinHttpCreateProxyManager");
	winhttp.originalWinHttpCreateProxyResolver = GetProcAddress(originalDll, "WinHttpCreateProxyResolver");
	winhttp.originalWinHttpCreateProxyResult = GetProcAddress(originalDll, "WinHttpCreateProxyResult");
	winhttp.originalWinHttpCreateUiCompatibleProxyString = GetProcAddress(originalDll, "WinHttpCreateUiCompatibleProxyString");
	winhttp.originalWinHttpCreateUrl = GetProcAddress(originalDll, "WinHttpCreateUrl");
	winhttp.originalWinHttpDetectAutoProxyConfigUrl = GetProcAddress(originalDll, "WinHttpDetectAutoProxyConfigUrl");
	winhttp.originalWinHttpFreeProxyResult = GetProcAddress(originalDll, "WinHttpFreeProxyResult");
	winhttp.originalWinHttpFreeProxyResultEx = GetProcAddress(originalDll, "WinHttpFreeProxyResultEx");
	winhttp.originalWinHttpFreeProxySettings = GetProcAddress(originalDll, "WinHttpFreeProxySettings");
	winhttp.originalWinHttpFreeProxySettingsEx = GetProcAddress(originalDll, "WinHttpFreeProxySettingsEx");
	winhttp.originalWinHttpFreeQueryConnectionGroupResult = GetProcAddress(originalDll, "WinHttpFreeQueryConnectionGroupResult");
	winhttp.originalWinHttpGetDefaultProxyConfiguration = GetProcAddress(originalDll, "WinHttpGetDefaultProxyConfiguration");
	winhttp.originalWinHttpGetIEProxyConfigForCurrentUser = GetProcAddress(originalDll, "WinHttpGetIEProxyConfigForCurrentUser");
	winhttp.originalWinHttpGetProxyForUrl = GetProcAddress(originalDll, "WinHttpGetProxyForUrl");
	winhttp.originalWinHttpGetProxyForUrlEx = GetProcAddress(originalDll, "WinHttpGetProxyForUrlEx");
	winhttp.originalWinHttpGetProxyForUrlEx2 = GetProcAddress(originalDll, "WinHttpGetProxyForUrlEx2");
	winhttp.originalWinHttpGetProxyForUrlHvsi = GetProcAddress(originalDll, "WinHttpGetProxyForUrlHvsi");
	winhttp.originalWinHttpGetProxyResult = GetProcAddress(originalDll, "WinHttpGetProxyResult");
	winhttp.originalWinHttpGetProxyResultEx = GetProcAddress(originalDll, "WinHttpGetProxyResultEx");
	winhttp.originalWinHttpGetProxySettingsEx = GetProcAddress(originalDll, "WinHttpGetProxySettingsEx");
	winhttp.originalWinHttpGetProxySettingsResultEx = GetProcAddress(originalDll, "WinHttpGetProxySettingsResultEx");
	winhttp.originalWinHttpGetProxySettingsVersion = GetProcAddress(originalDll, "WinHttpGetProxySettingsVersion");
	winhttp.originalWinHttpGetTunnelSocket = GetProcAddress(originalDll, "WinHttpGetTunnelSocket");
	winhttp.originalWinHttpOpen = GetProcAddress(originalDll, "WinHttpOpen");
	winhttp.originalWinHttpOpenRequest = GetProcAddress(originalDll, "WinHttpOpenRequest");
	winhttp.originalWinHttpPacJsWorkerMain = GetProcAddress(originalDll, "WinHttpPacJsWorkerMain");
	winhttp.originalWinHttpProbeConnectivity = GetProcAddress(originalDll, "WinHttpProbeConnectivity");
	winhttp.originalWinHttpProtocolCompleteUpgrade = GetProcAddress(originalDll, "WinHttpProtocolCompleteUpgrade");
	winhttp.originalWinHttpProtocolReceive = GetProcAddress(originalDll, "WinHttpProtocolReceive");
	winhttp.originalWinHttpProtocolSend = GetProcAddress(originalDll, "WinHttpProtocolSend");
	winhttp.originalWinHttpQueryAuthSchemes = GetProcAddress(originalDll, "WinHttpQueryAuthSchemes");
	winhttp.originalWinHttpQueryConnectionGroup = GetProcAddress(originalDll, "WinHttpQueryConnectionGroup");
	winhttp.originalWinHttpQueryDataAvailable = GetProcAddress(originalDll, "WinHttpQueryDataAvailable");
	winhttp.originalWinHttpQueryHeaders = GetProcAddress(originalDll, "WinHttpQueryHeaders");
	winhttp.originalWinHttpQueryHeadersEx = GetProcAddress(originalDll, "WinHttpQueryHeadersEx");
	winhttp.originalWinHttpQueryOption = GetProcAddress(originalDll, "WinHttpQueryOption");
	winhttp.originalWinHttpReadData = GetProcAddress(originalDll, "WinHttpReadData");
	winhttp.originalWinHttpReadDataEx = GetProcAddress(originalDll, "WinHttpReadDataEx");
	winhttp.originalWinHttpReadProxySettings = GetProcAddress(originalDll, "WinHttpReadProxySettings");
	winhttp.originalWinHttpReadProxySettingsHvsi = GetProcAddress(originalDll, "WinHttpReadProxySettingsHvsi");
	winhttp.originalWinHttpReceiveResponse = GetProcAddress(originalDll, "WinHttpReceiveResponse");
	winhttp.originalWinHttpRefreshProxySettings = GetProcAddress(originalDll, "WinHttpRefreshProxySettings");
	winhttp.originalWinHttpRegisterProxyChangeNotification = GetProcAddress(originalDll, "WinHttpRegisterProxyChangeNotification");
	winhttp.originalWinHttpResetAutoProxy = GetProcAddress(originalDll, "WinHttpResetAutoProxy");
	winhttp.originalWinHttpResolverGetProxyForUrl = GetProcAddress(originalDll, "WinHttpResolverGetProxyForUrl");
	winhttp.originalWinHttpSaveProxyCredentials = GetProcAddress(originalDll, "WinHttpSaveProxyCredentials");
	winhttp.originalWinHttpSendRequest = GetProcAddress(originalDll, "WinHttpSendRequest");
	winhttp.originalWinHttpSetCredentials = GetProcAddress(originalDll, "WinHttpSetCredentials");
	winhttp.originalWinHttpSetDefaultProxyConfiguration = GetProcAddress(originalDll, "WinHttpSetDefaultProxyConfiguration");
	winhttp.originalWinHttpSetOption = GetProcAddress(originalDll, "WinHttpSetOption");
	winhttp.originalWinHttpSetProxySettingsPerUser = GetProcAddress(originalDll, "WinHttpSetProxySettingsPerUser");
	winhttp.originalWinHttpSetSecureLegacyServersAppCompat = GetProcAddress(originalDll, "WinHttpSetSecureLegacyServersAppCompat");
	winhttp.originalWinHttpSetStatusCallback = GetProcAddress(originalDll, "WinHttpSetStatusCallback");
	winhttp.originalWinHttpSetTimeouts = GetProcAddress(originalDll, "WinHttpSetTimeouts");
	winhttp.originalWinHttpTimeFromSystemTime = GetProcAddress(originalDll, "WinHttpTimeFromSystemTime");
	winhttp.originalWinHttpTimeToSystemTime = GetProcAddress(originalDll, "WinHttpTimeToSystemTime");
	winhttp.originalWinHttpUnregisterProxyChangeNotification = GetProcAddress(originalDll, "WinHttpUnregisterProxyChangeNotification");
	winhttp.originalWinHttpWebSocketClose = GetProcAddress(originalDll, "WinHttpWebSocketClose");
	winhttp.originalWinHttpWebSocketCompleteUpgrade = GetProcAddress(originalDll, "WinHttpWebSocketCompleteUpgrade");
	winhttp.originalWinHttpWebSocketQueryCloseStatus = GetProcAddress(originalDll, "WinHttpWebSocketQueryCloseStatus");
	winhttp.originalWinHttpWebSocketReceive = GetProcAddress(originalDll, "WinHttpWebSocketReceive");
	winhttp.originalWinHttpWebSocketSend = GetProcAddress(originalDll, "WinHttpWebSocketSend");
	winhttp.originalWinHttpWebSocketShutdown = GetProcAddress(originalDll, "WinHttpWebSocketShutdown");
	winhttp.originalWinHttpWriteData = GetProcAddress(originalDll, "WinHttpWriteData");
	winhttp.originalWinHttpWriteProxySettings = GetProcAddress(originalDll, "WinHttpWriteProxySettings");
}
