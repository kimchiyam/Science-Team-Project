# 작업 기준

- 작업 파일은 Assets/Work/Jun 안에 두고 Main.unity에서 연결한다.
- 기능별로 끝나면 사용자 확인을 받고 다음 부분으로 넘어간다.
- 필요한 기능만 단순하고 읽기 쉽게 작성하고, 조절할 값은 Inspector에 노출한다.
- 메서드 위에는 역할을 설명하는 짧은 한국어 주석을 작성한다.
- 다른 담당 코드는 참조만 한다. 새 패키지나 복잡한 구조가 필요하면 먼저 확인받는다.
- 검증은 컴파일과 필수 연결 위주로 최소화하고, 시각적 체감은 사용자가 확인한다.
- 외부 도구 문제를 오래 우회하거나 반복 검사하지 않는다. 사용자가 더 빨리 처리할 수 있으면 부탁한다.
- 비용이 큰 조사와 반복 검증은 먼저 확인받는다. Git 커밋과 푸시는 요청받을 때만 한다.

## 허용된 예외

- CLI 연결용 com.unity.pipeline 재설치를 위한 Packages/manifest.json, packages-lock.json 갱신.
- 지형 가로 간격 조절을 위한 KSM/00.Scripts/MapCreateManager.cs의 간격 변수 두 개 추가. 기본값은 유지하고 Jun/Main에서만 설정.
- 빌드 전 로그 정리를 위한 MapCreateManager.cs, PotentialTracker.cs의 반복 Debug.Log 제거.
