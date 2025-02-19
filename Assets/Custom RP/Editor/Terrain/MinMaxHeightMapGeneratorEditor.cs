﻿using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace CustomURP
{
    public class MinMaxHeightMapGeneratorEditor
    {
        static ComputeShader _shader;
        const int patchMapSize = 1280;
        Texture2D _heightmap;
        string _dir;
        string _fileName;

        public MinMaxHeightMapGeneratorEditor(Texture2D heightmap)
        {
            _heightmap = heightmap;
        }

        RenderTexture CreateMinMaxHeightTexture(int texSize)
        {
            RenderTextureDescriptor desc = new RenderTextureDescriptor(texSize,texSize,RenderTextureFormat.ARGBFloat,0,1);
            desc.enableRandomWrite = true;
            desc.autoGenerateMips = false;
            var rt = RenderTexture.GetTemporary(desc);
            rt.filterMode = FilterMode.Point;
            // rt.isPowerOfTwo = false;
            rt.Create();
            return rt;
        }

        void CaluateGroupXY(int kernelIndex, int textureSize, out int groupX, out int groupY)
        {
            uint threadX, threadY, threadZ;
            Shader.GetKernelThreadGroupSizes(kernelIndex, out threadX, out threadY, out threadZ);
            groupX = (int)(textureSize / threadX);
            groupY = (int)(textureSize / threadY);
        }
        
        private void WaitRenderTexture(RenderTexture rt, System.Action<RenderTexture> callback)
        { 
            
            var graphicsFormat = GraphicsFormatUtility.GetGraphicsFormat(rt.format, true);
            TextureFormat format = GraphicsFormatUtility.GetTextureFormat(graphicsFormat);
            
            var request = AsyncGPUReadback.Request(rt, 0, format,(res)=>
            {
                callback(rt);
            });
            TerrainEditor.UpdateGpuAsyncRequest(request);    
        }

        void SaveMipTextures(List<RenderTexture> mipTextures)
        {
            for (var i = 0; i < mipTextures.Count; i++)
            {
                var path = GetMipTexPath(i);
                var tex = TerrainEditor.ConvertToTexture2D(mipTextures[i], TextureFormat.RGBAFloat);
                var bytes = tex.EncodeToPNG();
                System.IO.File.WriteAllBytes(path, bytes);
            }

            AssetDatabase.Refresh();
        }

        void EnsureDir()
        {
            var heightMapPath = AssetDatabase.GetAssetPath(_heightmap);
            var dir = System.IO.Path.GetDirectoryName(heightMapPath);
            _fileName = System.IO.Path.GetFileNameWithoutExtension(heightMapPath);
            // _dir = $"{dir}/Bounds{heightMapName}";
            _dir = $"{dir}/BoundsMaps";
            if (!System.IO.Directory.Exists(_dir))
            {
                System.IO.Directory.CreateDirectory(_dir);
            }
        }

        string GetMipTexPath(int mipIndex)
        {
            var path = $"{_dir}/{_fileName}_Bound_{mipIndex}.png";
            return path;
        }

        void GenerateMipMaps(int totalMips, List<RenderTexture> miptextures, System.Action cb)
        {
            GenerateMipMap(miptextures[miptextures.Count - 1], mipTex =>
            {
                miptextures.Add(mipTex);
                if (miptextures.Count < totalMips)
                {
                    GenerateMipMaps(totalMips, miptextures, cb);
                }
                else
                {
                    cb();
                }
            });
        }

        void GenerateMipMap(RenderTexture rt, System.Action<RenderTexture> cb)
        {
            var kernelIndex = 1;
            var reduceTex = CreateMinMaxHeightTexture(rt.width / 2);
            _shader.SetTexture(kernelIndex, ShaderParams._inTexId, rt);
            _shader.SetTexture(kernelIndex, ShaderParams._reduceTexId, reduceTex);
            int groupX, groupY;
            CaluateGroupXY(kernelIndex, reduceTex.width, out groupX, out groupY);
            _shader.Dispatch(kernelIndex, groupX, groupY, 1);
            WaitRenderTexture(reduceTex, cb);
        }

        void GeneratePatchMinMaxHeightTexMip0(System.Action<RenderTexture> cb)
        {
            int kernelIndex = 0;
            var minMaxHeightTex = CreateMinMaxHeightTexture(patchMapSize);
            int groupX, groupY;
            CaluateGroupXY(kernelIndex, patchMapSize, out groupX, out groupY);
            Shader.SetTexture(kernelIndex, ShaderParams._heightTexId, _heightmap);
            Shader.SetTexture(kernelIndex, ShaderParams._pathMinMaxHeighTexId, minMaxHeightTex);
            Shader.Dispatch(kernelIndex, groupX, groupY, 1);
            WaitRenderTexture(minMaxHeightTex, cb);
        }

        public void Generate()
        {
            EnsureDir();
            List<RenderTexture> textures = new List<RenderTexture>();
            GeneratePatchMinMaxHeightTexMip0(rt =>
            {
                textures.Add(rt);
                GenerateMipMaps(9, textures, () =>
                {
                    SaveMipTextures(textures);
                    foreach (var rt in textures)
                    {
                        RenderTexture.ReleaseTemporary(rt);
                    }
                });
            });
        }

        static ComputeShader Shader
        {
            get
            {
                if (!_shader)
                {
                    _shader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Custom RP/ShaderLibrary/ComputeShader/MinMaxHeights.compute");
                }
                return _shader;
            }
        }

        [MenuItem("Terrain/MinMaxHeightMapFromSelectedHeightMap")]
        public static void GenerateMinMaxHeightMapFromSelectedHeightMap()
        {
      
            if (Selection.activeObject is Texture2D heightmap)
            {
                var file = AssetDatabase.GetAssetPath(heightmap);
                new MinMaxHeightMapGeneratorEditor(heightmap).Generate();
            }
        }

        public static void GenerateMinMaxHeightMapFromSelectedHeightMapTest()
        {
            var heightmap = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Textures/Terrain/HeightMap.png");
            if (heightmap == null) return;
            
            new MinMaxHeightMapGeneratorEditor(heightmap).Generate();
        }
    };
};
