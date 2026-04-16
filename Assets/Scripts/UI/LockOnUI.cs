using UnityEngine;
using UnityEngine.UI;

public class LockOnUI : MonoBehaviour
{
    public Image markerImage;       // 락온 이미지 (드래그 앤 드롭)
    public PlayerController player; // 플레이어 (드래그 앤 드롭)

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        markerImage.enabled = false;
    }

    private void LateUpdate()
    {
        // 락온 중이고 타겟이 있을 때만 표식을 표시
        if (player.isLockedOn && player.currentTarget != null)
        {
            // 3D 월드 좌표(LockOnPoint)를 2D 화면 좌표로 변환
            Vector3 screenPos = mainCamera.WorldToScreenPoint(player.currentTarget.position);

            // 카메라 앞에 있을 때만 그림
            if (screenPos.z > 0)
            {
                markerImage.enabled = true;
                markerImage.transform.position = screenPos;
            }
            else
            {
                markerImage.enabled = false;
            }
        }
        else
        {
            markerImage.enabled = false;
        }
    }
}