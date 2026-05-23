import pybullet as p
import pybullet_data
import time
import math
import socket
import threading

# UDP 설정
UDP_IP   = "0.0.0.0"
UDP_PORT = 5005

# 공유 데이터
latest_data = {
    "leftTrigger": 0.0,
    "leftPos": [0.0, 0.5, 0.3],  # 기본 위치
}

def udp_receiver():
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    sock.bind((UDP_IP, UDP_PORT))
    print(f"UDP 수신 대기 중: {UDP_PORT}")

    while True:
        try:
            data, _ = sock.recvfrom(1024)
            values = list(map(float, data.decode().split(",")))
            latest_data["leftTrigger"] = values[0]
            latest_data["leftPos"]     = values[2:5]
        except Exception as e:
            print(f"수신 오류: {e}")

# UDP 스레드 시작
thread = threading.Thread(target=udp_receiver, daemon=True)
thread.start()

# PyBullet 시작
p.connect(p.GUI)
p.setAdditionalSearchPath(pybullet_data.getDataPath())
p.setGravity(0, 0, -9.8)
p.resetDebugVisualizerCamera(1.5, 45, -30, [0, 0, 0.3])

# 바닥 + 로봇 로드
p.loadURDF("plane.urdf")
robot = p.loadURDF("dofbot.urdf", basePosition=[0, 0, 0], useFixedBase=True,
                   flags=p.URDF_USE_SELF_COLLISION)

num_joints = p.getNumJoints(robot)
end_effector_index = num_joints - 1  # 마지막 관절 = 집게
print(f"관절 수: {num_joints}, 엔드이펙터: {end_effector_index}")

# VR 연동 토글
use_vr = p.addUserDebugParameter("VR 연동 ON/OFF", 0, 1, 0)

# 수동 테스트용 타겟 위치 슬라이더
tx = p.addUserDebugParameter("Target X", -0.3, 0.3, 0.0)
ty = p.addUserDebugParameter("Target Y",  0.0, 0.5, 0.3)
tz = p.addUserDebugParameter("Target Z",  0.0, 0.5, 0.3)

# 타겟 시각화 (빨간 구체)
target_visual = p.createVisualShape(p.GEOM_SPHERE, radius=0.02, rgbaColor=[1, 0, 0, 1])
target_body   = p.createMultiBody(0, -1, target_visual, [0, 0.3, 0.3])

while True:
    vr_mode = p.readUserDebugParameter(use_vr) > 0.5

    if vr_mode:
        target_pos = latest_data["leftPos"]
        grip       = latest_data["leftTrigger"]
    else:
        target_pos = [
            p.readUserDebugParameter(tx),
            p.readUserDebugParameter(ty),
            p.readUserDebugParameter(tz),
        ]
        grip = 0.0

    # 타겟 구체 위치 업데이트
    p.resetBasePositionAndOrientation(target_body, target_pos, [0, 0, 0, 1])

    # IK 계산
    joint_angles = p.calculateInverseKinematics(
        robot,
        end_effector_index,
        target_pos,
        maxNumIterations=100,
        residualThreshold=0.001
    )

    # 관절 1~5 적용
    for i in range(min(5, len(joint_angles))):
        p.setJointMotorControl2(
            robot, i,
            p.POSITION_CONTROL,
            targetPosition=joint_angles[i],
            force=50
        )

    # 집게 (joint6) 트리거로 제어
    p.setJointMotorControl2(
        robot, 5,
        p.POSITION_CONTROL,
        targetPosition=grip * math.pi,
        force=30
    )

    p.stepSimulation()
    time.sleep(1/240)