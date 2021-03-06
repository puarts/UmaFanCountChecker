# ウマ娘ファン数調べ太郎

## 概要

ウマ娘プリティーダービーにおいて、所属サークル内のメンバーのファン数の抽出を半自動化するためのツールです。獲得ファン数のノルマチェックなどに利用できます。
このツールが利用できるのはWindowsのDMM版のみです。スマホやmacOSでは利用できません。

## 基本的な使い方

1. Releaseページから最新バージョンのツールが入った.zip ファイルををダウンロードし、適当なフォルダに解凍します。  
https://github.com/puarts/UmaFanCountChecker/releases
2. フォルダ内に含まれる UmaFanCountChecker.exe を実行し、ツールを起動します(.NET Core 3.1 RuntimeがPCにインストールされていない場合、エラーが出ます。.NET Core 3.1 Runtimeをインストールしてから再度実行してください)  
![20210627_0346](https://user-images.githubusercontent.com/71858166/123530087-a239d300-d731-11eb-8738-152fc29d0465.jpg)
2. DMM版のウマ娘プリティーダービーを起動し、サークル画面に移動、サークルメニューから「サークル情報」を開きます  
![20210627_0347](https://user-images.githubusercontent.com/71858166/123530194-b7fbc800-d732-11eb-92ba-a854abcb3236.jpg)
4. サークル情報画面の総獲得ファン数などが表示されている領域をスクロールすると、自動的に名前と総獲得ファン数がテキスト化され、ツール上に表示されます  
![20210627_0349](https://user-images.githubusercontent.com/71858166/123530390-a1567080-d734-11eb-82ad-ca8a31475679.jpg)
5. 「一覧をコピー」ボタンをクリックすると、名前と総獲得ファン数がTSV形式でクリップボードにコピーされます。Excel等に表として貼り付ける事ができます。誤判定している項目はExcel等で編集して調整してください  
![20210627_0351](https://user-images.githubusercontent.com/71858166/123530459-918b5c00-d735-11eb-8a06-7879f884f2e1.jpg)


## 注意

文字認識の精度があまり高くないため、そこそこ誤判定が多いです。抽出し終わった後、目視でざっと間違いがないかは確認する事をお勧めします。以下を行うことで誤判定を減らす事はできます。

- ゲームが表示されているウインドウをできる限り大きくしておくと、誤判定が少なくなります
- サークル情報のスクロールはゆっくり行う方が誤判定が少なくなります
- 判定できないメンバーがいる場合は、そのメンバーをツール内のキャプチャ画面に表示した状態で、ゲーム画面のウインドウを拡縮すると判定される事があります
