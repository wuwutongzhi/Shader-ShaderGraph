using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PandaGrab : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        static string rt_name = "_PandaGrabTex";
        static int rt_ID = Shader.PropertyToID(rt_name);
        RTHandle rtHandle;

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor descriptor = new RenderTextureDescriptor(2560, 1440, RenderTextureFormat.DefaultHDR, 0);
            rtHandle = RTHandles.Alloc(descriptor, name: rt_name);
            ConfigureTarget(rtHandle); // Updated to use RTHandle
            cmd.ClearRenderTarget(true, true, Color.black);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("PandaPass");
            cmd.Blit(renderingData.cameraData.renderer.cameraColorTargetHandle, rtHandle.nameID);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            cmd.Release();
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            RTHandles.Release(rtHandle);
        }
    }

    CustomRenderPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass();
        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


