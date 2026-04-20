using UnityEngine;
using Unity.Cinemachine;

[ExecuteInEditMode]
[SaveDuringPlay]
public class LockCameraY : CinemachineExtension
{
    [Tooltip("플레이어(Follow 타겟)를 기준으로 더해줄 Y축 높이(오프셋)")]
    public float yOffset = 2f; // 변수 이름을 역할에 맞게 변경했습니다.

    [Tooltip("락온 상태일 때만 켤지 여부")]
    public bool isLockedOn = false;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        // 락온 상태가 아니면 개입하지 않음
        if (!isLockedOn) return;

        // 카메라가 따라갈 대상(플레이어)이 할당되어 있지 않으면 실행하지 않음
        if (vcam.Follow == null) return;

        // 시네머신이 카메라의 위치(Body) 계산을 끝마친 직후에 실행
        // (만약 다른 설정에 의해 무시된다면 Stage.Finalize로 변경하세요)
        if (stage == CinemachineCore.Stage.Body)
        {
            Vector3 pos = state.RawPosition;

            //  핵심 변경 부분: 플레이어의 현재 Y 위치 + 원하는 높이
            pos.y = vcam.Follow.position.y + yOffset;

            state.RawPosition = pos;
        }
    }
}