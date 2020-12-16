using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Vector2 vertical;
    [SerializeField] private Vector2 horizontal;
    
    void OnPreRender()
    {
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, vertical.x, vertical.y),
            transform.position.y,
            Mathf.Clamp(transform.position.z, horizontal.x, horizontal.y));
    }
    void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += RenderPipelineManager_beginCameraRendering;
    }
    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= RenderPipelineManager_beginCameraRendering;
    }
    private void RenderPipelineManager_beginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        OnPreRender();
    }
    
}
