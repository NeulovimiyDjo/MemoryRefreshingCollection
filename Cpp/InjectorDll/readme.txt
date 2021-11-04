DllMsg - dll to inject, spams messages on attach detach etc;
DllHide - hides notepad.exe from Taskmgr.exe or procexp64.exe when injected in those;
DllHost - starts NetCore runitme and executes managed assembly inside target process;
Injector - most simple console injector;
InjectorUI - injector with some simple shit winapi UI;
InjectorLib - static library used by Injector and InjectorUI;
InjectorDll - dynamic library wrapping InjectorLib;
InjectorNetCoreConsole - netcore injector which uses exports from InjectorDll;
ManagedAssembly - most simple test netcore assembly to be execuded by DllHost,
it has to be published with appropriate configuration and selfcontained to folder;