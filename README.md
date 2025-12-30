# DelRev 🎮

<div align="center">

**다양한 배경과 AI 몬스터들로부터 도망치며 생존하는 풀-3D 스텔스 서바이벌 게임**

[![Unity Version](https://img.shields.io/badge/Unity-2022.3.47f1-black?logo=unity)](https://unity.com)
[![Language](https://img.shields.io/badge/Language-C%23-239120?logo=c-sharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/License-MIT-blue)](LICENSE)
[![Status](https://img.shields.io/badge/Status-In%20Development-orange)]()

[게임 소개](#-게임-소개) • [주요-기능](#-주요-기능) • [설치-및-실행](#-설치-및-실행) • [프로젝트-구조](#-프로젝트-구조) • [개발-팀](#-개발-팀)

</div>

---

## 🎬 게임 소개

**DelRev**는 플레이어가 다양한 환경(회사, 공장, 유치원, 가족 주택 등)에서 고유한 AI를 가진 몬스터들로부터 도망치며 생존해야 하는 풀-3D 스텔스 서바이벌 게임입니다.

각 스테이지는 독특한 배경과 특화된 AI 몬스터들을 특징으로 하며, 플레이어의 스테미나 관리, 아이템 수집, 스텔스 전술이 필수적입니다.

**개발 기간:** 약 6개월  
**개발 엔진:** Unity 2022.3.47f1  
**개발 언어:** C#  

---

## ✨ 주요-기능

### 🎮 게임플레이
- **멀티 스테이지**: 5개의 고유한 배경(회사, 공장, 유치원, 가족 주택, 기타)
- **스텔스 메커닉**: 조명, 음성, 거리 감지를 통한 몬스터 탐지 시스템
- **스테미나 시스템**: 달리기와 숨기기 시 스테미나 소모 및 회복 메커닉
- **생존 시스템**: 
  - 체력 관리
  - 코인 수집
  - 회복 아이템 사용

### 🤖 AI 몬스터 시스템
각 스테이지별 독특한 몬스터들:

| 몬스터 | 특징 | 스테이지 |
|--------|------|--------|
| **Security A/B** | 경비원 AI, NavMesh 패트롤 | 회사 |
| **Turret Sentinel** | 감시탑, 범위 공격 | 공장 |
| **Welding Robot** | 용접 로봇, 추적 및 공격 | 공장 |
| **Drone Patrol** | 드론, 상공 감시 | 공장 |
| **SmartKid** | 지능형 아이, 수학 문제 출제 AI | 유치원 |
| **Teacher** | 선생님, 아이들 관리 | 유치원 |
| **Director** | 원장, 고급 패턴 | 유치원 |
| **Doll Monster** | 인형 몬스터, 특수 행동 | 유치원 |
| **Dog** | 개, 냄새 추적 | 가족 주택 |

### 🎨 시각 효과
- **Universal Render Pipeline (URP)**: 최적화된 그래픽 파이프라인
- **Volume Lighting**: 분위기 있는 조명 효과
- **고품질 3D 모델**: 다양한 에셋 라이브러리 활용

### 🔊 오디오
- 배경음악 및 효과음
- 장면별 음성 디렉션
- 영상 재생 기능 (Intro 영상 플레이어)

### 💾 게임 진행
- **세이브/로드 시스템**: 게임 진행 상황 저장
- **글로벌 스테이트**: 게임 상태 관리
- **로딩 씬**: 부드러운 씬 전환

---

## 🛠️ 기술 스택

### 엔진 & 프레임워크
- **Unity Engine** 2022.3.47f1 (LTS)
- **C# 9.0**

### 주요 패키지
| 패키지 | 버전 | 목적 |
|--------|------|------|
| Universal Render Pipeline | 14.0.11 | 그래픽 렌더링 |
| AI Navigation | 1.1.7 | NavMesh 기반 AI |
| TextMesh Pro | 3.0.9 | 고급 텍스트 렌더링 |
| Timeline | 1.7.6 | 시네마틱 연출 |
| Visual Scripting | 1.9.4 | 노드 기반 스크립팅 |

### 개발 도구
- **IDE**: Visual Studio / Rider
- **버전 관리**: Git
- **3D 모델링**: Blender, Maya (외부 에셋 포함)

---

## 📁 프로젝트-구조

```
DelRev/
├── Assets/
│   ├── 0.Scenes/                    # 게임 씬
│   │   ├── Intro.unity              # 인트로 씬
│   │   ├── GameStart.unity          # 스타트 화면
│   │   ├── Company.unity            # 스테이지 1: 회사
│   │   ├── Factory.unity            # 스테이지 2: 공장
│   │   ├── Kindergarten.unity       # 스테이지 3: 유치원
│   │   ├── FamilyHouse.unity        # 스테이지 4: 가족 주택
│   │   ├── GameOver.unity           # 게임 오버 씬
│   │   └── LoadingScene.unity       # 로딩 씬
│   │
│   ├── 1.Script/                    # 게임 로직 스크립트
│   │   ├── StartGame/               # 게임 시작 및 관리
│   │   │   ├── GlobalState.cs       # 전역 상태 관리
│   │   │   ├── StartSceneManager.cs # 시작 씬 관리
│   │   │   └── GameOverUI.cs        # 게임 오버 UI
│   │   │
│   │   ├── 1.Player/                # 플레이어 관련
│   │   │   ├── PlayerController.cs  # 플레이어 조작 (이동, 점프, 스테미나)
│   │   │   ├── PlayerInputBlocker.cs # 입력 제어
│   │   │   └── PlayerAnimation.cs   # 플레이어 애니메이션
│   │   │
│   │   ├── 2.Items/                 # 아이템 시스템
│   │   │   ├── Item.cs              # 기본 아이템
│   │   │   ├── SpeedBoost.cs        # 속도 부스트 아이템
│   │   │   ├── HealingItem.cs       # 회복 아이템
│   │   │   └── CoinUI.cs            # 코인 UI
│   │   │
│   │   ├── 3.Monster/               # AI 몬스터 시스템
│   │   │   ├── Monster.cs           # 기본 몬스터 클래스
│   │   │   ├── IDangerTarget.cs     # 위험 대상 인터페이스
│   │   │   │
│   │   │   ├── Factory/             # 공장 스테이지 몬스터
│   │   │   │   ├── Security_A.cs    # 경비원 A
│   │   │   │   ├── Security_B.cs    # 경비원 B
│   │   │   │   ├── TurretSentinel.cs # 감시탑
│   │   │   │   ├── WeldingRobot.cs  # 용접 로봇
│   │   │   │   ├── DronePatrol.cs   # 드론
│   │   │   │   └── Trap.cs          # 함정
│   │   │   │
│   │   │   ├── kindergarten/        # 유치원 스테이지 몬스터
│   │   │   │   ├── SmartKid/        # 똑똑한 아이 (수학 문제 AI)
│   │   │   │   │   ├── SmartKidAI.cs
│   │   │   │   │   ├── ProblemManager.cs
│   │   │   │   │   ├── MathProblemUI.cs
│   │   │   │   │   └── PlayerInputBlocker.cs
│   │   │   │   ├── Teacher.cs       # 선생님
│   │   │   │   ├── TeacherManager.cs # 선생님 관리
│   │   │   │   ├── Director.cs      # 원장
│   │   │   │   └── DollMonsterAI.cs # 인형 몬스터
│   │   │   │
│   │   │   └── FamilyHouse/
│   │   │       └── Dog.cs           # 개
│   │   │
│   │   ├── 4.loading/               # 로딩 시스템
│   │   │   └── SceneLoadingManager.cs
│   │   │
│   │   ├── Map/                     # 맵 관리
│   │   │   └── MapController.cs
│   │   │
│   │   ├── Navigation/              # 네비게이션
│   │   │   └── NavMeshManager.cs
│   │   │
│   │   ├── Save/                    # 세이브/로드 시스템
│   │   │   ├── SaveManager.cs
│   │   │   ├── SaveLoadUI.cs
│   │   │   └── NewGameInitializer.cs
│   │   │
│   │   ├── UI/                      # UI 시스템
│   │   │   ├── UIManager.cs
│   │   │   ├── PauseMenu.cs
│   │   │   └── HUDController.cs
│   │   │
│   │   ├── Settings/                # 게임 설정
│   │   │   └── GameSettings.cs
│   │   │
│   │   ├── Message/                 # 메시지/알림 시스템
│   │   │   └── MessageSystem.cs
│   │   │
│   │   ├── Bootstrapper.cs          # 게임 초기화
│   │   └── IntroVideoPlayer.cs      # 오프닝 영상 플레이어
│   │
│   ├── 2.Download/                  # 다운로드 콘텐츠
│   ├── 3.Sound/                     # 오디오 에셋
│   ├── 4.Hierarchy_group/           # 계층 정리
│   │
│   ├── Prefabs/                     # 프리팹
│   │   └── Player.prefab            # 플레이어 프리팹
│   │
│   ├── Resources/                   # 런타임 로드 리소스
│   ├── Settings/                    # 게임 설정 에셋
│   │
│   ├── [3D Assets]/                 # 외부 3D 모델 및 이펙트
│   │   ├── Gwangju_3D asset/
│   │   ├── Industrial building/
│   │   ├── Fire/
│   │   ├── VLights/
│   │   ├── ViapixGames/
│   │   ├── StylArts_B/
│   │   └── [기타 에셋들]/
│   │
│   ├── TextMesh Pro/                # TextMesh Pro 에셋
│   ├── UI/                          # UI 에셋
│   └── Fonts/                       # 폰트 에셋
│
├── Packages/
│   ├── manifest.json                # 패키지 의존성
│   └── packages-lock.json
│
├── ProjectSettings/
│   ├── ProjectVersion.txt           # Unity 버전: 2022.3.47f1
│   ├── ProjectSettings.asset        # 프로젝트 설정
│   ├── GraphicsSettings.asset       # 그래픽 설정
│   ├── AudioManager.asset           # 오디오 설정
│   ├── InputManager.asset           # 입력 설정
│   ├── TagManager.asset             # 태그 및 레이어
│   ├── QualitySettings.asset        # 품질 설정
│   └── [기타 설정들]
│
├── Logs/                            # 로그 파일
├── obj/                             # 빌드 중간 파일
├── UserSettings/                    # 사용자 설정
│
└── DelRev.sln                       # Visual Studio 솔루션

```

---

## 🎮 설치-및-실행

### 요구사항
- **Unity 2022.3.47f1 LTS** 이상
- **Visual Studio** 또는 **Rider** (C# 개발용)
- **최소 GPU**: NVIDIA GTX 1050 / AMD RX 560 (또는 동급)
- **메모리**: 8GB RAM 이상 권장

### 설치 단계

1. **프로젝트 클론**
   ```bash
   git clone https://github.com/hitori839/DelRev.git
   cd DelRev
   ```

2. **Unity Hub에서 프로젝트 열기**
   - Unity 2022.3.47f1 버전 필수
   - `/DelRev` 폴더를 선택하여 열기

3. **패키지 복구**
   - Unity가 자동으로 `Packages/manifest.json`에서 패키지 다운로드

4. **실행**
   - Play 버튼을 클릭하거나 `Ctrl+P` (또는 `Cmd+P`) 누르기

### 빌드

**윈도우/macOS 스탠드얼론:**
```
File > Build Settings > 플랫폼 선택 > Build
```

**안드로이드 / iOS:**
- 해당 SDK 설치 후 동일하게 빌드

---

## 🎮 게임플레이 가이드

### 기본 조작
| 키 | 기능 |
|-------|------|
| `WASD` | 이동 |
| `Space` | 점프 |
| `Shift` | 달리기 (스테미나 소모) |
| `Ctrl` | 숨기 (스테미나 소모) |
| `Mouse` | 카메라 회전 |
| `ESC` | 메뉴 |

### 생존 팁
1. **스테미나 관리**: 달리기와 숨기기는 스테미나를 소모합니다
2. **몬스터 회피**: 각 몬스터는 고유한 감지 범위를 가집니다
3. **아이템 수집**: 코인과 회복 아이템으로 생존율 향상
4. **환경 이용**: 조명 어둠과 장애물을 활용한 스텔스

---

## 🎨 주요 기술 구현

### 1. **AI 시스템**
- **NavMesh 기반 경로 찾기**: 효율적인 이동 경로 계산
- **상태 머신 (State Machine)**: 각 몬스터의 행동 상태 관리
- **시야각 및 거리 감지**: 플레이어 탐지 메커닉
- **다양한 AI 유형**: 추적, 패트롤, 특수 행동 등

### 2. **플레이어 시스템**
- **CharacterController 기반 이동**: 부드러운 캐릭터 이동
- **스테미나 시스템**: 동적 스태미나 회복/소모
- **애니메이션 상태 동기화**: 이동 상태와 애니메이션 연동
- **체력 관리**: 몬스터 충돌 시 피해 시스템

### 3. **게임 관리**
- **DontDestroyOnLoad 패턴**: 씬 전환 시 게임 상태 유지
- **글로벌 상태 관리**: `GlobalState` 싱글톤 패턴
- **세이브/로드**: 플레이어 진행 상황 저장

### 4. **그래픽 최적화**
- **URP (Universal Render Pipeline)**: 모바일-PC 크로스 플랫폼 최적화
- **Volume Lighting**: 동적 조명 효과

---

## 🤝 개발-팀

<table>
  <tr>
    <td align="center" width="200">
      <a href="https://github.com/yejin1029">
        <img src="https://github.com/yejin1029.png" width="90" alt="USERNAME1"/>
      </a>
      <br />
      <b>권예진</b>
      <br />
      Client Developer<br />UI/UX Designer
    </td>
    <td align="center" width="200">
      <a href="https://github.com/doyeon112">
        <img src="https://github.com/doyeon112.png" width="90" alt="USERNAME2"/>
      </a>
      <br />
      <b>김도연</b>
      <br />
      Graphic Designer<br/>
    </td>
    <td align="center" width="200">
      <a href="https://github.com/hitori839">
        <img src="https://github.com/hitori839.png" width="90" alt="USERNAME3"/>
      </a>
      <br />
      <b>김도현</b>
      <br />
      Client Developer<br/>
    </td>
    <td align="center" width="200">
      <a href="https://github.com/LeeBellHa">
        <img src="https://github.com/bell-ha.png" width="90" alt="USERNAME3"/>
      </a>
      <br />
      <b>이종하</b>
      <br />
      Client Developer<br/>Sound Design
    </td>
  </tr>
</table>

---

## 📊 프로젝트 통계

- **총 C# 스크립트**: 200+ 파일
- **게임 씬**: 8개
- **구현된 몬스터 AI**: 10+ 유형
- **아이템 종류**: 회복, 속도부스트, 코인 등
- **개발 기간**: ~6개월
- **코드 라인**: 10,000+ 라인

---

## 🏆 수상 내역

### 🥉 2025 RIEF-FESTA 캡스톤디자인 경진대회 (G7 부문)
- **수상:** 장려상  
- **주관:** 단국대학교 G-RISE 사업단  
- **선정:** 총 75팀 중 6팀 선정(대상, 우수, 장려)

<img src="award_2.jpg" alt="2025 RIEF-FESTA Award" width="400"/>

---

### 🥉 2025 단국대학교 SW중심대학 캡스톤 페스티벌
- **수상:** 장려상  
- **팀명:** Soulmate  
- **주관:** 단국대학교 SW중심대학사업단  
- **선정:** 총 100팀 중 15팀 선정(대상, 최우수, 우수, 장려)

<img src="award_1.png" alt="2025 SW Capstone Festival Award" width="400"/>

---

## 🐛 알려진 문제 및 개선사항

### 현재 개발 중인 기능
- [ ] 추가 난이도 레벨 (Easy, Normal, Hard)
- [ ] 네트워크 멀티플레이 (계획 중)
- [ ] 추가 몬스터 AI 유형
- [ ] 커스텀 키 매핑

### 최적화 계획
- 오브젝트 풀링 적용 예정
- 메모리 최적화 작업 진행 중

---

## 📝 라이선스

이 프로젝트는 **MIT License** 하에 배포됩니다.

```
MIT License

Copyright (c) 2024 hitori839

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", BASIS OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR
A PARTICULAR PURPOSE AND NONINFRINGEMENT.
```

---

## 🔗 추가 리소스

- **Unity 공식 문서**: https://docs.unity.com
- **URP 문서**: https://docs.unity.com/Manual/universal-render-pipeline
- **NavMesh 튜토리얼**: https://docs.unity.com/Manual/nav-mesh

---

## 📧 연락처 및 피드백

프로젝트에 대한 피드백, 버그 리포트, 기능 제안은 아래 방법으로 연락주세요:

- **GitHub Issues**: [Issues 제출](https://github.com/hitori839/DelRev/issues)
- **GitHub Discussions**: [토론 참여](https://github.com/hitori839/DelRev/discussions)

---

<div align="center">

**Made with ❤️ using Unity**

</div>
