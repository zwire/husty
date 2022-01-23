## SkyWay C# wrapper (preview)

[SkyWay](https://webrtc.ecl.ntt.com/)はNTTコミュニケーションズが提供するWebRTCのSDKで、JavaScript・Android・iOSのAPIも公開されています。  
しかしそれだけだとデスクトップで開発するのにブラウザが必須となるため、REST APIの形で素の通信エンジンも公開されています。  
それが[SkyWay-WebRTC-Gateway](https://webrtc.ecl.ntt.com/documents/webrtc-gateway.html)です。  
  
### Usage
1. 当ソリューションをクローン。自分の環境で書くならNuGetからHusty.SkywayGatewayをインストール。
2. [SkyWay公式](https://webrtc.ecl.ntt.com/)でログインし、APIキーを取得。これをPeerのコンストラクタに入れる。盗まれると悪用されるかもなので間違ってもpublicリポジトリなどに上げないこと。
3. [公式のダウンロードページ](https://github.com/skyway/skyway-webrtc-gateway/releases)からexeを持ってくる。
4. そのexeを起動してから、当プログラムを実行する。もちろん相手がいないと通信できているかわからないので、IDを変えてプロセスを2つ立ててください。
* 正常終了しなかった場合はポートが塞がれっぱなしになるので、そのときはexeを一旦閉じる。
  
### Contents
* Peer ... 接続を司ってくれる。ここからDataChannelおよびMediaChannelを生成できます。
* DataChannel, DataStream ... 中身はUDPクライアントが動いているので、通常の通信と同じくバイナリ・文字列・JSONをやりとりすることが可能です。  
* MediaChannel ... 私の映像ストリーミングに関する知識が乏しいためちゃんとテストできていませんが、ポートは意図通りに開いているのでおそらくできています。  
* ライブラリの本体は[Husty.SkywayGateway](../../Lib/cs/Husty.SkywayGateway)にあります。間違いがあれば指摘ください。  
