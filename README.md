# PersonalProject
물류창고 ai가 되어보는 게임

스크립트 구조도 및 역할

1. 게임 매니저 계층
GameManager
역할: 게임 전체 상태 관리, 스테이지 진행, 승리 조건 체크
영향 관계: 모든 매니저들을 총괄하며, StageManager와 UIManager에게 게임 상태 변화를 알림
주요 기능: 게임 시작/종료, 스테이지 클리어 체크, 전체 게임 루프 관리

TimeManager
역할: 게임 시간 흐름 제어, 재생/일시정지/리셋 기능
영향 관계: RobotController들의 움직임 제어, RecordingManager의 기록 재생 제어, UIManager의 시간바 업데이트
주요 기능: 시간 속도 조절, 시간 점프, 최대 시간 제한 관리

StageManager
역할: 스테이지별 목표 설정, 물품 배치, 스테이지 데이터 관리
영향 관계: ItemManager에게 아이템 생성 지시, GameManager에게 클리어 상태 보고
주요 기능: 스테이지 로드, 목표 위치 설정, 클리어 조건 체크

2. 로봇 제어 계층
RobotSelectionManager
역할: 키보드 입력(1,2,3,4...)을 통한 로봇 선택 관리
영향 관계: 현재 선택된 RobotController에게 활성화 신호 전송, UIManager에게 선택 상태 알림
주요 기능: 로봇 활성화/비활성화, 선택 상태 추적

RobotController
역할: 개별 로봇의 이동, 상호작용, 물품 운반 제어
영향 관계: TimeManager의 시간 상태에 따라 움직임 제어, RecordingManager에 행동 데이터 전송, ItemManager와 상호작용
주요 기능: 키보드 입력 처리, 물리 이동, 아이템 픽업/드롭

RobotAnimationController
역할: 로봇의 애니메이션 및 시각적 효과 관리
영향 관계: RobotController의 상태 변화에 따라 애니메이션 재생
주요 기능: 이동/정지/작업 애니메이션, 선택 시각 효과

3. 기록 및 재생 계층
RecordingManager
역할: 모든 로봇의 행동을 시간대별로 기록하고 재생
영향 관계: RobotController들로부터 행동 데이터 수집, TimeManager의 시간 변화에 따라 기록된 행동 재생
주요 기능: 위치/회전/행동 기록, 시간대별 데이터 저장, 재생 시 로봇 상태 복원

TimelineData
역할: 시간대별 로봇 상태 데이터 구조체/클래스
영향 관계: RecordingManager에서 생성/관리, RobotController 상태 복원에 사용
주요 기능: 위치, 회전, 아이템 보유 상태, 행동 타입 저장

4. 아이템 및 상호작용 계층
ItemManager
역할: 창고 내 모든 아이템의 생성, 배치, 상태 관리
영향 관계: StageManager로부터 배치 정보 받음, RobotController와 픽업/드롭 상호작용
주요 기능: 아이템 생성/제거, 목표 위치 체크, 정리 완료 검증

WarehouseItem
역할: 개별 아이템의 속성과 상태 관리
영향 관계: ItemManager에 의해 관리, RobotController와 직접 상호작용
주요 기능: 아이템 타입, 목표 위치, 현재 상태 저장

InteractionZone
역할: 픽업/드롭 가능 영역, 목표 지점 등의 상호작용 구역 관리
영향 관계: RobotController의 트리거 감지, ItemManager에게 상호작용 알림
주요 기능: 충돌 감지, 상호작용 가능 여부 판단

5. UI 및 시각 계층
UIManager
역할: 게임 UI 전체 관리 (시간바, 로봇 선택 표시, 스테이지 정보 등)
영향 관계: TimeManager로부터 시간 정보 받아 시간바 업데이트, RobotSelectionManager로부터 선택 상태 받음
주요 기능: 시간바 렌더링, 로봇 선택 UI, 스테이지 목표 표시

TimelineBar
역할: 화면 하단 시간바의 렌더링과 클릭 이벤트 처리
영향 관계: TimeManager에게 시간 점프 요청, UIManager에 의해 관리
주요 기능: 시간바 그리기, 클릭한 시간으로 점프, 현재 시간 표시

CameraController
역할: 시점 변경, 선택된 로봇 추적, 카메라 움직임 관리
영향 관계: RobotSelectionManager의 로봇 선택에 따라 추적 대상 변경, TimeManager의 일시정지 상태와 무관하게 작동
주요 기능: 마우스 회전, 로봇 추적, 줌 인/아웃

6. 확장 가능 계층 (추후 추가 예정)
ObstacleManager
역할: 점프대, 부스터, 장애물 등의 특수 오브젝트 관리
영향 관계: RobotController와 상호작용, StageManager에 의해 배치
주요 기능: 특수 효과 적용, 로봇 상태 변경

※ 핵심 상호작용 흐름
게임 시작: GameManager → StageManager → ItemManager (아이템 배치)
로봇 선택: 키 입력 → RobotSelectionManager → RobotController 활성화
시간 제어: 키 입력(Q/R) → TimeManager → 모든 RobotController 상태 변경
행동 기록: RobotController 움직임 → RecordingManager → TimelineData 저장
시간 되감기: TimelineBar 클릭 → TimeManager → RecordingManager → 로봇 상태 복원