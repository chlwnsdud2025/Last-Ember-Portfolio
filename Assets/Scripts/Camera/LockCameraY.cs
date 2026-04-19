using UnityEngine;
using Unity.Cinemachine;

[ExecuteInEditMode]
[SaveDuringPlay]
public class LockCameraY : CinemachineExtension
{
    [Tooltip("고정할 카메라의 Y축 세계 좌표(World Y)")]
    public float fixedYPosition = 2f;

    [Tooltip("락온 상태일 때만 켤지 여부")]
    public bool isLockedOn = false;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        // 락온 상태가 아니면 개입하지 않음
        if (!isLockedOn) return;

        // 시네머신이 카메라의 위치(Body) 계산을 끝마친 직후에 실행
        if (stage == CinemachineCore.Stage.Body)
        {
            Vector3 pos = state.RawPosition;
            pos.y = fixedYPosition; // 여기서 Y축을 2f로 강제 고정!
            state.RawPosition = pos;
        }
    }
}