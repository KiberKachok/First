using Photon.Realtime;
using Sirenix.OdinInspector;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class MainLayoutMover : MonoBehaviour
{
    public float height = 10f;
    public float speed = 2f;
    public RectTransform targetRect;

    private Vector3 _destination;
    [ReadOnly, SerializeField] private Vector3 _upperBorder;
    [ReadOnly, SerializeField] private Vector3 _lowerBorder;
    
    private void Start()
    {
        _lowerBorder = targetRect.anchoredPosition3D;
        _upperBorder = _lowerBorder + new Vector3(0, height, 0);
    }

    public void OnStateChanged(ClientState state)
    {
        if (state == ClientState.Disconnected || state == ClientState.Disconnecting
                                              || state == ClientState.ConnectingToNameServer || state == ClientState.ConnectedToNameServer)
        {
            _destination = _lowerBorder;
        }
        else
        {
            _destination = _upperBorder;
        }
    }

    private void Update()
    {
        if (targetRect.anchoredPosition3D != _destination)
        {
            targetRect.anchoredPosition3D =
                Vector3.Lerp(targetRect.anchoredPosition3D, _destination, speed * Time.deltaTime);
        }
    }
}
