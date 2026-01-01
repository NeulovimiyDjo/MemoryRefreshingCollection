#include <windows.h>
#include <stdio.h>
#include <cstdint>



//=============={ left it here for __thiscall example
constexpr INT32 wow_SomeShitInnerPrintFuncOffset = 0x00788150; // 1.12.1

struct wow_Obj_t {
    int8_t pad828[828];
    int32_t f828 = 0x00000012;//0x00000000; // number of msg in chat?
    int32_t f832 = 0x00000080;
    int32_t f836 = 0x00000011;//0xFFFFFFFF; // number of msg in chat - 1?
    int32_t f840 = 0x00000011;//0x00000000; // number of msg in chat- 1?
    int32_t f844 = 0x00000000;
    int8_t pad852[4];
    int32_t f852 = 0x00000001;
    int8_t pad868[12];
    int32_t f868 = 0x42F00000;
    int32_t f872 = 0x40400000;
    int8_t pad916[40];
    int32_t* f916;
};

typedef void(__thiscall *wow_SomeShitInnerPrintFunc_t)(wow_Obj_t* obj, const char* msg, int32_t* a3, int32_t a4);
//==============} left it here for __thiscall example




constexpr INT32 wow_PrintFuncOffset = 0x00703F50; // 1.12.1

using wow_PrintFunc_t = void (__stdcall *) (
    int32_t msgType, // msg/event type? say=0x000000E4 yell=0x000000E9 system=0x000000EE
    const char* format,
    const char* msg,
    int32_t a4,
    int32_t a5,
    int32_t a6,
    int32_t a7,
    int32_t a8,
    int32_t a9,
    int32_t a10,
    int32_t a11,
    int32_t a12
);






void ShowMsg(const char* msg) {
    MessageBox(NULL, msg, "From Target Process", NULL);
}

void __stdcall DoWork(LPVOID param) {
    ShowMsg("ENTERED_DLL_WORKING_THREAD");

    wow_SomeShitInnerPrintFunc_t wow_SomeShitInnerPrintFunc = (wow_SomeShitInnerPrintFunc_t)wow_SomeShitInnerPrintFuncOffset;
    wow_PrintFunc_t wow_PrintFunc = (wow_PrintFunc_t)wow_PrintFuncOffset;


    while (!GetAsyncKeyState(0x31)) {
        if (GetAsyncKeyState(0x32)) {
            ShowMsg("2 pressed");
        }


        //=============={ left it here for __thiscall example
        if (GetAsyncKeyState(0x33)) {
            //wow_Obj_t obj;
            ////int32_t f916 = 0x161BF008;
            //int32_t f916 = 0x0DCDF08;
            //obj.f916 = &f916;

            wow_Obj_t* obj_addr = (wow_Obj_t*)0x100A8808; // same once logged in?

            int32_t a3 = 0x0019D1DC;//0xFFFFFFFF; // idk apparently it changes
            int32_t a4 = 0x00000001;

            // crashes wow unless you set a3 and obj_addr for each new wow process instance
            //wow_SomeShitInnerPrintFunc(obj_addr, "3 pressed", &a3, a4);
        }
        //==============} left it here for __thiscall example


        if (GetAsyncKeyState(0x34)) {
            int32_t aMsgType = 0x000000EE;
            int32_t aSomething = 0x0019F284;
            int32_t aFunc = 0x00882748;
            int32_t aZero = 0x00000000;

            wow_PrintFunc(
                aMsgType,
                "%s%s%s%s%s%s%d%d%s%d",
                "4 pressed",
                aFunc,//"Loka",
                aFunc,//"Common"
                aSomething,
                aFunc,
                aFunc,
                aZero,
                aZero,
                aFunc,
                aZero
            );
        }

        Sleep(100);
    }

    ShowMsg("EXITING_DLL_WORKING_THREAD");
}

BOOL APIENTRY DllMain(HMODULE hModule,
    DWORD  ul_reason_for_call,
    LPVOID lpReserved
)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        CreateThread(
            NULL, // default security attributes
            0, // use default stack size
            reinterpret_cast<LPTHREAD_START_ROUTINE>(DoWork), // thread function
            nullptr, // argument to thread function
            0, // use default creation flags
            nullptr); // returns the thread identifier
        break;
    }

    return TRUE;
}