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

    public void Align(Transform target)
    {
        StartCoroutine(AlignRoutine(target));
    }

    IEnumerator AlignRoutine(Transform target)
    {
        while (Vector3.Distance(new Vector3(target.position.x, transform.position.y, target.position.z), transform.position) > 0.02)
        {
            transform.position = Vector3.Lerp(transform.position, new Vector3(target.position.x, transform.position.y, target.position.z), 10 * Time.deltaTime);
            yield return null;
        }
        //transform.position = new Vector3(target.position.x, transform.position.y, target.position.z);
    }
}
