using System.Text;
using System.Runtime.InteropServices;
using System;
using UnityEngine;

namespace TriLib
{
    /// <summary>
    /// Represents the internal Assimp library functions and helpers.
    /// @warning Do not modify!
    /// @note Documentation to be done.
    /// </summary>
    public static class AssimpInterop
    {
        #region DllImport
#if (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN) && (UNITY_EDITOR_64 || UNITY_64)
        public const string DllPath = "assimp64";
#elif (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN) && (!UNITY_EDITOR_64 && !UNITY_64)
        public const string DllPath = "assimp32";
#elif (UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX) && (UNITY_EDITOR_64 || UNITY_64)
        public const string DllPath = "libassimp64";
#elif (UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX) && (!UNITY_EDITOR_64 && !UNITY_64)
        public const string DllPath = "libassimp32";
#else
        public const string DllPath = "libassimp";
#endif
        #endregion
        #region Generated
        private const int MaxStringLength = 1024;
        private static readonly bool Is32Bits = IntPtr.Size == 4;
        private static readonly int IntSize = Is32Bits ? 4 : 8;

        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
        EntryPoint = "aiCreatePropertyStore")]
        public static extern IntPtr _aiCreatePropertyStore();
        public static IntPtr ai_CreatePropertyStore()
        {
            var result = _aiCreatePropertyStore();
            return result;
        }

        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
        EntryPoint = "aiReleasePropertyStore")]
        public static extern void _aiReleasePropertyStore(IntPtr ptrPropertyStore);
        public static void ai_CreateReleasePropertyStore(IntPtr ptrPropertyStore)
        {
            _aiReleasePropertyStore(ptrPropertyStore);
        }

        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
        EntryPoint = "aiImportFileExWithProperties")]
        public static extern IntPtr _aiImportFileEx(string filename, uint flags, IntPtr ptrFS, IntPtr ptrProps);
        public static IntPtr ai_ImportFileEx(string filename, uint flags, IntPtr ptrFS, IntPtr ptrProp)
        {
            var result = _aiImportFileEx(filename, flags, ptrFS, ptrProp);
            return result;
        }

        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
        EntryPoint = "aiSetImportPropertyInteger")]
        public static extern IntPtr _aiSetImportPropertyInteger(IntPtr ptrStore, StringBuilder name, int value);
        public static IntPtr ai_SetImportPropertyInteger(IntPtr ptrStore, string name, int value)
        {
            var stringBuffer = GetStringBuffer(name);
            var result = _aiSetImportPropertyInteger(ptrStore, stringBuffer, value);
            return result;
        }

        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
        EntryPoint = "aiSetImportPropertyFloat")]
        public static extern IntPtr _aiSetImportPropertyFloat(IntPtr ptrStore, StringBuilder name, float value);
        public static IntPtr ai_SetImportPropertyFloat(IntPtr ptrStore, string name, float value)
        {
            var stringBuffer = GetStringBuffer(name);
            var result = _aiSetImportPropertyFloat(ptrStore, stringBuffer, value);
            return result;
        }

        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
        EntryPoint = "aiSetImportPropertyString")]
        public static extern IntPtr _aiSetImportPropertyString(IntPtr ptrStore, StringBuilder name, IntPtr ptrValue);
        public static IntPtr ai_SetImportPropertyString(IntPtr ptrStore, string name, string value)
        {
            var stringBuffer = GetStringBuffer(name);
            var assimpStringBuffer = GetAssimpStringBuffer(value);
            var result = _aiSetImportPropertyString(ptrStore, stringBuffer, assimpStringBuffer);
            Marshal.FreeHGlobal(assimpStringBuffer);
            return result;
        }

        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiImportFile")]
        public static extern IntPtr _aiImportFile(string filename, uint flags);
        public static IntPtr ai_ImportFile(string filename, uint flags)
        {
            var result = _aiImportFile(filename, flags);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiReleaseImport")]
        public static extern void _aiReleaseImport(IntPtr scene);
        public static void ai_ReleaseImport(IntPtr scene)
        {
            _aiReleaseImport(scene);
        }
        //TODO: New interface
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
        EntryPoint = "aiGetExtensionList")]
        public static extern void _aiGetExtensionList(IntPtr ptrExtensionList);
        public static void ai_GetExtensionList(out string strExtensionList)
        {
            var array = new byte[MaxStringLength + IntSize];
            var buffer = LockGc(array);
            _aiGetExtensionList(buffer.AddrOfPinnedObject());
            buffer.Free();
            var length = Is32Bits ? BitConverter.ToInt32(array, 0) : BitConverter.ToInt64(array, 0);
            strExtensionList = Encoding.UTF8.GetString(array, IntSize, (int)length);
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
        EntryPoint = "aiGetErrorString")]
        public static extern IntPtr _aiGetErrorString();
        public static string ai_GetErrorString()
        {
            var result = _aiGetErrorString();
            return Marshal.PtrToStringAnsi(result);
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
        EntryPoint = "aiIsExtensionSupported")]
        public static extern bool _aiIsExtensionSupported(StringBuilder strExtension);
        public static bool ai_IsExtensionSupported(string strExtension)
        {
            var stringBuffer = GetStringBuffer(strExtension);
            return _aiIsExtensionSupported(stringBuffer);
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiScene_HasMaterials")]
        private static extern Boolean _aiScene_HasMaterials(IntPtr ptrScene);
        public static Boolean aiScene_HasMaterials(IntPtr ptrScene)
        {
            var result = _aiScene_HasMaterials(ptrScene);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiScene_GetNumMaterials")]
        private static extern UInt32 _aiScene_GetNumMaterials(IntPtr ptrScene);
        public static UInt32 aiScene_GetNumMaterials(IntPtr ptrScene)
        {
            var result = _aiScene_GetNumMaterials(ptrScene);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiScene_GetNumMeshes")]
        private static extern UInt32 _aiScene_GetNumMeshes(IntPtr ptrScene);
        public static UInt32 aiScene_GetNumMeshes(IntPtr ptrScene)
        {
            var result = _aiScene_GetNumMeshes(ptrScene);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiScene_GetNumAnimations")]
        private static extern UInt32 _aiScene_GetNumAnimations(IntPtr ptrScene);
        public static UInt32 aiScene_GetNumAnimations(IntPtr ptrScene)
        {
            var result = _aiScene_GetNumAnimations(ptrScene);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiScene_HasMeshes")]
        private static extern Boolean _aiScene_HasMeshes(IntPtr ptrScene);
        public static Boolean aiScene_HasMeshes(IntPtr ptrScene)
        {
            var result = _aiScene_HasMeshes(ptrScene);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiScene_HasAnimation")]
        private static extern Boolean _aiScene_HasAnimation(IntPtr ptrScene);
        public static Boolean aiScene_HasAnimation(IntPtr ptrScene)
        {
            var result = _aiScene_HasAnimation(ptrScene);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiScene_GetRootNode")]
        private static extern IntPtr _aiScene_GetRootNode(IntPtr ptrScene);
        public static IntPtr aiScene_GetRootNode(IntPtr ptrScene)
        {
            var result = _aiScene_GetRootNode(ptrScene);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiScene_GetMaterial")]
        private static extern IntPtr _aiScene_GetMaterial(IntPtr ptrScene, UInt32 uintIndex);
        public static IntPtr aiScene_GetMaterial(IntPtr ptrScene, UInt32 uintIndex)
        {
            var result = _aiScene_GetMaterial(ptrScene, uintIndex);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiScene_GetMesh")]
        private static extern IntPtr _aiScene_GetMesh(IntPtr ptrScene, UInt32 uintIndex);
        public static IntPtr aiScene_GetMesh(IntPtr ptrScene, UInt32 uintIndex)
        {
            var result = _aiScene_GetMesh(ptrScene, uintIndex);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiScene_GetAnimation")]
        private static extern IntPtr _aiScene_GetAnimation(IntPtr ptrScene, UInt32 uintIndex);
        public static IntPtr aiScene_GetAnimation(IntPtr ptrScene, UInt32 uintIndex)
        {
            var result = _aiScene_GetAnimation(ptrScene, uintIndex);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiNode_GetName")]
        private static extern IntPtr _aiNode_GetName(IntPtr ptrNode);
        public static String aiNode_GetName(IntPtr ptrNode)
        {
            var result = _aiNode_GetName(ptrNode);
            var resultConverted = ReadStringFromPointer(result);
            return resultConverted;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiNode_GetNumChildren")]
        private static extern UInt32 _aiNode_GetNumChildren(IntPtr ptrNode);
        public static UInt32 aiNode_GetNumChildren(IntPtr ptrNode)
        {
            var result = _aiNode_GetNumChildren(ptrNode);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiNode_GetNumMeshes")]
        private static extern UInt32 _aiNode_GetNumMeshes(IntPtr ptrNode);
        public static UInt32 aiNode_GetNumMeshes(IntPtr ptrNode)
        {
            var result = _aiNode_GetNumMeshes(ptrNode);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiNode_GetChildren")]
        private static extern IntPtr _aiNode_GetChildren(IntPtr ptrNode, UInt32 uintIndex);
        public static IntPtr aiNode_GetChildren(IntPtr ptrNode, UInt32 uintIndex)
        {
            var result = _aiNode_GetChildren(ptrNode, uintIndex);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiNode_GetMeshIndex")]
        private static extern UInt32 _aiNode_GetMeshIndex(IntPtr ptrNode, UInt32 uintIndex);
        public static UInt32 aiNode_GetMeshIndex(IntPtr ptrNode, UInt32 uintIndex)
        {
            var result = _aiNode_GetMeshIndex(ptrNode, uintIndex);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiNode_GetParent")]
        private static extern IntPtr _aiNode_GetParent(IntPtr ptrNode);
        public static IntPtr aiNode_GetParent(IntPtr ptrNode)
        {
            var result = _aiNode_GetParent(ptrNode);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiNode_GetTransformation")]
        private static extern IntPtr _aiNode_GetTransformation(IntPtr ptrNode);
        public static Matrix4x4 aiNode_GetTransformation(IntPtr ptrNode)
        {
            var result = _aiNode_GetTransformation(ptrNode);
            var resultArray = GetNewFloat16Array(result);
            var resultConverted = LoadMatrix4x4FromArray(resultArray);
            return resultConverted;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
        EntryPoint = "aiMaterial_IsEmbeddedTextureCompressed")]
        private static extern bool _aiMaterial_IsEmbeddedTextureCompressed(IntPtr ptrScene, UInt32 uintIndex);
        public static bool aiMaterial_IsEmbeddedTextureCompressed(IntPtr ptrScene, UInt32 uintIndex) {
            var result = _aiMaterial_IsEmbeddedTextureCompressed(ptrScene, uintIndex);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
        EntryPoint = "aiMaterial_GetEmbeddedTextureDataSize")]
        private static extern UInt32 _aiMaterial_GetEmbeddedTextureDataSize(IntPtr ptrScene, UInt32 uintIndex, bool boolCompressed);
        public static UInt32 aiMaterial_GetEmbeddedTextureDataSize(IntPtr ptrScene, UInt32 uintIndex, bool boolCompressed) {
            var result = _aiMaterial_GetEmbeddedTextureDataSize(ptrScene, uintIndex, boolCompressed);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
        EntryPoint = "aiMaterial_GetEmbeddedTextureData")]
        private static extern void _aiMaterial_GetEmbeddedTextureData(IntPtr ptrScene, IntPtr ptrData, UInt32 uintIndex, UInt32 uintSize);
        public static byte[] aiMaterial_GetEmbeddedTextureData(IntPtr ptrScene, UInt32 uintIndex, UInt32 uintSize) {
            var data = new byte[uintSize];
            var dataBuffer = LockGc(data);
            _aiMaterial_GetEmbeddedTextureData(ptrScene, dataBuffer.AddrOfPinnedObject(), uintIndex, uintSize);
            dataBuffer.Free();
            return data;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMaterial_GetTextureCount")]
        private static extern UInt32 _aiMaterial_GetTextureCount(IntPtr ptrMat, UInt32 uintType);
        public static UInt32 aiMaterial_GetTextureCount(IntPtr ptrMat, UInt32 uintType)
        {
            var result = _aiMaterial_GetTextureCount(ptrMat, uintType);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMaterial_HasTextureDiffuse")]
        private static extern Boolean _aiMaterial_HasTextureDiffuse(IntPtr ptrMat, UInt32 uintType);
        public static Boolean aiMaterial_HasTextureDiffuse(IntPtr ptrMat, UInt32 uintType)
        {
            var result = _aiMaterial_HasTextureDiffuse(ptrMat, uintType);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMaterial_GetTextureDiffuse")]
        private static extern Boolean _aiMaterial_GetTextureDiffuse(IntPtr ptrMat, UInt32 uintType,
            StringBuilder strPath, IntPtr uintMapping, IntPtr uintUvIndex, IntPtr floatBlend, IntPtr uintOp,
            IntPtr uintMapMode);
        public static Boolean aiMaterial_GetTextureDiffuse(IntPtr ptrMat, UInt32 uintType,
            out String strPath, out UInt32 uintMapping, out UInt32 uintUvIndex,
            out Single floatBlend, out UInt32 uintOp, out UInt32 uintMapMode)
        {
            StringBuilder strPathStringBuilder;
            var strPathBufferHandle = GetNewStringBuffer(out strPathStringBuilder);
            var uintMappingBufferHandle = GetNewUIntBuffer(out uintMapping);
            var uintUvIndexBufferHandle = GetNewUIntBuffer(out uintUvIndex);
            var floatBlendBufferHandle = GetNewFloatBuffer(out floatBlend);
            var uintOpBufferHandle = GetNewUIntBuffer(out uintOp);
            var uintMapModeBufferHandle = GetNewUIntBuffer(out uintMapMode);
            var result = _aiMaterial_GetTextureDiffuse(ptrMat, uintType, strPathStringBuilder,
                uintMappingBufferHandle.AddrOfPinnedObject(), uintUvIndexBufferHandle.AddrOfPinnedObject(),
                floatBlendBufferHandle.AddrOfPinnedObject(), uintOpBufferHandle.AddrOfPinnedObject(),
                uintMapModeBufferHandle.AddrOfPinnedObject());
            strPath = strPathStringBuilder.ToString();
            strPathBufferHandle.Free();
            uintMappingBufferHandle.Free();
            uintUvIndexBufferHandle.Free();
            floatBlendBufferHandle.Free();
            uintOpBufferHandle.Free();
            uintMapModeBufferHandle.Free();
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
        EntryPoint = "aiMaterial_GetNumTextureDiffuse")]
        private static extern uint _aiMaterial_GetNumTextureDiffuse(IntPtr ptrMat);
        public static uint aiMaterial_GetNumTextureDiffuse(IntPtr ptrMat)
        {
            var result = _aiMaterial_GetNumTextureDiffuse(ptrMat);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMaterial_HasTextureEmissive")]
        private static extern Boolean _aiMaterial_HasTextureEmissive(IntPtr ptrMat, UInt32 uintIndex);
        public static Boolean aiMaterial_HasTextureEmissive(IntPtr ptrMat, UInt32 uintIndex)
        {
            var result = _aiMaterial_HasTextureEmissive(ptrMat, uintIndex);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMaterial_GetTextureEmissive")]
        private static extern Boolean _aiMaterial_GetTextureEmissive(IntPtr ptrMat, UInt32 uintIndex,
            StringBuilder strPath, IntPtr uintMapping, IntPtr uintUvIndex, IntPtr floatBlend, IntPtr uintOp,
            IntPtr uintMapMode);
        public static Boolean aiMaterial_GetTextureEmissive(IntPtr ptrMat, UInt32 uintIndex,
            out String strPath, out UInt32 uintMapping, out UInt32 uintUvIndex,
            out Single floatBlend, out UInt32 uintOp, out UInt32 uintMapMode)
        {
            StringBuilder strPathStringBuilder;
            var strPathBufferHandle = GetNewStringBuffer(out strPathStringBuilder);
            var uintMappingBufferHandle = GetNewUIntBuffer(out uintMapping);
            var uintUvIndexBufferHandle = GetNewUIntBuffer(out uintUvIndex);
            var floatBlendBufferHandle = GetNewFloatBuffer(out floatBlend);
            var uintOpBufferHandle = GetNewUIntBuffer(out uintOp);
            var uintMapModeBufferHandle = GetNewUIntBuffer(out uintMapMode);
            var result = _aiMaterial_GetTextureEmissive(ptrMat, uintIndex, strPathStringBuilder,
                uintMappingBufferHandle.AddrOfPinnedObject(), uintUvIndexBufferHandle.AddrOfPinnedObject(),
                floatBlendBufferHandle.AddrOfPinnedObject(), uintOpBufferHandle.AddrOfPinnedObject(),
                uintMapModeBufferHandle.AddrOfPinnedObject());
            strPath = strPathStringBuilder.ToString();
            strPathBufferHandle.Free();
            uintMappingBufferHandle.Free();
            uintUvIndexBufferHandle.Free();
            floatBlendBufferHandle.Free();
            uintOpBufferHandle.Free();
            uintMapModeBufferHandle.Free();
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
        EntryPoint = "aiMaterial_GetNumTextureEmissive")]
        private static extern uint _aiMaterial_GetNumTextureEmissive(IntPtr ptrMat);
        public static uint aiMaterial_GetNumTextureEmissive(IntPtr ptrMat)
        {
            var result = _aiMaterial_GetNumTextureEmissive(ptrMat);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMaterial_HasTextureSpecular")]
        private static extern Boolean _aiMaterial_HasTextureSpecular(IntPtr ptrMat, UInt32 uintIndex);
        public static Boolean aiMaterial_HasTextureSpecular(IntPtr ptrMat, UInt32 uintIndex)
        {
            var result = _aiMaterial_HasTextureSpecular(ptrMat, uintIndex);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMaterial_GetTextureSpecular")]
        private static extern Boolean _aiMaterial_GetTextureSpecular(IntPtr ptrMat, UInt32 uintIndex,
        StringBuilder strPath, IntPtr uintMapping, IntPtr uintUvIndex, IntPtr floatBlend, IntPtr uintOp,
            IntPtr uintMapMode);
        public static Boolean aiMaterial_GetTextureSpecular(IntPtr ptrMat, UInt32 uintIndex,
            out String strPath, out UInt32 uintMapping, out UInt32 uintUvIndex,
            out Single floatBlend, out UInt32 uintOp, out UInt32 uintMapMode)
        {
            StringBuilder strPathStringBuilder;
            var strPathBufferHandle = GetNewStringBuffer(out strPathStringBuilder);
            var uintMappingBufferHandle = GetNewUIntBuffer(out uintMapping);
            var uintUvIndexBufferHandle = GetNewUIntBuffer(out uintUvIndex);
            var floatBlendBufferHandle = GetNewFloatBuffer(out floatBlend);
            var uintOpBufferHandle = GetNewUIntBuffer(out uintOp);
            var uintMapModeBufferHandle = GetNewUIntBuffer(out uintMapMode);
            var result = _aiMaterial_GetTextureSpecular(ptrMat, uintIndex, strPathStringBuilder,
            uintMappingBufferHandle.AddrOfPinnedObject(), uintUvIndexBufferHandle.AddrOfPinnedObject(),
            floatBlendBufferHandle.AddrOfPinnedObject(), uintOpBufferHandle.AddrOfPinnedObject(),
            uintMapModeBufferHandle.AddrOfPinnedObject());
            strPath = strPathStringBuilder.ToString();
            strPathBufferHandle.Free();
            uintMappingBufferHandle.Free();
            uintUvIndexBufferHandle.Free();
            floatBlendBufferHandle.Free();
            uintOpBufferHandle.Free();
            uintMapModeBufferHandle.Free();
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
        EntryPoint = "aiMaterial_GetNumTextureSpecular")]
        private static extern uint _aiMaterial_GetNumTextureSpecular(IntPtr ptrMat);
        public static uint aiMaterial_GetNumTextureSpecular(IntPtr ptrMat)
        {
            var result = _aiMaterial_GetNumTextureSpecular(ptrMat);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMaterial_HasTextureNormals")]
        private static extern Boolean _aiMaterial_HasTextureNormals(IntPtr ptrMat, UInt32 uintIndex);
        public static Boolean aiMaterial_HasTextureNormals(IntPtr ptrMat, UInt32 uintIndex)
        {
            var result = _aiMaterial_HasTextureNormals(ptrMat, uintIndex);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMaterial_GetTextureNormals")]
        private static extern Boolean _aiMaterial_GetTextureNormals(IntPtr ptrMat, UInt32 uintIndex,
            StringBuilder strPath, IntPtr uintMapping, IntPtr uintUvIndex, IntPtr floatBlend, IntPtr uintOp,
            IntPtr uintMapMode);
        public static Boolean aiMaterial_GetTextureNormals(IntPtr ptrMat, UInt32 uintIndex,
            out String strPath, out UInt32 uintMapping, out UInt32 uintUvIndex,
            out Single floatBlend, out UInt32 uintOp, out UInt32 uintMapMode)
        {
            StringBuilder strPathStringBuilder;
            var strPathBufferHandle = GetNewStringBuffer(out strPathStringBuilder);
            var uintMappingBufferHandle = GetNewUIntBuffer(out uintMapping);
            var uintUvIndexBufferHandle = GetNewUIntBuffer(out uintUvIndex);
            var floatBlendBufferHandle = GetNewFloatBuffer(out floatBlend);
            var uintOpBufferHandle = GetNewUIntBuffer(out uintOp);
            var uintMapModeBufferHandle = GetNewUIntBuffer(out uintMapMode);
            var result = _aiMaterial_GetTextureNormals(ptrMat, uintIndex, strPathStringBuilder,
                uintMappingBufferHandle.AddrOfPinnedObject(), uintUvIndexBufferHandle.AddrOfPinnedObject(),
                floatBlendBufferHandle.AddrOfPinnedObject(), uintOpBufferHandle.AddrOfPinnedObject(),
                uintMapModeBufferHandle.AddrOfPinnedObject());
            strPath = strPathStringBuilder.ToString();
            strPathBufferHandle.Free();
            uintMappingBufferHandle.Free();
            uintUvIndexBufferHandle.Free();
            floatBlendBufferHandle.Free();
            uintOpBufferHandle.Free();
            uintMapModeBufferHandle.Free();
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
        EntryPoint = "aiMaterial_GetNumTextureNormals")]
        private static extern uint _aiMaterial_GetNumTextureNormals(IntPtr ptrMat);
        public static uint aiMaterial_GetNumTextureNormals(IntPtr ptrMat)
        {
            var result = _aiMaterial_GetNumTextureNormals(ptrMat);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMaterial_HasTextureHeight")]
        private static extern Boolean _aiMaterial_HasTextureHeight(IntPtr ptrMat, UInt32 uintIndex);
        public static Boolean aiMaterial_HasTextureHeight(IntPtr ptrMat, UInt32 uintIndex)
        {
            var result = _aiMaterial_HasTextureHeight(ptrMat, uintIndex);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMaterial_GetTextureHeight")]
        private static extern Boolean _aiMaterial_GetTextureHeight(IntPtr ptrMat, UInt32 uintIndex,
            StringBuilder strPath, IntPtr uintMapping, IntPtr uintUvIndex, IntPtr floatBlend, IntPtr uintOp,
            IntPtr uintMapMode);
        public static Boolean aiMaterial_GetTextureHeight(IntPtr ptrMat, UInt32 uintIndex,
            out String strPath, out UInt32 uintMapping, out UInt32 uintUvIndex,
            out Single floatBlend, out UInt32 uintOp, out UInt32 uintMapMode)
        {
            StringBuilder strPathStringBuilder;
            var strPathBufferHandle = GetNewStringBuffer(out strPathStringBuilder);
            var uintMappingBufferHandle = GetNewUIntBuffer(out uintMapping);
            var uintUvIndexBufferHandle = GetNewUIntBuffer(out uintUvIndex);
            var floatBlendBufferHandle = GetNewFloatBuffer(out floatBlend);
            var uintOpBufferHandle = GetNewUIntBuffer(out uintOp);
            var uintMapModeBufferHandle = GetNewUIntBuffer(out uintMapMode);
            var result = _aiMaterial_GetTextureHeight(ptrMat, uintIndex, strPathStringBuilder,
                uintMappingBufferHandle.AddrOfPinnedObject(), uintUvIndexBufferHandle.AddrOfPinnedObject(),
                floatBlendBufferHandle.AddrOfPinnedObject(), uintOpBufferHandle.AddrOfPinnedObject(),
                uintMapModeBufferHandle.AddrOfPinnedObject());
            strPath = strPathStringBuilder.ToString();
            strPathBufferHandle.Free();
            uintMappingBufferHandle.Free();
            uintUvIndexBufferHandle.Free();
            floatBlendBufferHandle.Free();
            uintOpBufferHandle.Free();
            uintMapModeBufferHandle.Free();
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
        EntryPoint = "aiMaterial_GetNumTextureHeight")]
        private static extern uint _aiMaterial_GetNumTextureHeight(IntPtr ptrMat);
        public static uint aiMaterial_GetNumTextureHeight(IntPtr ptrMat)
        {
            var result = _aiMaterial_GetNumTextureHeight(ptrMat);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMaterial_HasAmbient")]
        private static extern Boolean _aiMaterial_HasAmbient(IntPtr ptrMat);
        public static Boolean aiMaterial_HasAmbient(IntPtr ptrMat)
        {
            var result = _aiMaterial_HasAmbient(ptrMat);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMaterial_GetAmbient")]
        private static extern Boolean _aiMaterial_GetAmbient(IntPtr ptrMat, IntPtr colorOut);
        public static Boolean aiMaterial_GetAmbient(IntPtr ptrMat, out Color colorOut)
        {
            float[] colorOutBuffer;
            var colorOutBufferHandle = GetNewFloat4Buffer(out colorOutBuffer);
            var result = _aiMaterial_GetAmbient(ptrMat, colorOutBufferHandle.AddrOfPinnedObject());
            colorOut = LoadColorFromArray(colorOutBuffer);
            colorOutBufferHandle.Free();
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMaterial_HasDiffuse")]
        private static extern Boolean _aiMaterial_HasDiffuse(IntPtr ptrMat);
        public static Boolean aiMaterial_HasDiffuse(IntPtr ptrMat)
        {
            var result = _aiMaterial_HasDiffuse(ptrMat);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMaterial_GetDiffuse")]
        private static extern Boolean _aiMaterial_GetDiffuse(IntPtr ptrMat, IntPtr colorOut);
        public static Boolean aiMaterial_GetDiffuse(IntPtr ptrMat, out Color colorOut)
        {
            float[] colorOutBuffer;
            var colorOutBufferHandle = GetNewFloat4Buffer(out colorOutBuffer);
            var result = _aiMaterial_GetDiffuse(ptrMat, colorOutBufferHandle.AddrOfPinnedObject());
            colorOut = LoadColorFromArray(colorOutBuffer);
            colorOutBufferHandle.Free();
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMaterial_HasSpecular")]
        private static extern Boolean _aiMaterial_HasSpecular(IntPtr ptrMat);
        public static Boolean aiMaterial_HasSpecular(IntPtr ptrMat)
        {
            var result = _aiMaterial_HasSpecular(ptrMat);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMaterial_GetSpecular")]
        private static extern Boolean _aiMaterial_GetSpecular(IntPtr ptrMat, IntPtr colorOut);
        public static Boolean aiMaterial_GetSpecular(IntPtr ptrMat, out Color colorOut)
        {
            float[] colorOutBuffer;
            var colorOutBufferHandle = GetNewFloat4Buffer(out colorOutBuffer);
            var result = _aiMaterial_GetSpecular(ptrMat, colorOutBufferHandle.AddrOfPinnedObject());
            colorOut = LoadColorFromArray(colorOutBuffer);
            colorOutBufferHandle.Free();
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMaterial_HasEmissive")]
        private static extern Boolean _aiMaterial_HasEmissive(IntPtr ptrMat);
        public static Boolean aiMaterial_HasEmissive(IntPtr ptrMat)
        {
            var result = _aiMaterial_HasEmissive(ptrMat);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMaterial_GetEmissive")]
        private static extern Boolean _aiMaterial_GetEmissive(IntPtr ptrMat, IntPtr colorOut);
        public static Boolean aiMaterial_GetEmissive(IntPtr ptrMat, out Color colorOut)
        {
            float[] colorOutBuffer;
            var colorOutBufferHandle = GetNewFloat4Buffer(out colorOutBuffer);
            var result = _aiMaterial_GetEmissive(ptrMat, colorOutBufferHandle.AddrOfPinnedObject());
            colorOut = LoadColorFromArray(colorOutBuffer);
            colorOutBufferHandle.Free();
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMaterial_HasName")]
        private static extern Boolean _aiMaterial_HasName(IntPtr ptrMat);
        public static Boolean aiMaterial_HasName(IntPtr ptrMat)
        {
            var result = _aiMaterial_HasName(ptrMat);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMaterial_GetName")]
        private static extern Boolean _aiMaterial_GetName(IntPtr ptrMat, StringBuilder strName);
        public static Boolean aiMaterial_GetName(IntPtr ptrMat, out String strName)
        {
            StringBuilder strNameStringBuilder;
            var strNameBufferHandle = GetNewStringBuffer(out strNameStringBuilder);
            var result = _aiMaterial_GetName(ptrMat, strNameStringBuilder);
            strName = strNameStringBuilder.ToString();
            strNameBufferHandle.Free();
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMaterial_HasBumpScaling")]
        private static extern Boolean _aiMaterial_HasBumpScaling(IntPtr ptrMat);
        public static Boolean aiMaterial_HasBumpScaling(IntPtr ptrMat)
        {
            var result = _aiMaterial_HasBumpScaling(ptrMat);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMaterial_GetBumpScaling")]
        private static extern Boolean _aiMaterial_GetBumpScaling(IntPtr ptrMat, IntPtr floatOut);
        public static Boolean aiMaterial_GetBumpScaling(IntPtr ptrMat, out Single floatOut)
        {
            var floatOutBufferHandle = GetNewFloatBuffer(out floatOut);
            var result = _aiMaterial_GetBumpScaling(ptrMat, floatOutBufferHandle.AddrOfPinnedObject());
            floatOutBufferHandle.Free();
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMaterial_HasShininess")]
        private static extern Boolean _aiMaterial_HasShininess(IntPtr ptrMat);
        public static Boolean aiMaterial_HasShininess(IntPtr ptrMat)
        {
            var result = _aiMaterial_HasShininess(ptrMat);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMaterial_GetShininess")]
        private static extern Boolean _aiMaterial_GetShininess(IntPtr ptrMat, IntPtr floatOut);
        public static Boolean aiMaterial_GetShininess(IntPtr ptrMat, out Single floatOut)
        {
            var floatOutBufferHandle = GetNewFloatBuffer(out floatOut);
            var result = _aiMaterial_GetShininess(ptrMat, floatOutBufferHandle.AddrOfPinnedObject());
            floatOutBufferHandle.Free();
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMaterial_HasShininessStrength")]
        private static extern Boolean _aiMaterial_HasShininessStrength(IntPtr ptrMat);
        public static Boolean aiMaterial_HasShininessStrength(IntPtr ptrMat)
        {
            var result = _aiMaterial_HasShininessStrength(ptrMat);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMaterial_GetShininessStrength")]
        private static extern Boolean _aiMaterial_GetShininessStrength(IntPtr ptrMat, IntPtr floatOut);
        public static Boolean aiMaterial_GetShininessStrength(IntPtr ptrMat, out Single floatOut)
        {
            var floatOutBufferHandle = GetNewFloatBuffer(out floatOut);
            var result = _aiMaterial_GetShininessStrength(ptrMat, floatOutBufferHandle.AddrOfPinnedObject());
            floatOutBufferHandle.Free();
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
        EntryPoint = "aiMaterial_HasOpacity")]
        private static extern Boolean _aiMaterial_HasOpacity(IntPtr ptrMat);
        public static Boolean aiMaterial_HasOpacity(IntPtr ptrMat)
        {
            var result = _aiMaterial_HasOpacity(ptrMat);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
        EntryPoint = "aiMaterial_GetOpacity")]
        private static extern Boolean _aiMaterial_GetOpacity(IntPtr ptrMat, IntPtr floatOut);
        public static Boolean aiMaterial_GetOpacity(IntPtr ptrMat, out Single floatOut)
        {
            var floatOutBufferHandle = GetNewFloatBuffer(out floatOut);
            var result = _aiMaterial_GetOpacity(ptrMat, floatOutBufferHandle.AddrOfPinnedObject());
            floatOutBufferHandle.Free();
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMesh_VertexCount")]
        private static extern UInt32 _aiMesh_VertexCount(IntPtr ptrMesh);
        public static UInt32 aiMesh_VertexCount(IntPtr ptrMesh)
        {
            var result = _aiMesh_VertexCount(ptrMesh);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMesh_HasNormals")]
        private static extern Boolean _aiMesh_HasNormals(IntPtr ptrMesh);
        public static Boolean aiMesh_HasNormals(IntPtr ptrMesh)
        {
            var result = _aiMesh_HasNormals(ptrMesh);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMesh_HasTangentsAndBitangents")]
        private static extern Boolean _aiMesh_HasTangentsAndBitangents(IntPtr ptrMesh);
        public static Boolean aiMesh_HasTangentsAndBitangents(IntPtr ptrMesh)
        {
            var result = _aiMesh_HasTangentsAndBitangents(ptrMesh);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMesh_HasTextureCoords")]
        private static extern Boolean _aiMesh_HasTextureCoords(IntPtr ptrMesh, UInt32 uintIndex);
        public static Boolean aiMesh_HasTextureCoords(IntPtr ptrMesh, UInt32 uintIndex)
        {
            var result = _aiMesh_HasTextureCoords(ptrMesh, uintIndex);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMesh_HasVertexColors")]
        private static extern Boolean _aiMesh_HasVertexColors(IntPtr ptrMesh, UInt32 uintIndex);
        public static Boolean aiMesh_HasVertexColors(IntPtr ptrMesh, UInt32 uintIndex)
        {
            var result = _aiMesh_HasVertexColors(ptrMesh, uintIndex);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMesh_GetVertex")]
        private static extern IntPtr _aiMesh_GetVertex(IntPtr ptrMesh, UInt32 uintIndex);
        public static Vector3 aiMesh_GetVertex(IntPtr ptrMesh, UInt32 uintIndex)
        {
            var result = _aiMesh_GetVertex(ptrMesh, uintIndex);
            var resultArray = GetNewFloat3Array(result);
            var resultConverted = LoadVector3FromArray(resultArray);
            return resultConverted;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMesh_GetNormal")]
        private static extern IntPtr _aiMesh_GetNormal(IntPtr ptrMesh, UInt32 uintIndex);
        public static Vector3 aiMesh_GetNormal(IntPtr ptrMesh, UInt32 uintIndex)
        {
            var result = _aiMesh_GetNormal(ptrMesh, uintIndex);
            var resultArray = GetNewFloat3Array(result);
            var resultConverted = LoadVector3FromArray(resultArray);
            return resultConverted;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMesh_GetTangent")]
        private static extern IntPtr _aiMesh_GetTangent(IntPtr ptrMesh, UInt32 uintIndex);
        public static Vector3 aiMesh_GetTangent(IntPtr ptrMesh, UInt32 uintIndex)
        {
            var result = _aiMesh_GetTangent(ptrMesh, uintIndex);
            var resultArray = GetNewFloat3Array(result);
            var resultConverted = LoadVector3FromArray(resultArray);
            return resultConverted;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMesh_GetBitangent")]
        private static extern IntPtr _aiMesh_GetBitangent(IntPtr ptrMesh, UInt32 uintIndex);
        public static Vector3 aiMesh_GetBitangent(IntPtr ptrMesh, UInt32 uintIndex)
        {
            var result = _aiMesh_GetBitangent(ptrMesh, uintIndex);
            var resultArray = GetNewFloat3Array(result);
            var resultConverted = LoadVector3FromArray(resultArray);
            return resultConverted;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMesh_GetTextureCoord")]
        private static extern IntPtr _aiMesh_GetTextureCoord(IntPtr ptrMesh, UInt32 uintChannel,
            UInt32 uintIndex);
        public static Vector2 aiMesh_GetTextureCoord(IntPtr ptrMesh, UInt32 uintChannel,
            UInt32 uintIndex)
        {
            var result = _aiMesh_GetTextureCoord(ptrMesh, uintChannel, uintIndex);
            var resultArray = GetNewFloat2Array(result);
            var resultConverted = LoadVector2FromArray(resultArray);
            return resultConverted;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMesh_GetVertexColor")]
        private static extern IntPtr _aiMesh_GetVertexColor(IntPtr ptrMesh, UInt32 uintChannel,
            UInt32 uintIndex);
        public static Color aiMesh_GetVertexColor(IntPtr ptrMesh, UInt32 uintChannel,
            UInt32 uintIndex)
        {
            var result = _aiMesh_GetVertexColor(ptrMesh, uintChannel, uintIndex);
            var resultArray = GetNewFloat4Array(result);
            var resultConverted = LoadColorFromArray(resultArray);
            return resultConverted;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMesh_GetMatrialIndex")]
        private static extern UInt32 _aiMesh_GetMatrialIndex(IntPtr ptrMesh);
        public static UInt32 aiMesh_GetMatrialIndex(IntPtr ptrMesh)
        {
            var result = _aiMesh_GetMatrialIndex(ptrMesh);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMesh_GetName")]
        private static extern IntPtr _aiMesh_GetName(IntPtr ptrMesh);
        public static String aiMesh_GetName(IntPtr ptrMesh)
        {
            var result = _aiMesh_GetName(ptrMesh);
            var resultConverted = ReadStringFromPointer(result);
            return resultConverted;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMesh_HasFaces")]
        private static extern Boolean _aiMesh_HasFaces(IntPtr ptrMesh);
        public static Boolean aiMesh_HasFaces(IntPtr ptrMesh)
        {
            var result = _aiMesh_HasFaces(ptrMesh);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMesh_GetNumFaces")]
        private static extern UInt32 _aiMesh_GetNumFaces(IntPtr ptrMesh);
        public static UInt32 aiMesh_GetNumFaces(IntPtr ptrMesh)
        {
            var result = _aiMesh_GetNumFaces(ptrMesh);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMesh_GetFace")]
        private static extern IntPtr _aiMesh_GetFace(IntPtr ptrMesh, UInt32 uintIndex);
        public static IntPtr aiMesh_GetFace(IntPtr ptrMesh, UInt32 uintIndex)
        {
            var result = _aiMesh_GetFace(ptrMesh, uintIndex);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMesh_HasBones")]
        private static extern Boolean _aiMesh_HasBones(IntPtr ptrMesh);
        public static Boolean aiMesh_HasBones(IntPtr ptrMesh)
        {
            var result = _aiMesh_HasBones(ptrMesh);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMesh_GetNumBones")]
        private static extern UInt32 _aiMesh_GetNumBones(IntPtr ptrMesh);
        public static UInt32 aiMesh_GetNumBones(IntPtr ptrMesh)
        {
            var result = _aiMesh_GetNumBones(ptrMesh);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiMesh_GetBone")]
        private static extern IntPtr _aiMesh_GetBone(IntPtr ptrMesh, UInt32 uintIndex);
        public static IntPtr aiMesh_GetBone(IntPtr ptrMesh, UInt32 uintIndex)
        {
            var result = _aiMesh_GetBone(ptrMesh, uintIndex);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiFace_GetNumIndices")]
        private static extern UInt32 _aiFace_GetNumIndices(IntPtr ptrFace);
        public static UInt32 aiFace_GetNumIndices(IntPtr ptrFace)
        {
            var result = _aiFace_GetNumIndices(ptrFace);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiFace_GetIndex")]
        private static extern UInt32 _aiFace_GetIndex(IntPtr ptrFace, UInt32 uintIndex);
        public static UInt32 aiFace_GetIndex(IntPtr ptrFace, UInt32 uintIndex)
        {
            var result = _aiFace_GetIndex(ptrFace, uintIndex);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiBone_GetName")]
        private static extern IntPtr _aiBone_GetName(IntPtr ptrBone);
        public static String aiBone_GetName(IntPtr ptrBone)
        {
            var result = _aiBone_GetName(ptrBone);
            var resultConverted = ReadStringFromPointer(result);
            return resultConverted;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiBone_GetNumWeights")]
        private static extern UInt32 _aiBone_GetNumWeights(IntPtr ptrBone);
        public static UInt32 aiBone_GetNumWeights(IntPtr ptrBone)
        {
            var result = _aiBone_GetNumWeights(ptrBone);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiBone_GetWeights")]
        private static extern IntPtr _aiBone_GetWeights(IntPtr ptrBone, UInt32 uintIndex);
        public static IntPtr aiBone_GetWeights(IntPtr ptrBone, UInt32 uintIndex)
        {
            var result = _aiBone_GetWeights(ptrBone, uintIndex);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiBone_GetOffsetMatrix")]
        private static extern IntPtr _aiBone_GetOffsetMatrix(IntPtr ptrBone);
        public static Matrix4x4 aiBone_GetOffsetMatrix(IntPtr ptrBone)
        {
            var result = _aiBone_GetOffsetMatrix(ptrBone);
            var resultArray = GetNewFloat16Array(result);
            var resultConverted = LoadMatrix4x4FromArray(resultArray);
            return resultConverted;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiVertexWeight_GetWeight")]
        private static extern Single _aiVertexWeight_GetWeight(IntPtr ptrVweight);
        public static Single aiVertexWeight_GetWeight(IntPtr ptrVweight)
        {
            var result = _aiVertexWeight_GetWeight(ptrVweight);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiVertexWeight_GetVertexId")]
        private static extern UInt32 _aiVertexWeight_GetVertexId(IntPtr ptrVweight);
        public static UInt32 aiVertexWeight_GetVertexId(IntPtr ptrVweight)
        {
            var result = _aiVertexWeight_GetVertexId(ptrVweight);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiAnimation_GetName")]
        private static extern IntPtr _aiAnimation_GetName(IntPtr ptrAnimation);
        public static String aiAnimation_GetName(IntPtr ptrAnimation)
        {
            var result = _aiAnimation_GetName(ptrAnimation);
            var resultConverted = ReadStringFromPointer(result);
            return resultConverted;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiAnimation_GetDuraction")]
        private static extern Single _aiAnimation_GetDuraction(IntPtr ptrAnimation);
        public static Single aiAnimation_GetDuraction(IntPtr ptrAnimation)
        {
            var result = _aiAnimation_GetDuraction(ptrAnimation);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiAnimation_GetTicksPerSecond")]
        private static extern Single _aiAnimation_GetTicksPerSecond(IntPtr ptrAnimation);
        public static Single aiAnimation_GetTicksPerSecond(IntPtr ptrAnimation)
        {
            var result = _aiAnimation_GetTicksPerSecond(ptrAnimation);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiAnimation_GetNumChannels")]
        private static extern UInt32 _aiAnimation_GetNumChannels(IntPtr ptrAnimation);
        public static UInt32 aiAnimation_GetNumChannels(IntPtr ptrAnimation)
        {
            var result = _aiAnimation_GetNumChannels(ptrAnimation);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiAnimation_GetNumMorphChannels")]
        private static extern UInt32 _aiAnimation_GetNumMorphChannels(IntPtr ptrAnimation);
        public static UInt32 aiAnimation_GetNumMorphChannels(IntPtr ptrAnimation)
        {
            var result = _aiAnimation_GetNumMorphChannels(ptrAnimation);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiAnimation_GetNumMeshChannels")]
        private static extern UInt32 _aiAnimation_GetNumMeshChannels(IntPtr ptrAnimation);
        public static UInt32 aiAnimation_GetNumMeshChannels(IntPtr ptrAnimation)
        {
            var result = _aiAnimation_GetNumMeshChannels(ptrAnimation);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiAnimation_GetAnimationChannel")]
        private static extern IntPtr _aiAnimation_GetAnimationChannel(IntPtr ptrAnimation, UInt32 uintIndex);
        public static IntPtr aiAnimation_GetAnimationChannel(IntPtr ptrAnimation, UInt32 uintIndex)
        {
            var result = _aiAnimation_GetAnimationChannel(ptrAnimation, uintIndex);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiNodeAnim_GetNodeName")]
        private static extern IntPtr _aiNodeAnim_GetNodeName(IntPtr ptrNodeAnim);
        public static String aiNodeAnim_GetNodeName(IntPtr ptrNodeAnim)
        {
            var result = _aiNodeAnim_GetNodeName(ptrNodeAnim);
            var resultConverted = ReadStringFromPointer(result);
            return resultConverted;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiNodeAnim_GetNumPositionKeys")]
        private static extern UInt32 _aiNodeAnim_GetNumPositionKeys(IntPtr ptrNodeAnim);
        public static UInt32 aiNodeAnim_GetNumPositionKeys(IntPtr ptrNodeAnim)
        {
            var result = _aiNodeAnim_GetNumPositionKeys(ptrNodeAnim);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiNodeAnim_GetNumRotationKeys")]
        private static extern UInt32 _aiNodeAnim_GetNumRotationKeys(IntPtr ptrNodeAnim);
        public static UInt32 aiNodeAnim_GetNumRotationKeys(IntPtr ptrNodeAnim)
        {
            var result = _aiNodeAnim_GetNumRotationKeys(ptrNodeAnim);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiNodeAnim_GetNumScalingKeys")]
        private static extern UInt32 _aiNodeAnim_GetNumScalingKeys(IntPtr ptrNodeAnim);
        public static UInt32 aiNodeAnim_GetNumScalingKeys(IntPtr ptrNodeAnim)
        {
            var result = _aiNodeAnim_GetNumScalingKeys(ptrNodeAnim);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiNodeAnim_GetPostState")]
        private static extern UInt32 _aiNodeAnim_GetPostState(IntPtr ptrNodeAnim);
        public static UInt32 aiNodeAnim_GetPostState(IntPtr ptrNodeAnim)
        {
            var result = _aiNodeAnim_GetPostState(ptrNodeAnim);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiNodeAnim_GetPreState")]
        private static extern UInt32 _aiNodeAnim_GetPreState(IntPtr ptrNodeAnim);
        public static UInt32 aiNodeAnim_GetPreState(IntPtr ptrNodeAnim)
        {
            var result = _aiNodeAnim_GetPreState(ptrNodeAnim);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiNodeAnim_GetPositionKey")]
        private static extern IntPtr _aiNodeAnim_GetPositionKey(IntPtr ptrNodeAnim, UInt32 uintIndex);
        public static IntPtr aiNodeAnim_GetPositionKey(IntPtr ptrNodeAnim, UInt32 uintIndex)
        {
            var result = _aiNodeAnim_GetPositionKey(ptrNodeAnim, uintIndex);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiNodeAnim_GetRotationKey")]
        private static extern IntPtr _aiNodeAnim_GetRotationKey(IntPtr ptrNodeAnim, UInt32 uintIndex);
        public static IntPtr aiNodeAnim_GetRotationKey(IntPtr ptrNodeAnim, UInt32 uintIndex)
        {
            var result = _aiNodeAnim_GetRotationKey(ptrNodeAnim, uintIndex);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiNodeAnim_GetScalingKey")]
        private static extern IntPtr _aiNodeAnim_GetScalingKey(IntPtr ptrNodeAnim, UInt32 uintIndex);
        public static IntPtr aiNodeAnim_GetScalingKey(IntPtr ptrNodeAnim, UInt32 uintIndex)
        {
            var result = _aiNodeAnim_GetScalingKey(ptrNodeAnim, uintIndex);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiVectorKey_GetTime")]
        private static extern Single _aiVectorKey_GetTime(IntPtr ptrVectorKey);
        public static Single aiVectorKey_GetTime(IntPtr ptrVectorKey)
        {
            var result = _aiVectorKey_GetTime(ptrVectorKey);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiVectorKey_GetValue")]
        private static extern IntPtr _aiVectorKey_GetValue(IntPtr ptrVectorKey);
        public static Vector3 aiVectorKey_GetValue(IntPtr ptrVectorKey)
        {
            var result = _aiVectorKey_GetValue(ptrVectorKey);
            var resultArray = GetNewFloat3Array(result);
            var resultConverted = LoadVector3FromArray(resultArray);
            return resultConverted;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiQuatKey_GetTime")]
        private static extern Single _aiQuatKey_GetTime(IntPtr ptrQuatKey);
        public static Single aiQuatKey_GetTime(IntPtr ptrQuatKey)
        {
            var result = _aiQuatKey_GetTime(ptrQuatKey);
            return result;
        }
        [DllImport(DllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall,
            EntryPoint = "aiQuatKey_GetValue")]
        private static extern IntPtr _aiQuatKey_GetValue(IntPtr ptrQuatKey);
        public static Quaternion aiQuatKey_GetValue(IntPtr ptrQuatKey)
        {
            var result = _aiQuatKey_GetValue(ptrQuatKey);
            var resultArray = GetNewFloat4Array(result);
            var resultConverted = LoadQuaternionFromArray(resultArray);
            return resultConverted;
        }
        #endregion
        #region Helpers
        private static GCHandle LockGc(object value)
        {
            return GCHandle.Alloc(value, GCHandleType.Pinned);
        }
        public static StringBuilder GetStringBuffer(string value)
        {
            var buffer = new StringBuilder(MaxStringLength);
            buffer.Append(value);
            return buffer;
        }
        //TODO: wrapper
        public static IntPtr GetAssimpStringBuffer(string value) {
            var offset = Is32Bits ? 4 : 8;
            var buffer = Marshal.AllocHGlobal(offset + value.Length); 
            if (Is32Bits) {
                Marshal.WriteInt32(buffer, value.Length);
            } {
                Marshal.WriteInt64(buffer, value.Length);
            }
            var bytes = Encoding.ASCII.GetBytes(value);
            Marshal.Copy(bytes, 0, new IntPtr(Is32Bits ? buffer.ToInt32() : buffer.ToInt64() + offset), value.Length);
            return buffer;
        }
        private static GCHandle GetNewStringBuffer(out StringBuilder stringBuilder)
        {
            stringBuilder = new StringBuilder(2048);
            return LockGc(stringBuilder);
        }
        private static GCHandle GetNewFloatBuffer(out float value)
        {
            value = new float();
            return LockGc(value);
        }
        private static GCHandle GetNewFloat2Buffer(out float[] array)
        {
            array = new float[2];
            return LockGc(array);
        }
        private static GCHandle GetNewFloat3Buffer(out float[] array)
        {
            array = new float[3];
            return LockGc(array);
        }
        private static GCHandle GetNewFloat4Buffer(out float[] array)
        {
            array = new float[4];
            return LockGc(array);
        }
        private static GCHandle GetNewFloat16Buffer(out float[] array)
        {
            array = new float[16];
            return LockGc(array);
        }
        private static GCHandle GetNewUIntBuffer(out uint value)
        {
            value = new uint();
            return LockGc(value);
        }
        private static float[] GetNewFloat2Array(IntPtr pointer)
        {
            var array = new float[2];
            Marshal.Copy(pointer, array, 0, 2);
            return array;
        }
        private static float[] GetNewFloat3Array(IntPtr pointer)
        {
            var array = new float[3];
            Marshal.Copy(pointer, array, 0, 3);
            return array;
        }
        private static float[] GetNewFloat4Array(IntPtr pointer)
        {
            var array = new float[4];
            Marshal.Copy(pointer, array, 0, 4);
            return array;
        }
        private static float[] GetNewFloat16Array(IntPtr pointer)
        {
            var array = new float[16];
            Marshal.Copy(pointer, array, 0, 16);
            return array;
        }
        private static string ReadStringFromPointer(IntPtr pointer)
        {
            return Marshal.PtrToStringAnsi(pointer);
        }
        private static Vector2 LoadVector2FromArray(float[] array)
        {
            return new Vector2(array[0], array[1]);
        }
        private static Vector3 LoadVector3FromArray(float[] array)
        {
            return new Vector3(array[0], array[1], array[2]);
        }
        private static Color LoadColorFromArray(float[] array)
        {
            return new Color(array[0], array[1], array[2], array[3]);
        }
        private static Quaternion LoadQuaternionFromArray(float[] array)
        {
            return new Quaternion(array[1], array[2], array[3], array[0]);
        }
        private static Matrix4x4 LoadMatrix4x4FromArray(float[] array)
        {
            var matrix = new Matrix4x4();
            matrix[0] = array[0];
            matrix[1] = array[4];
            matrix[2] = array[8];
            matrix[3] = array[12];
            matrix[4] = array[1];
            matrix[5] = array[5];
            matrix[6] = array[9];
            matrix[7] = array[13];
            matrix[8] = array[2];
            matrix[9] = array[6];
            matrix[10] = array[10];
            matrix[11] = array[14];
            matrix[12] = array[3];
            matrix[13] = array[7];
            matrix[14] = array[11];
            matrix[15] = array[15];
            return matrix;
        }
        #endregion
    }
}