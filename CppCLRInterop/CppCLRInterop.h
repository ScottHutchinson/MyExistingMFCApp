#pragma once

#ifdef WIN32

#ifdef _NG_DART_CLR_DLL_
#define DLLEXP_NG_DART_CLR	__declspec(dllexport)
#else
#define DLLEXP_NG_DART_CLR	__declspec(dllimport)
#endif

#else		// Not WIN32
#define DLLEXP_NG_DART_CLR
#endif

namespace CppCLRInterop {

    DLLEXP_NG_DART_CLR void LoadMsgTypeFiltersWindow(const unsigned short msgTypeID, const char* msgTypeName, const char* parentStructName);

}
