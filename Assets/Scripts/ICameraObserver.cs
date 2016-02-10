using UnityEngine;

public interface ICameraObserver {
    void OnCameraMove(Vector3 newCameraPosition);
}
