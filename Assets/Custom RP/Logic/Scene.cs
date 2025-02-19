﻿
using System.Collections;
using System.Collections.Generic;
using CustomURP;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Rendering;

namespace CustomPipeline
{
    public unsafe class Scene
    {
        private CustomRenderPipelineAsset _asset;
        private int _clusterCount = 0;
        
        public bool _gpuDriven { get; private set; } = false;
        ClusterAction _cluster = null;
        public ClusterAction Cluster { get => _cluster; set { _cluster = value; } }

        // VolumetircLightAction _volumetircLight = null;
        
        public VolumetircLightAction VolumeLight { get; set; }
        
        public Scene(CustomRenderPipelineAsset asset)
        {
            _asset = asset;
        }
        public void Awake()
        {
            int maxClusterCount = 0;
            // _cluster = _asset.ClusterShading._clusterAction;
            // // if (Application.isPlaying && _cluster != null)
            // if(_cluster != null)
            // {
            //     //todo: init cluster
            //     _cluster.Initialization(_asset);
            // }

            if (_cluster == null)
            {
                _cluster = _asset.ClusterShading._clusterAction;
                if (_cluster)
                {
                    _cluster.Initialization(_asset);
                    _gpuDriven = true;
                }
                else
                {
                    _gpuDriven = false;
                }
                
            }
            else
            {
                _cluster.Initialization(_asset);
                _gpuDriven = true;
            }

            // if (_volumetircLight == null)
            // {
            //     
            //     if (_asset.VolumetircLightAction != null)
            //     {
            //         _volumetircLight = _asset.VolumetircLightAction;
            //         _volumetircLight.Initialization(_asset);
            //     }
            // }
            // else
            // {
            //     _volumetircLight.Initialization(_asset);
            // }
        }

        public void SetClusterCullResult(CullingResults results)
        {
            // if (Application.isPlaying && _cluster != null && !_cluster._isInited)
            if(_cluster != null)
            {
                _cluster.SetCullResult(results);
            }
        }

        // public void SetVolumetricLightCamera(CustomRenderPipelineCamera camera)
        // {
        //     if (_volumetircLight == null) return;
        //     _volumetircLight.SetCamera(camera);
        // }
        
        public void BeginRendering(CustomRenderPipelineCamera camera, ref Command cmd)
        {
            // if (Application.isPlaying && _cluster != null && !_cluster._isInited)
            if (_cluster != null && _cluster.Enabled)
            // if(_cluster != null)
            {
                _cluster.BeginRendering(camera, ref cmd);
            }

            // if (_volumetircLight != null && _volumetircLight.Enabled)
            // {
            //     _volumetircLight.SetCamera(camera);
            //     _volumetircLight.BeginRendering(camera, ref cmd);
            // }
        }


        public void Tick(CustomRenderPipelineCamera camera, ref Command cmd)
        {
            // if (Application.isPlaying && _cluster != null && _cluster._isInited)
            // if (Application.isPlaying && _cluster != null )
            if (_cluster != null && _cluster.Enabled)
            // if(_cluster != null)
            {
                // _cluster.DebugCluster(camera._camera);
                _cluster.Tick(camera, ref cmd); 
            }

            CleanClusterShadow();
            
            // if (_volumetircLight != null && _volumetircLight.Enabled)
            // {
            //     _volumetircLight.Tick(camera, ref cmd);
            // }
        }

        public void EndRendering(CustomRenderPipelineCamera camera, ref Command cmd)
        {   
            // if (_volumetircLight != null && _volumetircLight.Enabled)
            // {
            //     _volumetircLight.EndRendering(camera, ref cmd);
            // }
        }

        public void CleanClusterShadow()
        {
            if (_cluster && _cluster.Enabled)
            {
                _cluster.CleanShadows();
            }
        }

        public void Dispose()
        {
            if (_cluster)
            {
                _cluster.Dispose();
                _cluster = null;
                _gpuDriven = false;
            }

            // if (_volumetircLight)
            // {
            //     _volumetircLight.Dispose();
            //     _volumetircLight = null;
            // }
        }

        public void SetState()
        {
            _gpuDriven = _clusterCount > 0;
        }
        
    }
}