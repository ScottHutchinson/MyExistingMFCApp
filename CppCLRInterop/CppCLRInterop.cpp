#include "pch.h"

#include "CppCLRInterop.h"

namespace CppCLRInterop {

    using namespace System;
    using namespace NG_DART_Elmish;

    void LoadMsgTypeFiltersWindow(const unsigned short msgTypeID, const char* msgTypeName, const char* parentStructName) {
        MsgTypeFilters::PublicAPI::LoadWindow(msgTypeID, gcnew String(msgTypeName), gcnew String(parentStructName));
    }

}
