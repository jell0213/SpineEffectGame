# SpineEffectGame

Unity 6 LTS（6000.0.62f1）基本 2D 專案骨架，使用 Built-in Render Pipeline。

## 開啟專案

1. 透過 Unity Hub 安裝 Unity Editor **6000.0.62f1**，並完成適用的授權啟用。
2. 在 Unity Hub 的 Projects 選擇 Add project from disk，指定本儲存庫根目錄。
3. 等待 Unity 完成套件解析與首次匯入；缺少的預設專案設定會由 Editor 產生。
4. 開啟 `Assets/Scenes/Main.unity`，按下 Play。

主場景已加入建置場景清單。按 Play 後，若未接上 Spine 角色，會自動建立名為 `man` 的示範人偶，有頭、身體、雙臂、雙手、雙腿與雙腳。這是 Unity Sprite 組成的骨架替代品，不是 Spine 素材。

按住任一部位可將該骨骼連同子骨骼微幅拖曳並放大；放開、切換視窗或停用互動元件即還原。`Game` 物件上的 `BoneDragGame` 可調整最大世界位移（預設 0.18）與縮放倍率（預設 1.08）。拖出原部位仍會保持選取直到放開。

使用舊版 `UnityEngine.Input` 滑鼠 API。若 Editor 提示輸入後端不相容，在 Player Settings > Active Input Handling 選擇 **Input Manager (Old)** 或 **Both**。

首次開啟後，請一併提交 Unity 產生的專案設定與 `Packages/packages-lock.json`；不要提交 Library、Temp 或 UserSettings。

## 驗證

雲端環境沒有 Unity Editor，因此尚未驗證 C# 編譯、匯入、Play Mode 或建置。靜態檔案檢查不代表遊戲已在 Unity 執行通過。

在有 Editor 與有效授權的環境可執行以下匯入檢查（將路徑替換成實際 Editor 執行檔）：

```bash
/path/to/Unity -batchmode -nographics -quit -projectPath "$PWD" -logFile /tmp/SpineEffectGame-import.log
```

確認程序成功結束並檢查日誌中的匯入、套件解析及編譯錯誤。Play Mode 與平台建置需另外在 Editor 驗證。

## Spine

依需求暫不放入 `man` 素材，目標素材版本為 **Spine 4.3.26**。尚未包含 Spine Unity Runtime。`SpineManInteraction.cs` 已參照官方 4.3 分支 API 撰寫，使用 `Bone.Pose`／`Bone.AppliedPose` 與 `ISkeletonRenderer` 回呼；目前由 `SPINE_UNITY` 條件編譯保護，尚未實際編譯驗證。官方 4.3 分支持續更新，日後匯入 Runtime 時仍需針對實際版本驗收。

1. 日後放入素材時，依 Spine 授權條件匯入官方 Spine Unity **4.3** Runtime，以及 **4.3.26** 匯出的角色 `.json`／`.skel.bytes`、`.atlas.txt` 與貼圖。此處 4.3.26 是 Spine 編輯器／素材版本，不是 Unity Editor 版本。
2. 在 Player Settings > Scripting Define Symbols 加入 `SPINE_UNITY`，等候編譯完成。此符號是本專案定義，請勿在尚未安裝 Runtime 時啟用。
3. 從角色的 SkeletonDataAsset 建立場景物件，名稱設為 `man`，含 4.3 的 `SkeletonRenderer` 與 `SkeletonAnimation`；使用世界座標角色，不是 `SkeletonGraphic` UI。互動使用主執行緒更新，請勿啟用 Spine 的多執行緒動畫更新功能。
4. 將 `SpineManInteraction` 加到同一物件，並將此元件指派到 `Game > BoneDragGame > Man`。在場景中啟用角色後按 Play，即不會產生替代人偶。
5. 調整 `Hit Radius`（世界單位）及 `Interactive Bones`。空清單允許所有啟用的非 root 骨骼；實際素材建議填入可操作部位的骨骼名稱，排除 IK、裝飾與控制用骨骼。

點選以骨骼起點到長度末端的線段距離判定，不是貼圖像素或 Spine BoundingBoxAttachment；重疊時選取最近的骨骼。互動會在 `SkeletonAnimation.UpdateLocal` 加入偏移與縮放，於 `UpdateComplete` 還原骨骼 local 值，保留當幀的 world pose 供渲染，避免未設動畫關鍵格的屬性逐幀累積。放開後回到當前動畫姿勢，而非強制重設 setup pose。受 IK／transform constraints 控制的骨骼可能覆蓋此變形，需依素材選用合適的控制骨骼。

## 手動驗收

- 進入 Play Mode 後，執行 **Tools > SpineEffectGame > Run Demo Interaction Checks (Play Mode)**，可檢查部位選取、反覆拖曳不累加、還原、子骨骼繼承與停用還原。此檢查尚未在目前雲端執行，且不代替滑鼠輸入與真正 Spine 素材驗收。
- 開啟 Main 並 Play，確認人偶 `man`、說明文字與攝影機正常，Console 無錯誤。
- 分別按住頭、手、腿並拖曳：選定骨骼及其子骨骼跟隨，位移不超過設定值、縮放不會逐幀增大。
- 拖出部位外後放開，以及按住時切換視窗，確認位置與比例還原。
- 點背景不應操作任何骨骼；重複按住／放開不得累積位移。
- 接上真正 Spine 角色後重做上述項目，並測試播放動畫時的還原與所用 constraints。
